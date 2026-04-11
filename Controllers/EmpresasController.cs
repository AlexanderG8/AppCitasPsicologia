using AppCitasPsicologia.Models.Empresas;
using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppCitasPsicologia.Controllers
{
    public class EmpresasController : Controller
    {
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IRepositorioSuscripciones repositorioSuscripciones;

        public EmpresasController(IRepositorioEmpresas repositorioEmpresas, IRepositorioSuscripciones repositorioSuscripciones)
        {
            this.repositorioEmpresas = repositorioEmpresas;
            this.repositorioSuscripciones = repositorioSuscripciones;
        }

        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var empresas = await repositorioEmpresas.Buscar(paginacion);
            var totalEmpresas = await repositorioEmpresas.Contar();
            var respuestaVM = new PaginacionRespuesta<Empresas>()
            {
                Elementos = empresas,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalEmpresas,
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
        public async Task<IActionResult> Crear(Empresas empresa)
        {
            if (!ModelState.IsValid)
            {
                return View(empresa);
            }
            #region Validaciones
            var yaExisteRuc = await repositorioEmpresas.ExisteRuc(empresa.Ruc, empresa.Id);
            if (yaExisteRuc)
            {
                ModelState.AddModelError(nameof(empresa.Ruc), $"El RUC {empresa.Ruc} ya existe.");
            }

            var yaExisteNombre = await repositorioEmpresas.ExisteNombreEmpresa(empresa.NombreEmpresa, empresa.Id);
            if (yaExisteNombre)
            {
                ModelState.AddModelError(nameof(empresa.NombreEmpresa), $"La empresa {empresa.NombreEmpresa} ya existe.");
            }
            #endregion

            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            await repositorioEmpresas.Crear(empresa);
            TempData["Toast"] = "Empresa creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            }
            return View(empresa);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Empresas empresa)
        {
            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            var empresaDB = await repositorioEmpresas.BuscarPorId(empresa.Id);
            if (empresaDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            }
            #region Validaciones
            var yaExisteRuc = await repositorioEmpresas.ExisteRuc(empresa.Ruc, empresa.Id);
            if (yaExisteRuc)
            {
                ModelState.AddModelError(nameof(empresa.Ruc), $"El RUC {empresa.Ruc} ya existe.");
            }

            var yaExisteNombre = await repositorioEmpresas.ExisteNombreEmpresa(empresa.NombreEmpresa, empresa.Id);
            if (yaExisteNombre)
            {
                ModelState.AddModelError(nameof(empresa.NombreEmpresa), $"La empresa {empresa.NombreEmpresa} ya existe.");
            }
            #endregion

            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            empresa.FechaActualizacion = DateTime.Now;
            await repositorioEmpresas.Actualizar(empresa);
            TempData["Toast"] = "Empresa actualizada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BorrarEmpresa(int id)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            }
            await repositorioEmpresas.Borrar(id);
            TempData["Toast"] = "Empresa eliminada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteRuc(string ruc, int id)
        {
            var yaExiste = await repositorioEmpresas.ExisteRuc(ruc, id);
            if (yaExiste)
            {
                return Json($"El RUC {ruc} ya existe.");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteNombreEmpresa(string nombreEmpresa, int id)
        {
            var yaExiste = await repositorioEmpresas.ExisteNombreEmpresa(nombreEmpresa, id);
            if (yaExiste)
            {
                return Json($"La empresa {nombreEmpresa} ya existe.");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> DetalleSuscripcionEmpresa(int id)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            }

            var detalle = await repositorioEmpresas.BuscarDetalleSuscripcionEmpresa(id);
            if (detalle is null)
            {
                detalle = new DetalleSuscripciones() { EmpresaId = id };
            }

            var suscripciones = await repositorioSuscripciones.BuscarTodos();
            var selectSuscripciones = suscripciones
                .Select(x => new SelectListItem(x.NombreSuscripcion, x.Id.ToString()))
                .ToList();
            selectSuscripciones.Insert(0, new SelectListItem("-- Seleccione una suscripción --", "0"));
            ViewBag.Suscripciones = selectSuscripciones;
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;

            return View(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> DetalleSuscripcionEmpresa(DetalleSuscripciones detalle, IFormFile docPagoFile)
        {
            #region Validaciones
            var empresaDB = await repositorioEmpresas.BuscarPorId(detalle.EmpresaId);

            if (detalle.SuscripcionId == 0) 
            {
                ModelState.AddModelError(nameof(detalle.SuscripcionId), "Favor de selecionar una suscripción.");
            }

            if (detalle.FechaInicio.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError(nameof(detalle.FechaInicio), "La fecha de inicio no puede ser anterior a la fecha actual.");
            }

            if (detalle.FechaFin < detalle.FechaInicio)
            {
                ModelState.AddModelError(nameof(detalle.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }

            if (docPagoFile == null || docPagoFile.Length == 0)
            {
                ModelState.AddModelError(nameof(detalle.DocPago), "El documento de pago es requerido.");
            }

            if (!ModelState.IsValid)
            {
                var suscripciones = await repositorioSuscripciones.BuscarTodos();
                var selectSuscripciones = suscripciones
                    .Select(x => new SelectListItem(x.NombreSuscripcion, x.Id.ToString()))
                    .ToList();
                selectSuscripciones.Insert(0, new SelectListItem("-- Seleccione una suscripción --", "0"));
                ViewBag.Suscripciones = selectSuscripciones;
                ViewBag.NombreEmpresa = empresaDB.NombreEmpresa;
                return View(detalle);
            }

            if (empresaDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            }

            var suscripcionDB = await repositorioSuscripciones.BuscarPorId(detalle.SuscripcionId);
            if (suscripcionDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La suscripción no existe." });
            }
            #endregion

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pagos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{detalle.DocPago}_{detalle.Id}{Path.GetExtension(docPagoFile.FileName)}";
            var rutaArchivo = Path.Combine(carpeta, nombreArchivo);
            detalle.DocPago = nombreArchivo;
            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await docPagoFile.CopyToAsync(stream);
            }

            await repositorioEmpresas.GuardarDetalleSuscripcionEmpresa(detalle);
            TempData["Toast"] = "Suscripción de empresa guardada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
