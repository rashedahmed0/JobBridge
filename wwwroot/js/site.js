/* kormokhoj – shared front-end helpers (toast, modal, PDF, sortable/filterable tables) */
(function () {
  const $ = (s, r = document) => r.querySelector(s);
  const csrf = () => (document.querySelector('meta[name=csrf]') || {}).content || '';

  /* ---------- toast ---------- */
  window.toast = function (m) {
    const t = $('#toast');
    if (!t) return;
    t.textContent = m;
    t.style.display = 'block';
    clearTimeout(window.toast.h);
    window.toast.h = setTimeout(() => (t.style.display = 'none'), 2800);
  };

  /* ---------- PDF ---------- */
  window.pdfFrom = function (el, name) {
    if (!el) return;
    if (!window.html2pdf) {
      toast('PDF library blocked – using print dialog.');
      return window.print();
    }
    el.parentElement.style.padding = 0;
    html2pdf()
      .set({
        margin: 0,
        filename: (name || 'cv') + '.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true },
        jsPDF: { unit: 'px', format: [794, 1123], hotfixes: ['px_scaling'] },
      })
      .from(el.firstElementChild)
      .save()
      .then(() => (el.parentElement.style.padding = ''))
      .catch(() => {
        el.parentElement.style.padding = '';
        window.print();
      });
  };

  /* ---------- modal (applicant CV) ---------- */
  function openModal(html) {
    $('#modal').innerHTML =
      '<div class="ov"><div class="md">' + html +
      '<div style="margin-top:14px"><button class="btn alt" type="button" data-close>Close</button></div></div></div>';
  }
  const closeModal = () => ($('#modal').innerHTML = '');

  document.addEventListener('click', async (e) => {
    const t = e.target;
    if (t.closest('[data-close]') || (t.classList && t.classList.contains('ov'))) return closeModal();

    const cvBtn = t.closest('[data-cv-url]');
    if (cvBtn) {
      try {
        const r = await fetch(cvBtn.dataset.cvUrl, { credentials: 'same-origin' });
        if (!r.ok) throw new Error();
        openModal(await r.text());
      } catch (_) { toast('Could not load the CV.'); }
      return;
    }

    const pdfBtn = t.closest('[data-pdf]');
    if (pdfBtn) return pdfFrom($(pdfBtn.dataset.pdf), pdfBtn.dataset.name || 'cv');
  });

  /* ---------- confirm on destructive forms ---------- */
  document.addEventListener('submit', (e) => {
    const m = e.target.dataset && e.target.dataset.confirm;
    if (m && !confirm(m)) e.preventDefault();
  });

  /* ---------- applicant status dropdown ---------- */
  document.addEventListener('change', async (e) => {
    const s = e.target.closest && e.target.closest('select[data-status-url]');
    if (!s) return;
    const td = s.closest('td');
    if (td) td.dataset.v = s.value;
    try {
      const r = await fetch(s.dataset.statusUrl, {
        method: 'POST',
        credentials: 'same-origin',
        headers: {
          RequestVerificationToken: csrf(),
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({ status: s.value }),
      });
      toast(r.ok ? 'Status updated' : 'Could not update status');
    } catch (_) { toast('Could not update status'); }
  });

  /* ---------- sortable / filterable tables (table.gt) ---------- */
  function initGrid(t) {
    const body = t.tBodies[0];
    const rows = [...body.rows];
    const filters = [...t.querySelectorAll('[data-col]')];
    const heads = [...t.querySelectorAll('thead th[data-sort]')];
    const count = document.querySelector('[data-count-for="' + t.id + '"]');
    const clear = document.querySelector('[data-clear-for="' + t.id + '"]');
    const cols = t.querySelector('thead tr').cells.length;
    let sort = null;

    const val = (r, c) => {
      const td = r.cells[c];
      return (td.dataset.v !== undefined ? td.dataset.v : td.textContent).trim();
    };

    const empty = document.createElement('tr');
    empty.innerHTML = '<td class="mut" colspan="' + cols + '">No records match these filters.</td>';
    empty.hidden = true;
    body.appendChild(empty);

    function apply() {
      const active = filters
        .map((el) => ({ el, c: +el.dataset.col, v: el.value.trim().toLowerCase() }))
        .filter((f) => f.v);

      let ordered = rows;
      if (sort) {
        ordered = [...rows].sort(
          (a, b) => val(a, sort.c).localeCompare(val(b, sort.c), undefined, { numeric: true }) * sort.d
        );
      }
      let shown = 0;
      ordered.forEach((r) => {
        const ok = active.every((f) => {
          const cell = val(r, f.c).toLowerCase();
          return f.el.tagName === 'SELECT' ? cell === f.v : cell.includes(f.v);
        });
        r.hidden = !ok;
        if (ok) shown++;
        body.insertBefore(r, empty);
      });
      empty.hidden = shown !== 0;
      if (count) count.textContent = shown + ' of ' + rows.length + ' records';
      heads.forEach((h) => {
        const ind = h.querySelector('.ind');
        if (ind) ind.textContent = sort && sort.c === +h.dataset.sort ? (sort.d > 0 ? ' ▲' : ' ▼') : '';
      });
    }

    heads.forEach((h) =>
      h.addEventListener('click', () => {
        const c = +h.dataset.sort;
        sort = sort && sort.c === c ? { c, d: -sort.d } : { c, d: 1 };
        apply();
      })
    );
    filters.forEach((el) => {
      el.addEventListener('input', apply);
      el.addEventListener('change', apply);
    });
    if (clear)
      clear.addEventListener('click', () => {
        filters.forEach((el) => (el.value = ''));
        sort = null;
        apply();
      });
    apply();
  }
  document.querySelectorAll('table.gt').forEach(initGrid);
})();
