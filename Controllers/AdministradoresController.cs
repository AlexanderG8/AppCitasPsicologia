using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Usuarios;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class AdministradoresController : Controller
    {
        private readonly IRepositorioAdministradores repositorioAdministradores;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioRoles repositorioRoles;

        public AdministradoresController(
            IRepositorioAdministradores repositorioAdministradores,
            IRepositorioEmpresas repositorioEmpresas,
            IServicioUsuario servicioUsuario,
            IRepositorioRoles repositorioRoles)
        {
            this.repositorioAdministradores = repositorioAdministradores;
            this.repositorioEmpresas = repositorioEmpresas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioRoles = repositorioRoles;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            var rol = await repositorioRoles.BuscarPorCodigo("ADM");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol del usuario no existe." });

            var administradores = await repositorioAdministradores.Buscar(paginacion, empresaId, rol.Id);
            var total = await repositorioAdministradores.Contar(empresaId, rol.Id);
            var respuestaVM = new PaginacionRespuesta<Usuarios>
            {
                Elementos = administradores,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Administradores")
            };
            ViewBag.EmpresaId = empresaId;
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            ViewBag.NombreRol = rol.NombreRol;
            return View(respuestaVM);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            var rol = await repositorioRoles.BuscarPorCodigo("ADM");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol del usuario no existe." });

            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            ViewBag.NombreRol = rol.NombreRol;
            return View(new Usuarios { EmpresaId = empresaId, RolId = rol.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var administrador = await repositorioAdministradores.BuscarPorId(id);
            if (administrador is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El administrador no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(administrador.EmpresaId);
            ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
            return View(administrador);
        }
    }
}
