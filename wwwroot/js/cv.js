/* kormokhoj – CV builder (live preview). Guests save to localStorage, job seekers save to the server. */
(function () {
  const root = document.getElementById('cv-app');
  if (!root) return;

  const guest = root.dataset.mode === 'guest';
  const saveUrl = root.dataset.saveUrl;
  const csrf = () => (document.querySelector('meta[name=csrf]') || {}).content || '';
  const esc = (s) =>
    String(s == null ? '' : s).replace(/[&<>"']/g, (c) =>
      ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

  const DEGREES = ['Honours', 'Degree', 'HSC', 'SSC'];
  const TEMPLATES = [['classic', 'Classic'], ['modern', 'Modern'], ['minimal', 'Minimal']];

  const blankItem = {
      edu: () => ({ deg: '', dept: '', inst: '', cgpa: '', year: '' }),
    exp: () => ({ role: '', org: '', period: '', desc: '' }),
    proj: () => ({ title: '', desc: '' }),
  };
  const LISTS = {
      // edu: { title: 'Education', sing: 'education', fields: [['deg', 'Education / Degree'], ['inst', 'Institute'], ['cgpa', 'CGPA / GPA'], ['year', 'Year']] },
      edu: {
          title: 'Education',
          sing: 'education',
          fields: [
              ['deg', 'Education / Degree'],
              ['dept', 'Department / Group'],
              ['inst', 'Institute'],
              ['cgpa', 'CGPA / GPA'],
              ['year', 'Year']
          ]
      },
    exp: { title: 'Experience', sing: 'experience', fields: [['role', 'Job title'], ['org', 'Company'], ['period', 'Period'], ['desc', 'Details', 1]] },
    proj: { title: 'Projects', sing: 'project', fields: [['title', 'Project title'], ['desc', 'Description', 1]] },
  };

  const blank = () => ({
    tpl: 'classic', name: '', email: '', phone: '', address: '', obj: '', skills: '',
    edu: [blankItem.edu()], exp: [blankItem.exp()], proj: [blankItem.proj()],
  });

  function normalize(c) {
    const d = Object.assign(blank(), c || {});
    Object.keys(LISTS).forEach((k) => {
      if (!Array.isArray(d[k]) || !d[k].length) d[k] = [blankItem[k]()];
    });
    return d;
  }

  let init = null;
  try { init = JSON.parse(document.getElementById('cv-init').textContent); } catch (e) { }

  let D;
  if (guest) {
    let g = null;
    try { g = JSON.parse(localStorage.getItem('jh_guest')); } catch (e) { }
    D = normalize(g);
  } else {
    D = normalize(init);
  }

  const $form = document.getElementById('cvf');
  const $prev = document.getElementById('cvp');

  /* ---------- form ---------- */
  function formHTML() {
    const I = (k, l, t) =>
      '<label>' + l + '</label><input type="' + (t || 'text') + '" data-f="' + k + '" value="' + esc(D[k]) + '">';

    const list = (k) => {
      const L = LISTS[k];
      return (
        '<h3 style="margin-top:18px">' + L.title + '</h3>' +
        D[k].map((it, i) =>
          '<div class="sub">' +
          L.fields.map(([f, l, ta]) => {
            const a = 'data-l="' + k + '" data-i="' + i + '" data-k="' + f + '"';
            let ctl;
            if (k === 'edu' && f === 'deg') {
              ctl = '<select ' + a + '><option value="">Select degree</option>' +
                DEGREES.map((v) => '<option value="' + v + '"' + (it[f] === v ? ' selected' : '') + '>' + v + '</option>').join('') +
                '</select>';
            } else if (ta) {
              ctl = '<textarea ' + a + '>' + esc(it[f]) + '</textarea>';
            } else {
              ctl = '<input type="text" ' + a + ' value="' + esc(it[f]) + '">';
            }
            return '<label>' + l + '</label>' + ctl;
          }).join('') +
          '<br><button type="button" class="btn bad sm" data-act="rm" data-l="' + k + '" data-i="' + i + '">Remove</button></div>'
        ).join('') +
        '<button type="button" class="btn alt sm" data-act="add" data-l="' + k + '">+ Add ' + L.sing + '</button>'
      );
    };

    return (
      '<label>Template</label><select data-f="tpl">' +
      TEMPLATES.map(([v, n]) => '<option value="' + v + '"' + (D.tpl === v ? ' selected' : '') + '>' + n + '</option>').join('') +
      '</select>' +
      I('name', 'Full name') + I('email', 'Email', 'email') + I('phone', 'Phone') + I('address', 'Address') +
      '<label>Career objective</label><textarea data-f="obj">' + esc(D.obj) + '</textarea>' +
      list('edu') + list('exp') +
      '<label>Skills (comma separated)</label><textarea data-f="skills">' + esc(D.skills) + '</textarea>' +
      list('proj')
    );
  }

  /* ---------- preview ---------- */
  function cvHTML(c) {
    const used = (a) => (a || []).filter((x) => Object.values(x).some((v) => v));
    const sec = (t, b) => (b ? '<h2>' + t + '</h2>' + b : '');
    const tpl = ['modern', 'minimal'].includes(c.tpl) ? c.tpl : 'classic';
    const contact = [c.email, c.phone, c.address].filter(Boolean).map(esc).join(' | ');

    // const edu = used(c.edu).map((x) =>
    //   '<div class="it"><b>' + esc(x.deg) + '</b><p>' + esc(x.inst) + ' ' +
      //   (x.cgpa ? '· CGPA/GPA: ' + esc(x.cgpa) : '') + ' ' + (x.year ? '(' + esc(x.year) + ')' : '') + '</p></div>').join('');
      const edu = used(c.edu).map((x) =>
          '<div class="it">' +
          '<b>' + esc(x.deg) + '</b>' +
          '<p>' +
          (x.dept ? esc(x.dept) + ' · ' : '') +
          esc(x.inst) +
          (x.cgpa ? ' · CGPA/GPA: ' + esc(x.cgpa) : '') +
          (x.year ? ' (' + esc(x.year) + ')' : '') +
          '</p>' +
          '</div>'
      ).join('');
    const exp = used(c.exp).map((x) =>
      '<div class="it"><b>' + esc(x.role) + '</b> – ' + esc(x.org) + ' <i>' + esc(x.period) + '</i><p>' + esc(x.desc) + '</p></div>').join('');
    const skills = (c.skills || '').split(',').map((s) => s.trim()).filter(Boolean)
      .map((s) => '<span>' + esc(s) + '</span>').join('');
    const proj = used(c.proj).map((x) =>
      '<div class="it"><b>' + esc(x.title) + '</b><p>' + esc(x.desc) + '</p></div>').join('');

    return (
      '<div class="cv t-' + tpl + '"><div class="cvh"><h1>' + esc(c.name || 'Your Name') + '</h1><p>' + contact + '</p></div>' +
      sec('Career Objective', c.obj ? '<p>' + esc(c.obj) + '</p>' : '') +
      sec('Education', edu) + sec('Experience', exp) +
      sec('Skills', skills ? '<div class="sk">' + skills + '</div>' : '') +
      sec('Projects', proj) + '</div>'
    );
  }

  const renderForm = () => ($form.innerHTML = formHTML());
  const renderPreview = () => ($prev.innerHTML = cvHTML(D));
  const persistGuest = () => { if (guest) { try { localStorage.setItem('jh_guest', JSON.stringify(D)); } catch (e) { } } };

  /* ---------- events ---------- */
  function onInput(e) {
    const t = e.target;
    if (t.dataset.f) D[t.dataset.f] = t.value;
    else if (t.dataset.l) D[t.dataset.l][+t.dataset.i][t.dataset.k] = t.value;
    else return;
    renderPreview();
    persistGuest();
  }
  $form.addEventListener('input', onInput);
  $form.addEventListener('change', onInput);

  $form.addEventListener('click', (e) => {
    const b = e.target.closest('[data-act]');
    if (!b) return;
    const k = b.dataset.l;
    if (b.dataset.act === 'add') {
      D[k].push(blankItem[k]());
    } else {
      D[k].splice(+b.dataset.i, 1);
      if (!D[k].length) D[k].push(blankItem[k]());
    }
    renderForm();
    renderPreview();
    persistGuest();
  });

  document.getElementById('cv-save').addEventListener('click', async () => {
    if (guest) {
      persistGuest();
      return toast('Saved in this browser. Register to attach it to applications.');
    }
    try {
      const r = await fetch(saveUrl, {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json', RequestVerificationToken: csrf() },
        body: JSON.stringify(D),
      });
      toast(r.ok ? 'CV saved. It will be attached when you apply.' : 'Could not save the CV.');
    } catch (e) { toast('Could not save the CV.'); }
  });
  document.getElementById('cv-pdf').addEventListener('click', () => pdfFrom($prev, D.name || 'cv'));
  document.getElementById('cv-print').addEventListener('click', () => window.print());

  renderForm();
  renderPreview();
})();
