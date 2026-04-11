using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Roles;
using AppCitasPsicologia.Repositorys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class OpcionesController : Controller
    {
        private readonly IRepositorioOpciones repositorioOpciones;
        public OpcionesController(IRepositorioOpciones repositorioOpciones)
        {
            this.repositorioOpciones = repositorioOpciones;
        }
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var opciones = await repositorioOpciones.Buscar(paginacion);
            var totalOpciones = await repositorioOpciones.Contar();
            var respuestaVM = new PaginacionRespuesta<Opciones>()
            {
                Elementos = opciones,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalOpciones,
                BaseURL = Url.Action()
            };
            return View(respuestaVM);
        }

        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(Opciones opcion)
        {
            if (!ModelState.IsValid)
            {
                return View(opcion);
            }

            #region Validaciones
            var yaExisteNombreOpcion = await repositorioOpciones.ExisteNombreOpcion(opcion.NombreOpcion, opcion.Id);
            if (yaExisteNombreOpcion)
            {
                ModelState.AddModelError(nameof(opcion.NombreOpcion), $"El nombre de opción {opcion.NombreOpcion} ya existe.");
            }
            #endregion

            await repositorioOpciones.Crear(opcion);
            TempData["Toast"] = "Opción creada correctamente";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var opcion = await repositorioOpciones.BuscarPorId(id);
            if (opcion is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La opción no existe." });
            }
            return View(opcion);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Opciones opcion)
        {
            if (!ModelState.IsValid)
            {
                return View(opcion);
            }
            var opcionDB = await repositorioOpciones.BuscarPorId(opcion.Id);
            if (opcionDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La opción no existe." });
            }

            #region Validaciones
            var yaExisteNombreOpcion = await repositorioOpciones.ExisteNombreOpcion(opcion.NombreOpcion, opcion.Id);
            if (yaExisteNombreOpcion)
            {
                ModelState.AddModelError(nameof(opcion.NombreOpcion), $"El nombre de opción {opcion.NombreOpcion} ya existe.");
            }
            #endregion

            opcion.FechaActualizacion = DateTime.Now;
            await repositorioOpciones.Actualizar(opcion);
            TempData["Toast"] = "Opción actualizada correctamente";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BorrarOpcion(int id)
        {
            var opcion = await repositorioOpciones.BuscarPorId(id);
            if (opcion is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La opción no existe." });
            }
            TempData["Toast"] = "Opción eliminada correctamente";
            await repositorioOpciones.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteCodigoOpcion(string codigoOpcion, int id)
        {
            var yaExisteCodigoOpcion = await repositorioOpciones.ExisteNombreOpcion(codigoOpcion, id);
            if (yaExisteCodigoOpcion)
            {
                return Json($"El código de opción {codigoOpcion} ya existe.");
            }
            return Json(true);
        }
    }
}
