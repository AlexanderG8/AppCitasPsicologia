using AppCitasPsicologia.Models.Empresas;
using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Repositorys;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    public class SuscripcionesController : Controller
    {
        private readonly IRepositorioSuscripciones repositorioSuscripciones;

        public SuscripcionesController(IRepositorioSuscripciones repositorioSuscripciones)
        {
            this.repositorioSuscripciones = repositorioSuscripciones;
        }

        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var suscripciones = await repositorioSuscripciones.Buscar(paginacion);
            var totalSuscripciones = await repositorioSuscripciones.Contar();
            var respuestaVM = new PaginacionRespuesta<Suscripciones>()
            {
                Elementos = suscripciones,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalSuscripciones,
                BaseURL = Url.Action()
            };
            return View(respuestaVM);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Suscripciones suscripcion)
        {
            if (!ModelState.IsValid)
            {
                return View(suscripcion);
            }

            var yaExiste = await repositorioSuscripciones.ExisteNombreSuscripcion(suscripcion.NombreSuscripcion, suscripcion.Id);
            if (yaExiste)
            {
                ModelState.AddModelError(nameof(suscripcion.NombreSuscripcion), $"La suscripción {suscripcion.NombreSuscripcion} ya existe.");
                return View(suscripcion);
            }

            await repositorioSuscripciones.Crear(suscripcion);
            TempData["Toast"] = "Suscripción creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var suscripcion = await repositorioSuscripciones.BuscarPorId(id);
            if (suscripcion is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La suscripción no existe." });
            }
            return View(suscripcion);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Suscripciones suscripcion)
        {
            if (!ModelState.IsValid)
            {
                return View(suscripcion);
            }

            var suscripcionDB = await repositorioSuscripciones.BuscarPorId(suscripcion.Id);
            if (suscripcionDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La suscripción no existe." });
            }

            var yaExiste = await repositorioSuscripciones.ExisteNombreSuscripcion(suscripcion.NombreSuscripcion, suscripcion.Id);
            if (yaExiste)
            {
                ModelState.AddModelError(nameof(suscripcion.NombreSuscripcion), $"La suscripción {suscripcion.NombreSuscripcion} ya existe.");
                return View(suscripcion);
            }

            suscripcion.FechaActualizacion = DateTime.Now;
            await repositorioSuscripciones.Actualizar(suscripcion);
            TempData["Toast"] = "Suscripción actualizada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BorrarSuscripcion(int id)
        {
            var suscripcion = await repositorioSuscripciones.BuscarPorId(id);
            if (suscripcion is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La suscripción no existe." });
            }
            await repositorioSuscripciones.Borrar(id);
            TempData["Toast"] = "Suscripción eliminada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteNombreSuscripcion(string nombreSuscripcion, int id)
        {
            var yaExiste = await repositorioSuscripciones.ExisteNombreSuscripcion(nombreSuscripcion, id);
            if (yaExiste)
            {
                return Json($"La suscripción {nombreSuscripcion} ya existe.");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCantidadDeMeses(int id) 
        {
            var suscripcion = await repositorioSuscripciones.BuscarPorId(id);
            if (suscripcion is null)
            {
                return Json($"La suscripción no existe.");
            }
            return Json(suscripcion.CantMeses);
        }
    }
}
