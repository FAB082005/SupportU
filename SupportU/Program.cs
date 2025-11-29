using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// Capa Infrastructure
using SupportU.Infrastructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository.Implementations;

// Capa Application
using SupportU.Application.Services.Interfaces;
using SupportU.Application.Services.Implementations;
using SupportU.Application.Profiles;
using SupportU.Infraestructure.Data;
using SupportU.Web.Middleware;
using SupportU.Application.Services;
using SupportU.Infrastructure.Repository;

using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infraestructure.Repository.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1); // Dura todo un día de inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Configurar D.I.
//Usuario
builder.Services.AddTransient<IRepositoryUsuario, RepositoryUsuario>();
builder.Services.AddTransient<IServiceUsuario, ServiceUsuario>();
builder.Services.AddAutoMapper(typeof(UsuariosProfile));

//Tecnico
builder.Services.AddScoped<IRepositoryTecnico, RepositoryTecnico>();
builder.Services.AddScoped<IServiceTecnico, ServiceTecnico>();
builder.Services.AddAutoMapper(typeof(TecnicosProfiles));

//categoria
builder.Services.AddScoped<IRepositoryCategoria, RepositoryCategoria>();
builder.Services.AddScoped<IServiceCategoria, ServiceCategoria>();
builder.Services.AddAutoMapper(typeof(CategoriasProfiles));

//especialidad
builder.Services.AddScoped<IRepositoryEspecialidad, RepositoryEspecialidad>();
builder.Services.AddScoped<IServiceEspecialidad, ServiceEspecialidad>();
builder.Services.AddAutoMapper(typeof(EspecialidadesProfiles));


//sla
builder.Services.AddScoped<IRepositorySla, RepositorySla>();
builder.Services.AddScoped<IServiceSla, ServiceSla>();
builder.Services.AddAutoMapper(typeof(SlasProfiles));

//ticket
builder.Services.AddScoped<IRepositoryTicket, RepositoryTicket>();
builder.Services.AddScoped<IServiceTicket, ServiceTicket>();
builder.Services.AddAutoMapper(typeof(TicketProfile));

//Asignacion
builder.Services.AddScoped<IRepositoryAsignacion, RepositoryAsignacion>();
builder.Services.AddScoped<IServiceAsignacion, ServiceAsignacion>();
builder.Services.AddAutoMapper(typeof(AsignacionProfile));

//Etiqueta
builder.Services.AddScoped<IRepositoryEtiqueta, RepositoryEtiqueta>();
builder.Services.AddScoped<IServiceEtiqueta, ServiceEtiqueta>();
builder.Services.AddAutoMapper(typeof(EtiquetaProfile));

//Historial Estado
builder.Services.AddScoped<IRepositoryHistorialEstados, RepositoryHistorialEstados>();
builder.Services.AddScoped<IServiceHistorialEstados, ServiceHistorialEstados>();
builder.Services.AddAutoMapper(typeof(HistorialEstadosProfile));


//Configurar Automapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<UsuariosProfile>();
});

//Configurar Conexión a la Base de Datos SQL
builder.Services.AddDbContext<SupportUContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDataBase"));

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// =============================
// Configuración Serilog
// =============================
var logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Information)
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File(@"Logs\Info-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
        .WriteTo.File(@"Logs\Debug-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.File(@"Logs\Warning-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File(@"Logs\Error-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File(@"Logs\Fatal-.log", shared: true, encoding: System.Text.Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Host.UseSerilog(logger);
// =============================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/Logout";
        options.Cookie.Name = "SupportU.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Error control Middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();
}

// Activar soporte a la solicitud de registro con Serilog
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();


// AUTENTICACIÓN / AUTORIZACIÓN: UseAuthentication debe ir antes de UseAuthorization
app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
