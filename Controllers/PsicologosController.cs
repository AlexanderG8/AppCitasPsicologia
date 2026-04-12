using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Servicios;
using AppCitasPsicologia.Models.Usuarios;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class PsicologosController : Controller
    {
        private readonly IRepositorioPsicologos repositorioPsicologos;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioRoles repositorioRoles;
        private readonly IRepositorioServiciosPsicologos repositorioServiciosPsicologos;

        public PsicologosController(
            IRepositorioPsicologos repositorioPsicologos,
            IRepositorioEmpresas repositorioEmpresas,
            IServicioUsuario servicioUsuario,
            IRepositorioRoles repositorioRoles,
            IRepositorioServiciosPsicologos repositorioServiciosPsicologos)
        {
            this.repositorioPsicologos = repositorioPsicologos;
            this.repositorioEmpresas = repositorioEmpresas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioRoles = repositorioRoles;
            this.repositorioServiciosPsicologos = repositorioServiciosPsicologos;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            var rol = await repositorioRoles.BuscarPorCodigo("PSIC");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol de psicólogo no existe." });

            var psicologos = await repositorioPsicologos.Buscar(paginacion, empresaId, rol.Id);
            var total = await repositorioPsicologos.Contar(empresaId, rol.Id);
            var respuestaVM = new PaginacionRespuesta<Usuarios>
            {
                Elementos = psicologos,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Psicologos")
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

            var rol = await repositorioRoles.BuscarPorCodigo("PSIC");
            if (rol is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol de psicólogo no existe." });

            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            ViewBag.NombreRol = rol.NombreRol;
            return View(new Usuarios { EmpresaId = empresaId, RolId = rol.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var psicologo = await repositorioPsicologos.BuscarPorId(id);
            if (psicologo is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El psicólogo no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(psicologo.EmpresaId);
            ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
            return View(psicologo);
        }

        [HttpGet]
        public async Task<IActionResult> ServiciosPsicologo(int id)
        {
            var psicologo = await repositorioPsicologos.BuscarPorId(id);
            if (psicologo is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El psicólogo no existe." });

            var servicios = await repositorioServiciosPsicologos.ObtenerServiciosDePsicologo(id, psicologo.EmpresaId);
            var vm = new ServiciosPsicologosViewModel
            {
                IdPsicologo = id,
                NombrePsicologo = $"{psicologo.Nombres} {psicologo.Apellidos}",
                Servicios = servicios
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarServiciosPsicologo(int psicologoId, List<int> serviciosSeleccionados)
        {
            var psicologo = await repositorioPsicologos.BuscarPorId(psicologoId);
            if (psicologo is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El psicólogo no existe." });

            serviciosSeleccionados ??= new List<int>();
            await repositorioServiciosPsicologos.GuardarServiciosPsicologo(psicologoId, serviciosSeleccionados);
            TempData["Toast"] = "Servicios del psicólogo actualizados correctamente.";
            return RedirectToAction("Index");
        }
    }
}
