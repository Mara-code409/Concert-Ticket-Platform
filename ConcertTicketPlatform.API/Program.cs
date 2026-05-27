using System;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using ConcertTicketPlatform.Infrastructure.Data;
using ConcertTicketPlatform.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Repository Pattern
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IConcertRepository, ConcertRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
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


// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}


// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Venues.Any())
    {
        context.Venues.Add(new ConcertTicketPlatform.Core.Entities.Venue
        { Name = "Arena Națională", Address = "Str. Maior Coravu 2", City = "București", Capacity = 55000 });
        context.SaveChanges();
    }

    if (!context.Artists.Any())
    {
        context.Artists.Add(new ConcertTicketPlatform.Core.Entities.Artist
        { Name = "Metallica", Bio = "American heavy metal band", Genre = "Metal", ImageUrl = "" });
        context.SaveChanges();
    }

    if (!context.Categories.Any())
    {
        context.Categories.Add(new ConcertTicketPlatform.Core.Entities.Category { Name = "Metal" });
        context.SaveChanges();
    }

    if (!context.Concerts.Any())
    {
        context.Concerts.Add(new ConcertTicketPlatform.Core.Entities.Concert
        { Title = "Metallica World Tour", Date = new DateTime(2025, 7, 15), Price = 250, TotalSeats = 1000, AvailableSeats = 1000, ArtistId = 1, VenueId = 1, CategoryId = 1 });
        context.SaveChanges();
    }
}

app.UseMiddleware<ConcertTicketPlatform.API.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();