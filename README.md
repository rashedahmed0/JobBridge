# কর্মখোঁজ – Job Portal & CV Maker (ASP.NET Core MVC + EF Core + SQL Server)

HTML/JS prototype-টার পুরো .NET version. Data আর localStorage-এ থাকে না, SQL Server database-এ থাকে।

## Run korar steps

1. **.NET 10 SDK** + **SQL Server** (Visual Studio-র সাথে আসা LocalDB হলেই চলবে) লাগবে.
2. `appsettings.json`-এ `ConnectionStrings:DefaultConnection` দেখে নাও / বদলাও.
   - SQL Server Express: `Server=.\\SQLEXPRESS;Database=KormoKhojDb;Trusted_Connection=True;TrustServerCertificate=True`
   - SQL login: `Server=localhost;Database=KormoKhojDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True`
3. Migration + database create:

   ```bash
   dotnet tool install --global dotnet-ef      # ekbar-i lagbe
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

   Visual Studio → Package Manager Console:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
4. `dotnet run` (ba VS-e F5).

Prothom bar app cholle demo data auto-seed hoy (table khali thakle):

| Role      | Email                | Password |
|-----------|----------------------|----------|
| Admin     | admin@karmo.com      | admin123 |
| Recruiter | recruiter@demo.com   | demo123  |

Job seeker-ra Register page theke account khulbe. **Production-e admin password obosshoi bodlaben** (`Data/DbSeeder.cs`).

## Feature list (original HTML-er moto)

- Job feed: search, location / type filter, sort (Newest / A–Z), job details page, Apply Now
- CV Maker: guest mode (browser-e draft), live preview, 3 template, PDF download, print.
  Job seeker login thakle CV database-e save hoy; Register korar somoy guest CV account-e copy hoye jay
- Apply: saved CV-r snapshot application-e attach hoy; duplicate apply block kora
- Job Seeker dashboard: profile, my CV, my applications
- Recruiter dashboard: my jobs (post / edit / close-reopen / delete), company profile,
  applicants table (column sort, status change, Profile & CV modal + PDF) – recruiter table-e filter nai
- Admin dashboard: overview, users (filter/sort/delete), jobs (filter/sort/close/delete)

## Project structure

```
Controllers/   Account, Jobs, Cv, Dashboard, Seeker, Recruiter, Admin
Models/        AppUser, Job, JobApplication, Cv (+Education/Experience/Project), CvData (DTO)
Data/          AppDbContext, DbSeeder
Services/      CvMapper (entity <-> JSON DTO)
Views/         Razor views (+ Shared partials)
wwwroot/       css/site.css, js/site.js (tables, modal, PDF), js/cv.js (CV builder)
```

## Notes

- Password hash kora hoy (`PasswordHasher`), cookie authentication, role-based `[Authorize]`.
- Sob POST form / fetch CSRF-protected.
- User delete korle tar jobs, CV ar oi jobs-er applications o delete hoy.
- PDF download-er `html2pdf.js` CDN theke load hoy, tai PDF-er jonno internet lagbe.
