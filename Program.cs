using AppCitasPsicologia.Models.Usuarios;
using AppCitasPsicologia.Repositorys;
using AppCitasPsicologia.Services;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using NETPortafolio.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de la pol�tica de autorizaci�n para requerir usuarios autenticados
var politicaUsuarioAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
// Agregar la pol�tica de autorizaci�n a nivel global para todas las vistas
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuarioAutenticados));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IRepositorioRoles, RepositorioRoles>();
builder.Services.AddTransient<IRepositorioOpciones, RepositorioOpciones>();
builder.Services.AddTransient<IRepositorioEmpresas, RepositorioEmpresas>();
builder.Services.AddTransient<IRepositorioSuscripciones, RepositorioSuscripciones>();
builder.Services.AddTransient<IRepositorioServicios, RepositorioServicios>();
builder.Services.AddTransient<IRepositorioAdministradores, RepositorioAdministradores>();
builder.Services.AddTransient<IRepositorioPsicologos, RepositorioPsicologos>();
builder.Services.AddTransient<IRepositorioPacientes, RepositorioPacientes>();
builder.Services.AddTransient<IRepositorioServiciosPsicologos, RepositorioServiciosPsicologos>();

builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IServicioUsuario, ServicioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuarios>, UsuarioStore>();
builder.Services.AddTransient<IServiceEmail, ServiceEmailGmail>();
builder.Services.AddIdentityCore<Usuarios>(opciones =>
{
    // True = Si || False = No
    // Require Digit: Indica si se requiere al menos un d�gito en la contrase�a.
    opciones.Password.RequireDigit = false;
    // RequireLowercase: Indica si se requiere al menos una letra min�scula en la contrase�a.
    opciones.Password.RequireLowercase = false;
    // RequireUppercase: Indica si se requiere al menos una letra may�scula en la contrase�a.
    opciones.Password.RequireUppercase = false;
    // RequireNonAlphanumeric: Indica si se requiere al menos un car�cter no alfanum�rico (como s�mbolos) en la contrase�a.
    opciones.Password.RequireNonAlphanumeric = false;

}).AddErrorDescriber<MensajesDeErrorIdentity>()
  .AddSignInManager();


// Configuraci�n de autenticaci�n utilizando cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/Usuarios/Login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
