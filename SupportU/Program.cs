using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SupportU.Infrastructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository.Implementations;
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
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregar localización (servicio)
builder.Services.AddLocalization();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- CONFIGURACIÓN DE CULTURAS ---
var supportedCultures = new[] { "es-CR", "en-US" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-CR");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});


//Configurar D.I.
//Usuario
builder.Services.AddTransient<IRepositoryUsuario, RepositoryUsuario>();
builder.Services.AddTransient<IServiceUsuario, ServiceUsuario>();

//Tecnico
builder.Services.AddScoped<IRepositoryTecnico, RepositoryTecnico>();
builder.Services.AddScoped<IServiceTecnico, ServiceTecnico>();

//categoria
builder.Services.AddScoped<IRepositoryCategoria, RepositoryCategoria>();
builder.Services.AddScoped<IServiceCategoria, ServiceCategoria>();

//especialidad
builder.Services.AddScoped<IRepositoryEspecialidad, RepositoryEspecialidad>();
builder.Services.AddScoped<IServiceEspecialidad, ServiceEspecialidad>();

//sla
builder.Services.AddScoped<IRepositorySla, RepositorySla>();
builder.Services.AddScoped<IServiceSla, ServiceSla>();

//ticket
builder.Services.AddScoped<IRepositoryTicket, RepositoryTicket>();
builder.Services.AddScoped<IServiceTicket, ServiceTicket>();

//Asignacion
builder.Services.AddScoped<IRepositoryAsignacion, RepositoryAsignacion>();
builder.Services.AddScoped<IServiceAsignacion, ServiceAsignacion>();

//Etiqueta
builder.Services.AddScoped<IRepositoryEtiqueta, RepositoryEtiqueta>();
builder.Services.AddScoped<IServiceEtiqueta, ServiceEtiqueta>();

//Historial Estado
builder.Services.AddScoped<IRepositoryHistorialEstados, RepositoryHistorialEstados>();
builder.Services.AddScoped<IServiceHistorialEstados, ServiceHistorialEstados>();

//Imagen
builder.Services.AddScoped<IRepositoryImagen, RepositoryImagen>();
builder.Services.AddScoped<IServiceImagen, ServiceImagen>();

//Notificacion
builder.Services.AddScoped<IRepositoryNotificacion, RepositoryNotificacion>();
builder.Services.AddScoped<IServiceNotificacion, ServiceNotificacion>();

//Notificacion
builder.Services.AddScoped<IRepositoryValoracion, RepositoryValoracion>();
builder.Services.AddScoped<IServiceValoracion, ServiceValoracion>();


//Autoasignado
builder.Services.AddScoped<IServiceAutoTriage, ServiceAutotriage>();






builder.Services.AddAutoMapper(
    typeof(UsuariosProfile),
    typeof(TecnicosProfiles),
    typeof(CategoriasProfiles),
    typeof(EspecialidadesProfiles),
    typeof(SlasProfiles),
    typeof(TicketProfile),
    typeof(AsignacionProfile),
    typeof(EtiquetaProfile),
    typeof(HistorialEstadosProfile),
    typeof(ImagenProfile),
    typeof(NotificacionProfile),
	typeof(ValoracionProfile)
);

//Configurar Conexión a la Base de Datos SQL
builder.Services.AddDbContext<SupportUContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDataBase"));

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// Configuración Serilog
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


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseMiddleware<ErrorHandlingMiddleware>();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();


var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);


app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
