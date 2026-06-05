# Concert Ticket Platform

Platformă web pentru cumpărarea și gestionarea biletelor la concerte. Utilizatorii pot naviga prin artiști, venue-uri și evenimente, cumpăra bilete și lăsa recenzii. Adminii gestionează evenimentele, artiștii și capacitatea sălilor.

---

## Tehnologii folosite

| Strat | Tehnologie |
|---|---|
| Backend API | ASP.NET Core 8 Web API |
| Frontend MVC | ASP.NET Core 8 Razor Views |
| Frontend SPA | Angular 17 (component hibrid) |
| Bază de date | SQL Server (LocalDB dev / Docker prod) |
| ORM | Entity Framework Core 8 |
| Autentificare | ASP.NET Core Identity + JWT Bearer |
| Logging | Serilog |
| Containerizare | Docker + docker-compose |

---

## Structura proiectului

```
ConcertTicketPlatform/
├── ConcertTicketPlatform.API/          # REST API (JWT Auth)
│   ├── Controllers/                    # 6 controllere RESTful
│   └── Middleware/                     # ExceptionMiddleware global
├── ConcertTicketPlatform.Core/         # Entități, Interfețe, DTO-uri
│   ├── Entities/                       # Concert, Artist, Venue, Ticket, Review, Category
│   ├── Interfaces/                     # IRepository, IService
│   └── DTOs/                           # ArtistDto, VenueDto, CreateArtistDto
├── ConcertTicketPlatform.Infrastructure/  # Implementări
│   ├── Data/                           # AppDbContext + EF Migrations
│   ├── Repositories/                   # ConcertRepository, ArtistRepository
│   └── Services/                       # ConcertService, ArtistService
├── ConcertTicketPlatform.Web/          # MVC Razor frontend
│   ├── Controllers/                    # AccountController, ConcertsController, ReviewsController
│   └── Views/                          # Login, Register, Concerts, MyTickets, Profile
└── concert-angular/                    # Component SPA Angular
```

---

## Relații EF Core

- **One-to-Many**: Artist → Concerts, Venue → Concerts, Category → Concerts, User → Tickets, User → Reviews
- **Many-to-Many**: Artist ↔ Category (tabelă `ArtistCategories` – un artist poate acoperi mai multe genuri)

---

## Rulare locală (Development)

### Cerințe
- .NET 8 SDK
- SQL Server LocalDB
- Node.js + Angular CLI (pentru componenta Angular)

### Pași

1. **Clonează repo-ul**
   ```bash
   git clone https://github.com/Mara-code409/Concert-Ticket-Platform.git
   cd Concert-Ticket-Platform
   ```

2. **Pornește API-ul** (aplică automat migrările și seed-ul)
   ```bash
   dotnet run --project ConcertTicketPlatform.API
   ```
   API disponibil la: `https://localhost:7283`

3. **Pornește aplicația Web**
   ```bash
   dotnet run --project ConcertTicketPlatform.Web
   ```
   Web disponibil la: `https://localhost:52362`

4. **Pornește componenta Angular** (opțional)
   ```bash
   cd concert-angular
   npm install
   ng serve
   ```
   Angular disponibil la: `http://localhost:4200`

---

## Rulare cu Docker

```bash
docker-compose up --build
```

| Serviciu | URL |
|---|---|
| Web MVC | http://localhost:5000 |
| API | http://localhost:5001 |

---

## Conturi demo

| Email | Parolă | Rol |
|---|---|---|
| `demo@demo.com` | `Demo@1234` | User (4 bilete, fără recenzii) |
| `ion.pop@example.com` | `Review@123` | User (bilete + recenzii) |
| `maria.ionescu@example.com` | `Review@123` | User (bilete + recenzii) |

---

## Endpoint-uri principale API

| Metodă | Endpoint | Acces |
|---|---|---|
| GET | `/api/concerts` | Public |
| GET | `/api/concerts/{id}` | Public |
| POST | `/api/concerts` | Admin |
| PUT | `/api/concerts/{id}` | Admin |
| DELETE | `/api/concerts/{id}` | Admin |
| GET | `/api/reviews?concertId=X` | Public |
| POST | `/api/reviews` | User (JWT) |
| DELETE | `/api/reviews/{id}` | Admin |
| GET | `/api/tickets` | User (JWT) |
| POST | `/api/tickets/buy` | User (JWT) |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/register` | Public |

---

## Pagini Web (MVC)

| Pagină | Acces |
|---|---|
| Home | Public |
| Concerts (listing + filtre) | Public |
| Concert Details | Public |
| Login / Register | Anonim |
| My Tickets | Autentificat |
| Profile | Autentificat |
