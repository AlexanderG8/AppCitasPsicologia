using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Servicios;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class ServiciosController : Controller
    {
        private readonly IRepositorioServicios repositorioServicios;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServicioUsuario servicioUsuario;

        public ServiciosController(
            IRepositorioServicios repositorioServicios,
            IRepositorioEmpresas repositorioEmpresas,
            IServicioUsuario servicioUsuario)
        {
            this.repositorioServicios = repositorioServicios;
            this.repositorioEmpresas = repositorioEmpresas;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            var servicios = await repositorioServicios.Buscar(empresaId, paginacion);
            var total = await repositorioServicios.Contar(empresaId);
            var respuestaVM = new PaginacionRespuesta<Servicios>
            {
                Elementos = servicios,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Servicios")
            };
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            return View(respuestaVM);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();

            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            ViewBag.EmpresaId = empresaId;
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            return View(new Servicios { EmpresaId = empresaId });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Servicios modelo)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            modelo.EmpresaId = empresaId;
            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(modelo.EmpresaId);
                ViewBag.EmpresaId = modelo.EmpresaId;
                ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
                return View(modelo);
            }

            modelo.FechaCreacion = DateTime.Now;
            await repositorioServicios.Crear(modelo);
            TempData["Toast"] = "Servicio creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var servicio = await repositorioServicios.BuscarPorId(id);
            if (servicio is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El servicio no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(servicio.EmpresaId);
            ViewBag.EmpresaId = servicio.EmpresaId;
            ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
            return View(servicio);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Servicios modelo)
        {
            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(modelo.EmpresaId);
                ViewBag.EmpresaId = modelo.EmpresaId;
                ViewBag.NombreEmpresa = empresa?.NombreEmpresa;
                return View(modelo);
            }

            var servicioDB = await repositorioServicios.BuscarPorId(modelo.Id);
            if (servicioDB is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El servicio no existe." });

            modelo.FechaActualizacion = DateTime.Now;
            await repositorioServicios.Actualizar(modelo);
            TempData["Toast"] = "Servicio actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id)
        {
            var servicio = await repositorioServicios.BuscarPorId(id);
            if (servicio is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El servicio no existe." });

            await repositorioServicios.Borrar(id);
            TempData["Toast"] = "Servicio eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
