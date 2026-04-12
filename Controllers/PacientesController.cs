using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Usuarios;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class PacientesController : Controller
    {
        private readonly IRepositorioPacientes repositorioPacientes;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioRoles repositorioRoles;

        public PacientesController(
            IRepositorioPacientes repositorioPacientes,
            IRepositorioEmpresas repositorioEmpresas,
            IServicioUsuario servicioUsuario,
            IRepositorioRoles repositorioRoles)
        {
            this.repositorioPacientes = repositorioPacientes;
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

            var rol = await repositorioRoles.BuscarPorCodigo("PACI");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol de paciente no existe." });

            var pacientes = await repositorioPacientes.Buscar(paginacion, empresaId, rol.Id);
            var total = await repositorioPacientes.Contar(empresaId, rol.Id);
            var respuestaVM = new PaginacionRespuesta<Usuarios>
            {
                Elementos = pacientes,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Pacientes")
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

            var rol = await repositorioRoles.BuscarPorCodigo("PACI");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol de paciente no existe." });

            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            ViewBag.NombreRol = rol.NombreRol;
            return View(new Usuarios { EmpresaId = empresaId, RolId = rol.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var paciente = await repositorioPacientes.BuscarPorId(id);
            if (paciente is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El paciente no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(paciente.EmpresaId);
            ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
            return View(paciente);
        }
    }
}
