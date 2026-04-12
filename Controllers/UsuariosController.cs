using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Usuarios;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NETPortafolio.Services;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioRoles repositorioRoles;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServiceEmail serviceEmail;
        private readonly IPasswordHasher<Usuarios> passwordHasher;
        private readonly IServicioUsuario servicioUsuario;
        private readonly SignInManager<Usuarios> signInManager;

        public UsuariosController(
            IRepositorioUsuarios repositorioUsuarios,
            IRepositorioRoles repositorioRoles,
            IRepositorioEmpresas repositorioEmpresas,
            IServiceEmail serviceEmail,
            IPasswordHasher<Usuarios> passwordHasher,
            IServicioUsuario servicioUsuario,
            SignInManager<Usuarios> signInManager)
        {
            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioRoles = repositorioRoles;
            this.repositorioEmpresas = repositorioEmpresas;
            this.serviceEmail = serviceEmail;
            this.passwordHasher = passwordHasher;
            this.servicioUsuario = servicioUsuario;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id, PaginacionViewModel paginacion)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            var usuarios = await repositorioUsuarios.Buscar(id, paginacion);
            var total = await repositorioUsuarios.Contar(id);
            var respuestaVM = new PaginacionRespuesta<Usuarios>
            {
                Elementos = usuarios,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Usuarios", new { id })
            };
            ViewBag.EmpresaId = id;
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            return View(respuestaVM);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int empresaId)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            await CargarViewBagRoles(empresaId, empresa.NombreEmpresa);
            return View(new Usuarios { EmpresaId = empresaId });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuarios modelo, string returnUrl = null)
        {
            ModelState.Remove(nameof(modelo.Contrasena));

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(modelo.EmpresaId);
                await CargarViewBagRoles(modelo.EmpresaId, empresa?.NombreEmpresa);
                return View(modelo);
            }

            var emailExiste = await repositorioUsuarios.BuscarUsuarioPorEmail(modelo.Email);
            if (emailExiste is not null)
            {
                ModelState.AddModelError(nameof(modelo.Email), $"El correo {modelo.Email} ya está registrado.");
                var empresa = await repositorioEmpresas.BuscarPorId(modelo.EmpresaId);
                await CargarViewBagRoles(modelo.EmpresaId, empresa?.NombreEmpresa);
                return View(modelo);
            }

            var nuevoId = await repositorioUsuarios.CrearUsuario(modelo);

            var token = Guid.NewGuid().ToString("N");
            var expiracion = DateTime.Now.AddHours(24);
            await repositorioUsuarios.GuardarTokenActivacion(nuevoId, token, expiracion);

            var link = Url.Action("EstablecerContrasena", "Usuarios", new { token }, Request.Scheme);
            await serviceEmail.EnviarCorreoIngresarContrasena(modelo.Email, modelo.Nombres, link);

            TempData["Toast"] = $"Usuario creado. Se envió el enlace de acceso a {modelo.Email}.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", new { id = modelo.EmpresaId });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await repositorioUsuarios.BuscarPorId(id);
            if (usuario is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El usuario no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(usuario.EmpresaId);
            await CargarViewBagRoles(usuario.EmpresaId, empresa?.NombreEmpresa);
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Usuarios modelo, string returnUrl = null)
        {
            ModelState.Remove(nameof(modelo.Contrasena));

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(modelo.EmpresaId);
                await CargarViewBagRoles(modelo.EmpresaId, empresa?.NombreEmpresa);
                return View(modelo);
            }

            var usuarioDB = await repositorioUsuarios.BuscarPorId(modelo.Id);
            if (usuarioDB is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El usuario no existe." });

            modelo.FechaActualizacion = DateTime.Now;
            await repositorioUsuarios.Actualizar(modelo);
            TempData["Toast"] = "Usuario actualizado correctamente.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", new { id = modelo.EmpresaId });
        }

        [HttpPost]
        public async Task<IActionResult> BorrarUsuario(int id, int empresaId, string returnUrl = null)
        {
            var usuario = await repositorioUsuarios.BuscarPorId(id);
            if (usuario is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El usuario no existe." });

            await repositorioUsuarios.Borrar(id);
            TempData["Toast"] = "Usuario eliminado correctamente.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", new { id = empresaId });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> EstablecerContrasena(string token)
        {
            var usuario = await repositorioUsuarios.BuscarPorToken(token);
            if (usuario is null)
                return RedirectToAction("TokenInvalido");

            ViewBag.Token = token;
            ViewBag.NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EstablecerContrasena(string token, string contrasena, string confirmarContrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
            {
                ModelState.AddModelError("contrasena", "La contraseña debe tener al menos 6 caracteres.");
                ViewBag.Token = token;
                return View();
            }

            if (contrasena != confirmarContrasena)
            {
                ModelState.AddModelError("confirmarContrasena", "Las contraseñas no coinciden.");
                ViewBag.Token = token;
                return View();
            }

            var usuario = await repositorioUsuarios.BuscarPorToken(token);
            if (usuario is null)
                return RedirectToAction("TokenInvalido");

            var hash = passwordHasher.HashPassword(usuario, contrasena);
            await repositorioUsuarios.ActualizarContrasena(usuario.Id, hash);

            TempData["Toast"] = "Contraseña establecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult TokenInvalido()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await signInManager.PasswordSignInAsync(
                modelo.Email, modelo.Password, isPersistent: true, lockoutOnFailure: false);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreKey(int id, int empresaId, string returnUrl)
        {
            var usuario = await repositorioUsuarios.BuscarPorId(id);
            if (usuario is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El usuario no existe." });

            var token = Guid.NewGuid().ToString("N");
            var expiracion = DateTime.Now.AddHours(24);
            await repositorioUsuarios.GuardarTokenActivacion(id, token, expiracion);

            var link = Url.Action("EstablecerContrasena", "Usuarios", new { token }, Request.Scheme);
            await serviceEmail.EnviarCorreoIngresarContrasena(usuario.Email, usuario.Nombres, link);

            TempData["Toast"] = $"Correo de acceso reenviado a {usuario.Email}.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", new { id = empresaId });
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var usuario = await repositorioUsuarios.BuscarPorId(usuarioId);
            if (usuario is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El usuario no existe." });

            var rol = usuario.RolId > 0 ? await repositorioRoles.BuscarPorId(usuario.RolId) : null;
            usuario.NombreRol = rol?.NombreRol ?? "Sin rol";
            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login");
        }

        // ──── Helpers ───────────────────────────────────────────────────────

        private async Task CargarViewBagRoles(int empresaId, string nombreEmpresa)
        {
            var roles = await repositorioRoles.Buscar(new Models.Paginacion.PaginacionViewModel
            {
                RecordsPorPagina = 50
            });
            var select = roles.Select(r => new SelectListItem(r.NombreRol, r.Id.ToString())).ToList();
            select.Insert(0, new SelectListItem("-- Seleccione un rol --", "0"));
            ViewBag.Roles = select;
            ViewBag.EmpresaId = empresaId;
            ViewBag.NombreEmpresa = nombreEmpresa;
        }

        public async Task<IActionResult> VerificarNroDocumento (string NroDocumento, int Id)
        {
            var yaExisteNroDocumento = await repositorioUsuarios.ExisteNroDocumento(NroDocumento, Id);
            if (yaExisteNroDocumento)
            {
                return Json($"El número de documento {NroDocumento} ya existe.");
            }
            return Json(true);
        }
    }
}
