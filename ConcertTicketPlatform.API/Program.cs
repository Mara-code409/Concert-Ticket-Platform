using System;
using System.Text;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using ConcertTicketPlatform.Infrastructure.Data;
using ConcertTicketPlatform.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme   = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IConcertRepository, ConcertRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    if (!context.Concerts.Any(c => c.Date > DateTime.UtcNow))
    {
        context.Tickets.RemoveRange(context.Tickets);
        context.Reviews.RemoveRange(context.Reviews);
        context.Concerts.RemoveRange(context.Concerts);
        context.Artists.RemoveRange(context.Artists);
        context.Venues.RemoveRange(context.Venues);
        context.Categories.RemoveRange(context.Categories);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Concerts', RESEED, 0)");
        context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Artists', RESEED, 0)");
        context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Venues', RESEED, 0)");
        context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Categories', RESEED, 0)");
    }

    if (!context.Categories.Any())
    {
        var metal      = new ConcertTicketPlatform.Core.Entities.Category { Name = "Metal" };
        var rock       = new ConcertTicketPlatform.Core.Entities.Category { Name = "Rock" };
        var pop        = new ConcertTicketPlatform.Core.Entities.Category { Name = "Pop" };
        var electronic = new ConcertTicketPlatform.Core.Entities.Category { Name = "Electronic" };
        var hipHop     = new ConcertTicketPlatform.Core.Entities.Category { Name = "Hip-Hop" };
        var jazz       = new ConcertTicketPlatform.Core.Entities.Category { Name = "Jazz" };
        context.Categories.AddRange(metal, rock, pop, electronic, hipHop, jazz);
        context.SaveChanges();

        var arenaNat   = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Arena Națională",          Address = "Str. Maior Coravu 2",              City = "București",   Capacity = 55000 };
        var salaPalat  = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Sala Palatului",            Address = "Str. Ion Câmpineanu 28",           City = "București",   Capacity = 4000  };
        var clujArena  = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Cluj Arena",                Address = "Str. Cardinal Iuliu Hossu 2",      City = "Cluj-Napoca", Capacity = 30000 };
        var romexpo    = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Romexpo",                   Address = "Bd. Expozitiei 2",                  City = "București",   Capacity = 20000 };
        var arenele    = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Arenele Romane",            Address = "Calea Victoriei 155",               City = "București",   Capacity = 5000  };
        var stadionDan = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Stadionul Dan Păltinișanu", Address = "Str. Gheorghe Dima 2",              City = "Timișoara",   Capacity = 32000 };
        var salaSport  = new ConcertTicketPlatform.Core.Entities.Venue { Name = "Sala Sporturilor Dinamo",   Address = "Str. Maior Coravu 14",              City = "București",   Capacity = 8000  };
        var btArena    = new ConcertTicketPlatform.Core.Entities.Venue { Name = "BT Arena",                  Address = "Str. Pasteur 10",                   City = "Cluj-Napoca", Capacity = 10000 };
        context.Venues.AddRange(arenaNat, salaPalat, clujArena, romexpo, arenele, stadionDan, salaSport, btArena);
        context.SaveChanges();

        var metallica  = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Metallica",      Bio = "Formată în 1981 la Los Angeles, Metallica este una dintre cele mai influente trupe de heavy metal din lume, cu peste 125 milioane de albume vândute.",          Genre = "Metal",      ImageUrl = "" };
        var ironMaiden = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Iron Maiden",    Bio = "Legendara trupă britanică de heavy metal, fondată în 1975 la Londra. Cunoscuți pentru mascota Eddie și albumele concept.",                                      Genre = "Metal",      ImageUrl = "" };
        var coldplay   = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Coldplay",       Bio = "Trupă rock britanică formată în 1996 la Londra, cu hit-uri globale și spectacole vizuale impresionante.",                                                       Genre = "Rock",       ImageUrl = "" };
        var acdc       = new ConcertTicketPlatform.Core.Entities.Artist { Name = "AC/DC",          Bio = "Trupa australiană de hard rock fondată în 1973 la Sydney de frații Young. Unul dintre cei mai bine vânduți artiști din toate timpurile.",                       Genre = "Rock",       ImageUrl = "" };
        var edSheeran  = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Ed Sheeran",     Bio = "Cantautor britanic cu peste 150 milioane de albume vândute, cunoscut pentru melodii pop și folk pline de emoție.",                                              Genre = "Pop",        ImageUrl = "" };
        var adele      = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Adele",          Bio = "Cântăreața britanică cu voce excepțională, câștigătoare a 15 premii Grammy, cu albume multi-platină în toată lumea.",                                          Genre = "Pop",        ImageUrl = "" };
        var duaLipa    = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Dua Lipa",       Bio = "Artistă pop britanico-kosovară, una dintre cele mai ascultate artiste ale generației sale, cu hituri globale.",                                                 Genre = "Pop",        ImageUrl = "" };
        var prodigy    = new ConcertTicketPlatform.Core.Entities.Artist { Name = "The Prodigy",    Bio = "Grup electronic britanic fondat în 1990, pionieri ai muzicii rave, hardcore techno și big beat.",                                                               Genre = "Electronic", ImageUrl = "" };
        var guetta     = new ConcertTicketPlatform.Core.Entities.Artist { Name = "David Guetta",   Bio = "DJ și producător francez de muzică electronică, unul dintre cei mai influenți DJ din lume cu o carieră de peste 30 de ani.",                                   Genre = "Electronic", ImageUrl = "" };
        var garrix     = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Martin Garrix",  Bio = "DJ și producător olandez, cel mai tânăr DJ ajuns pe locul 1 în topul DJ Mag, cunoscut pentru energetic electronic music.",                                     Genre = "Electronic", ImageUrl = "" };
        var kendrick   = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Kendrick Lamar", Bio = "Rapper american din Compton, California, considerat unul dintre cei mai mari MC din istoria hip-hop-ului. Câștigător al Premiului Pulitzer.",                   Genre = "Hip-Hop",    ImageUrl = "" };
        var norah      = new ConcertTicketPlatform.Core.Entities.Artist { Name = "Norah Jones",    Bio = "Cântăreață și pianistă americană de jazz-pop, câștigătoare a 9 premii Grammy, cu un stil distinctiv și cald.",                                                 Genre = "Jazz",       ImageUrl = "" };
        context.Artists.AddRange(metallica, ironMaiden, coldplay, acdc, edSheeran, adele, duaLipa, prodigy, guetta, garrix, kendrick, norah);
        context.SaveChanges();

        context.Concerts.AddRange(
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – M72 World Tour",           Date = new DateTime(2025,  7, 15), Price = 350, TotalSeats = 50000, AvailableSeats = 0,     Artist = metallica,  Venue = arenaNat,   Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – Night 2 București",        Date = new DateTime(2025,  7, 16), Price = 350, TotalSeats = 50000, AvailableSeats = 0,     Artist = metallica,  Venue = arenaNat,   Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – Timișoara 2025",           Date = new DateTime(2025,  7, 19), Price = 340, TotalSeats = 32000, AvailableSeats = 0,     Artist = metallica,  Venue = stadionDan, Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Iron Maiden – Legacy of the Beast",    Date = new DateTime(2025,  8,  3), Price = 290, TotalSeats = 28000, AvailableSeats = 0,     Artist = ironMaiden, Venue = clujArena,  Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Coldplay – Music of the Spheres",      Date = new DateTime(2025,  6, 20), Price = 320, TotalSeats = 55000, AvailableSeats = 0,     Artist = coldplay,   Venue = arenaNat,   Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Coldplay – Cluj-Napoca 2025",          Date = new DateTime(2025,  6, 23), Price = 300, TotalSeats = 30000, AvailableSeats = 0,     Artist = coldplay,   Venue = clujArena,  Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "AC/DC – Power Up Tour 2025",           Date = new DateTime(2025, 10,  5), Price = 380, TotalSeats = 30000, AvailableSeats = 0,     Artist = acdc,       Venue = clujArena,  Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Ed Sheeran – Mathematics Tour 2025",   Date = new DateTime(2025,  5, 18), Price = 280, TotalSeats = 55000, AvailableSeats = 0,     Artist = edSheeran,  Venue = arenaNat,   Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Adele – An Evening with Adele",        Date = new DateTime(2025, 11,  8), Price = 450, TotalSeats =  4000, AvailableSeats = 0,     Artist = adele,      Venue = salaPalat,  Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Dua Lipa – Future Nostalgia Tour 2025",Date = new DateTime(2025,  8, 22), Price = 240, TotalSeats = 20000, AvailableSeats = 0,     Artist = duaLipa,    Venue = romexpo,    Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "The Prodigy – Fire Starter Tour",      Date = new DateTime(2025,  7,  4), Price = 180, TotalSeats =  8000, AvailableSeats = 0,     Artist = prodigy,    Venue = salaSport,  Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "David Guetta – Summer Session 2025",   Date = new DateTime(2025,  6, 28), Price = 200, TotalSeats = 10000, AvailableSeats = 0,     Artist = guetta,     Venue = btArena,    Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Martin Garrix – ANIMA Tour 2025",      Date = new DateTime(2025,  9, 27), Price = 210, TotalSeats = 20000, AvailableSeats = 0,     Artist = garrix,     Venue = romexpo,    Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Norah Jones – Noapte de Jazz",         Date = new DateTime(2025, 10, 18), Price = 150, TotalSeats =  4000, AvailableSeats = 0,     Artist = norah,      Venue = salaPalat,  Category = jazz       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – World Wired Tour 2026",    Date = new DateTime(2026,  8, 10), Price = 380, TotalSeats = 55000, AvailableSeats = 55000, Artist = metallica,  Venue = arenaNat,   Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – Night 2 2026",             Date = new DateTime(2026,  8, 11), Price = 380, TotalSeats = 55000, AvailableSeats = 42000, Artist = metallica,  Venue = arenaNat,   Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Metallica – Timișoara 2026",           Date = new DateTime(2026,  8, 14), Price = 360, TotalSeats = 32000, AvailableSeats = 32000, Artist = metallica,  Venue = stadionDan, Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Iron Maiden – Future Past Tour 2026",  Date = new DateTime(2026, 10, 15), Price = 310, TotalSeats = 28000, AvailableSeats = 28000, Artist = ironMaiden, Venue = clujArena,  Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Iron Maiden – Arenele Romane 2026",    Date = new DateTime(2026, 10, 17), Price = 290, TotalSeats =  5000, AvailableSeats =  3800, Artist = ironMaiden, Venue = arenele,    Category = metal      },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Coldplay – Music of the Spheres 2026", Date = new DateTime(2026,  9,  5), Price = 340, TotalSeats = 55000, AvailableSeats = 55000, Artist = coldplay,   Venue = arenaNat,   Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Coldplay – Night 2 București 2026",    Date = new DateTime(2026,  9,  6), Price = 340, TotalSeats = 55000, AvailableSeats = 48000, Artist = coldplay,   Venue = arenaNat,   Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Coldplay – Cluj-Napoca 2026",          Date = new DateTime(2026,  9,  8), Price = 320, TotalSeats = 30000, AvailableSeats = 22000, Artist = coldplay,   Venue = clujArena,  Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "AC/DC – Power Up Tour 2026",           Date = new DateTime(2026, 11,  7), Price = 400, TotalSeats = 55000, AvailableSeats = 55000, Artist = acdc,       Venue = arenaNat,   Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "AC/DC – Timișoara 2026",               Date = new DateTime(2026, 11,  9), Price = 375, TotalSeats = 32000, AvailableSeats = 28000, Artist = acdc,       Venue = stadionDan, Category = rock       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Ed Sheeran – The Mathematics Tour 2026",Date = new DateTime(2026,  7, 18), Price = 295, TotalSeats = 55000, AvailableSeats = 55000, Artist = edSheeran,  Venue = arenaNat,   Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Ed Sheeran – Cluj-Napoca 2026",        Date = new DateTime(2026,  7, 20), Price = 275, TotalSeats = 30000, AvailableSeats = 24000, Artist = edSheeran,  Venue = clujArena,  Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Adele – Live in Romania 2026",         Date = new DateTime(2026, 12, 10), Price = 480, TotalSeats =  4000, AvailableSeats =  4000, Artist = adele,      Venue = salaPalat,  Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Dua Lipa – Radical Optimism Tour",     Date = new DateTime(2026,  6, 20), Price = 255, TotalSeats = 20000, AvailableSeats = 20000, Artist = duaLipa,    Venue = romexpo,    Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Dua Lipa – BT Arena Cluj 2026",        Date = new DateTime(2026,  6, 22), Price = 240, TotalSeats = 10000, AvailableSeats =  8200, Artist = duaLipa,    Venue = btArena,    Category = pop        },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "David Guetta – United at Home NYE 2026",Date = new DateTime(2026, 12, 31), Price = 235, TotalSeats = 20000, AvailableSeats = 20000, Artist = guetta,     Venue = romexpo,    Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Martin Garrix – ANIMA Tour 2026",      Date = new DateTime(2026,  9, 27), Price = 225, TotalSeats = 20000, AvailableSeats = 20000, Artist = garrix,     Venue = romexpo,    Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Martin Garrix – Revelion 2027",        Date = new DateTime(2027,  1,  1), Price = 295, TotalSeats = 20000, AvailableSeats = 20000, Artist = garrix,     Venue = romexpo,    Category = electronic },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Kendrick Lamar – Grand National Tour", Date = new DateTime(2027,  2, 14), Price = 330, TotalSeats = 30000, AvailableSeats = 30000, Artist = kendrick,   Venue = clujArena,  Category = hipHop     },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Kendrick Lamar – București 2027",      Date = new DateTime(2027,  2, 16), Price = 350, TotalSeats = 20000, AvailableSeats = 20000, Artist = kendrick,   Venue = romexpo,    Category = hipHop     },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "Norah Jones – Jazz by Night 2026",     Date = new DateTime(2026, 11, 20), Price = 165, TotalSeats =  4000, AvailableSeats =  4000, Artist = norah,      Venue = salaPalat,  Category = jazz       },
            new ConcertTicketPlatform.Core.Entities.Concert { Title = "The Prodigy – Arenele Romane 2026",    Date = new DateTime(2026,  7,  5), Price = 205, TotalSeats =  5000, AvailableSeats =  5000, Artist = prodigy,    Venue = arenele,    Category = electronic }
        );
        context.SaveChanges();
    }

    if (!context.Reviews.Any())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var demo = await userManager.FindByEmailAsync("demo@demo.com");
        if (demo == null)
        {
            demo = new AppUser { UserName = "demo@demo.com", Email = "demo@demo.com", FullName = "Utilizator Demo" };
            await userManager.CreateAsync(demo, "Demo@1234");
            await userManager.AddToRoleAsync(demo, "User");
        }

        var ion = await userManager.FindByEmailAsync("ion.pop@example.com");
        if (ion == null)
        {
            ion = new AppUser { UserName = "ion.pop@example.com", Email = "ion.pop@example.com", FullName = "Ion Pop" };
            await userManager.CreateAsync(ion, "Review@123");
            await userManager.AddToRoleAsync(ion, "User");
        }

        var maria = await userManager.FindByEmailAsync("maria.ionescu@example.com");
        if (maria == null)
        {
            maria = new AppUser { UserName = "maria.ionescu@example.com", Email = "maria.ionescu@example.com", FullName = "Maria Ionescu" };
            await userManager.CreateAsync(maria, "Review@123");
            await userManager.AddToRoleAsync(maria, "User");
        }

        var cMet1  = context.Concerts.FirstOrDefault(c => c.Title == "Metallica – M72 World Tour");
        var cMet2  = context.Concerts.FirstOrDefault(c => c.Title == "Metallica – Night 2 București");
        var cMetTm = context.Concerts.FirstOrDefault(c => c.Title == "Metallica – Timișoara 2025");
        var cCold  = context.Concerts.FirstOrDefault(c => c.Title == "Coldplay – Music of the Spheres");
        var cEd    = context.Concerts.FirstOrDefault(c => c.Title == "Ed Sheeran – Mathematics Tour 2025");
        var cAcdc  = context.Concerts.FirstOrDefault(c => c.Title == "AC/DC – Power Up Tour 2025");
        var cIron  = context.Concerts.FirstOrDefault(c => c.Title == "Iron Maiden – Legacy of the Beast");
        var cAdele = context.Concerts.FirstOrDefault(c => c.Title == "Adele – An Evening with Adele");
        var cNorah = context.Concerts.FirstOrDefault(c => c.Title == "Norah Jones – Noapte de Jazz");

        if (cMet1  != null) context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cMet1.Id,  UserId = demo.Id, SeatNumber = "A1001", PurchasedAt = new DateTime(2025, 3, 10), IsUsed = true });
        if (cCold  != null) context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cCold.Id,  UserId = demo.Id, SeatNumber = "B2002", PurchasedAt = new DateTime(2025, 2,  5), IsUsed = true });
        if (cEd    != null) context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cEd.Id,    UserId = demo.Id, SeatNumber = "C3003", PurchasedAt = new DateTime(2025, 1, 20), IsUsed = true });
        if (cAdele != null) context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cAdele.Id, UserId = demo.Id, SeatNumber = "D4004", PurchasedAt = new DateTime(2025, 6, 15), IsUsed = true });

        if (cMet1 != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cMet1.Id, UserId = ion.Id, SeatNumber = "A5100", PurchasedAt = new DateTime(2025, 3, 1), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cMet1.Id, UserId = ion.Id, Rating = 5, Comment = "Show absolut incredibil! Sunetul a fost perfect la Arena Nationala. Master of Puppets a ridicat tot stadionul. Cel mai bun concert din viata mea!", CreatedAt = new DateTime(2025, 7, 16) });
        }
        if (cMetTm != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cMetTm.Id, UserId = ion.Id, SeatNumber = "E5300", PurchasedAt = new DateTime(2025, 4, 2), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cMetTm.Id, UserId = ion.Id, Rating = 4, Comment = "Concert excelent la Timisoara! Ambianta mai intima decat la Bucuresti dar energia a fost la fel de mare. Sunetul putea fi ceva mai bun in zona noastra, dar show-ul vizual a compensat din plin.", CreatedAt = new DateTime(2025, 7, 20) });
        }
        if (cEd != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cEd.Id, UserId = ion.Id, SeatNumber = "G3100", PurchasedAt = new DateTime(2024, 12, 10), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cEd.Id, UserId = ion.Id, Rating = 4, Comment = "Ed Sheeran singur pe scena cu chitara si looper-ul - pur si simplu magic! Vocea lui live e chiar mai buna decat in studio. Singurul minus: a durat doar 2 ore, as fi stat toata noaptea.", CreatedAt = new DateTime(2025, 5, 19) });
        }
        if (cAcdc != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cAcdc.Id, UserId = ion.Id, SeatNumber = "H2100", PurchasedAt = new DateTime(2025, 5, 20), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cAcdc.Id, UserId = ion.Id, Rating = 5, Comment = "La varsta lor, AC/DC inca domina scena! Brian Johnson a cantat impecabil, TNT si Highway to Hell au facut tot stadionul sa sara. Tunul de la final a fost epic. Rock n roll nu moare niciodata!", CreatedAt = new DateTime(2025, 10, 6) });
        }
        if (cIron != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cIron.Id, UserId = ion.Id, SeatNumber = "I1100", PurchasedAt = new DateTime(2025, 5, 1), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cIron.Id, UserId = ion.Id, Rating = 5, Comment = "Iron Maiden live este o experienta unica! Eddie pe scena, pirotehnica, costumele - spectacol total. Wasted Years si The Trooper m-au dat gata. Trupa e in forma maxima dupa atatatia ani!", CreatedAt = new DateTime(2025, 8, 4) });
        }
        if (cMet2 != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cMet2.Id, UserId = maria.Id, SeatNumber = "D5200", PurchasedAt = new DateTime(2025, 3, 5), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cMet2.Id, UserId = maria.Id, Rating = 5, Comment = "A doua seara a fost si mai buna decat prima! Au schimbat setlist-ul si au cantat One si Nothing Else Matters. Atmosfera electrizanta. Recomand orice concert Metallica cu ochii inchisi!", CreatedAt = new DateTime(2025, 7, 17) });
        }
        if (cCold != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cCold.Id, UserId = maria.Id, SeatNumber = "F4100", PurchasedAt = new DateTime(2025, 1, 15), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cCold.Id, UserId = maria.Id, Rating = 5, Comment = "Spectacolul vizual de neuitat! Bratarile LED, confetti-ul, show-ul de lumini - totul perfect sincronizat. Yellow la final m-a facut sa plang. Coldplay sunt altceva live, nimic nu se compara!", CreatedAt = new DateTime(2025, 6, 21) });
        }
        if (cAdele != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cAdele.Id, UserId = maria.Id, SeatNumber = "J0100", PurchasedAt = new DateTime(2025, 6, 14), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cAdele.Id, UserId = maria.Id, Rating = 5, Comment = "Adele m-a facut sa plang de 3 ori in 2 ore. Vocea ei live lasa fara cuvinte, iar momentele intime cu publicul au fost speciale. Someone Like You la final - piele de gaina garantat.", CreatedAt = new DateTime(2025, 11, 9) });
        }
        if (cNorah != null)
        {
            context.Tickets.Add(new ConcertTicketPlatform.Core.Entities.Ticket { ConcertId = cNorah.Id, UserId = maria.Id, SeatNumber = "K0200", PurchasedAt = new DateTime(2025, 8, 10), IsUsed = true });
            context.Reviews.Add(new ConcertTicketPlatform.Core.Entities.Review { ConcertId = cNorah.Id, UserId = maria.Id, Rating = 5, Comment = "O seara de jazz absolut fermecatoare! Norah Jones are o prezenta scenica calda si vocea ei e ca mierea. Sala Palatului a creat o atmosfera intima perfecta. Ideal pentru o seara romantica.", CreatedAt = new DateTime(2025, 10, 19) });
        }

        await context.SaveChangesAsync();
    }
}

app.UseMiddleware<ConcertTicketPlatform.API.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
