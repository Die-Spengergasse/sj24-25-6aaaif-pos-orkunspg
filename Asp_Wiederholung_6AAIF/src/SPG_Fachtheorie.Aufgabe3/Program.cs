using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Suche im Programmcode nach allen Klassen mit [ApiController]
builder.Services.AddControllers();
// SERVICE PROVIDER
// Stellt konfigurierte Instanzen von Klassen bereit
builder.Services.AddDbContext<AppointmentContext>(opt =>
{
    opt.UseSqlite("DataSource=cash.db");
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();   // Wird mit http zugegriffen, wird auf https weitergeleitet.

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    using (var db = scope.ServiceProvider.GetRequiredService<AppointmentContext>())
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        db.Seed();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request pipeline
app.MapControllers();  // Passt ein Controller zur Adresse? Ja: Diesen ausführen.
app.Run();

