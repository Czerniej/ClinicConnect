using ClinicConnectApi.Data;
using ClinicConnectApi.Services;
using ClinicConnectApi.Exceptions;

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options.UseSqlite(serviceProvider.GetRequiredService<IConfiguration>().
        GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddScoped<IDoctorsService, DoctorsService>();
builder.Services.AddScoped<IPatientsService, PatientsService>();
builder.Services.AddScoped<IAppointmentsService, AppointmentsService>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.Run();

public partial class Program { }