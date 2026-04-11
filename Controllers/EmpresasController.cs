using AppCitasPsicologia.Models.Empresas;
using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Repositorys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
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
            if (!ModelState.IsValid) return View(empresa);

            var yaExisteRuc = await repositorioEmpresas.ExisteRuc(empresa.Ruc, empresa.Id);
            if (yaExisteRuc)
                ModelState.AddModelError(nameof(empresa.Ruc), $"El RUC {empresa.Ruc} ya existe.");

            var yaExisteNombre = await repositorioEmpresas.ExisteNombreEmpresa(empresa.NombreEmpresa, empresa.Id);
            if (yaExisteNombre)
                ModelState.AddModelError(nameof(empresa.NombreEmpresa), $"La empresa {empresa.NombreEmpresa} ya existe.");

            if (!ModelState.IsValid) return View(empresa);

            await repositorioEmpresas.Crear(empresa);
            TempData["Toast"] = "Empresa creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            return View(empresa);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Empresas empresa)
        {
            if (!ModelState.IsValid) return View(empresa);

            var empresaDB = await repositorioEmpresas.BuscarPorId(empresa.Id);
            if (empresaDB is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            var yaExisteRuc = await repositorioEmpresas.ExisteRuc(empresa.Ruc, empresa.Id);
            if (yaExisteRuc)
                ModelState.AddModelError(nameof(empresa.Ruc), $"El RUC {empresa.Ruc} ya existe.");

            var yaExisteNombre = await repositorioEmpresas.ExisteNombreEmpresa(empresa.NombreEmpresa, empresa.Id);
            if (yaExisteNombre)
                ModelState.AddModelError(nameof(empresa.NombreEmpresa), $"La empresa {empresa.NombreEmpresa} ya existe.");

            if (!ModelState.IsValid) return View(empresa);

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
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });
            await repositorioEmpresas.Borrar(id);
            TempData["Toast"] = "Empresa eliminada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteRuc(string ruc, int id)
        {
            var yaExiste = await repositorioEmpresas.ExisteRuc(ruc, id);
            if (yaExiste) return Json($"El RUC {ruc} ya existe.");
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteNombreEmpresa(string nombreEmpresa, int id)
        {
            var yaExiste = await repositorioEmpresas.ExisteNombreEmpresa(nombreEmpresa, id);
            if (yaExiste) return Json($"La empresa {nombreEmpresa} ya existe.");
            return Json(true);
        }

        // ──── Detalle Suscripciones ────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> DetalleSuscripcionEmpresa(int id, PaginacionViewModel paginacion)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(id);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            var detalles = await repositorioEmpresas.BuscarDetallesSuscripcionEmpresa(id, paginacion);
            var total = await repositorioEmpresas.ContarDetallesSuscripcionEmpresa(id);
            var respuestaVM = new PaginacionRespuesta<DetalleSuscripciones>
            {
                Elementos = detalles,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("DetalleSuscripcionEmpresa", "Empresas", new { id })
            };
            ViewBag.EmpresaId = id;
            ViewBag.NombreEmpresa = empresa.NombreEmpresa;
            return View(respuestaVM);
        }

        [HttpGet]
        public async Task<IActionResult> CrearDetalleSuscripcionEmpresa(int empresaId)
        {
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa no existe." });

            await CargarViewBagSuscripciones(empresaId, empresa.NombreEmpresa);
            return View(new DetalleSuscripciones { EmpresaId = empresaId });
        }

        [HttpPost]
        public async Task<IActionResult> CrearDetalleSuscripcionEmpresa(DetalleSuscripciones detalle, IFormFile docPagoFile)
        {
            ModelState.Remove(nameof(detalle.DocPago));

            if (detalle.SuscripcionId == 0)
                ModelState.AddModelError(nameof(detalle.SuscripcionId), "Seleccione una suscripcion.");

            if (detalle.FechaInicio != default && detalle.FechaInicio.Date < DateTime.Now.Date)
                ModelState.AddModelError(nameof(detalle.FechaInicio), "La fecha de inicio no puede ser anterior a hoy.");

            if (detalle.FechaFin != default && detalle.FechaFin < detalle.FechaInicio)
                ModelState.AddModelError(nameof(detalle.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");

            if (docPagoFile == null || docPagoFile.Length == 0)
                ModelState.AddModelError(nameof(detalle.DocPago), "El documento de pago es requerido.");

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(detalle.EmpresaId);
                await CargarViewBagSuscripciones(detalle.EmpresaId, empresa?.NombreEmpresa);
                return View(detalle);
            }

            detalle.DocPago = await GuardarArchivoPago(docPagoFile);
            await repositorioEmpresas.CrearDetalleSuscripcionEmpresa(detalle);
            TempData["Toast"] = "Suscripcion agregada correctamente.";
            return RedirectToAction("DetalleSuscripcionEmpresa", new { id = detalle.EmpresaId });
        }

        [HttpGet]
        public async Task<IActionResult> EditarDetalleSuscripcionEmpresa(int id)
        {
            var detalle = await repositorioEmpresas.BuscarDetalleSuscripcionPorId(id);
            if (detalle is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El detalle de suscripcion no existe." });

            var empresa = await repositorioEmpresas.BuscarPorId(detalle.EmpresaId);
            await CargarViewBagSuscripciones(detalle.EmpresaId, empresa?.NombreEmpresa);
            return View(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> EditarDetalleSuscripcionEmpresa(DetalleSuscripciones detalle, IFormFile docPagoFile)
        {
            ModelState.Remove(nameof(detalle.DocPago));

            if (detalle.SuscripcionId == 0)
                ModelState.AddModelError(nameof(detalle.SuscripcionId), "Seleccione una suscripcion.");

            if (detalle.FechaInicio != default && detalle.FechaInicio.Date < DateTime.Now.Date)
                ModelState.AddModelError(nameof(detalle.FechaInicio), "La fecha de inicio no puede ser anterior a hoy.");

            if (detalle.FechaFin != default && detalle.FechaFin < detalle.FechaInicio)
                ModelState.AddModelError(nameof(detalle.FechaFin), "La fecha de fin no puede ser anterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(detalle.EmpresaId);
                await CargarViewBagSuscripciones(detalle.EmpresaId, empresa?.NombreEmpresa);
                return View(detalle);
            }

            var detalleDB = await repositorioEmpresas.BuscarDetalleSuscripcionPorId(detalle.Id);
            if (detalleDB is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El detalle de suscripcion no existe." });

            if (docPagoFile != null && docPagoFile.Length > 0)
                detalle.DocPago = await GuardarArchivoPago(docPagoFile);
            else
                detalle.DocPago = detalleDB.DocPago;

            detalle.FechaActualizacion = DateTime.Now;
            await repositorioEmpresas.ActualizarDetalleSuscripcionEmpresa(detalle);
            TempData["Toast"] = "Suscripcion actualizada correctamente.";
            return RedirectToAction("DetalleSuscripcionEmpresa", new { id = detalle.EmpresaId });
        }

        [HttpPost]
        public async Task<IActionResult> BorrarDetalleSuscripcionEmpresa(int id, int empresaId)
        {
            var detalle = await repositorioEmpresas.BuscarDetalleSuscripcionPorId(id);
            if (detalle is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El detalle de suscripcion no existe." });

            await repositorioEmpresas.BorrarDetalleSuscripcionEmpresa(id);
            TempData["Toast"] = "Suscripcion eliminada correctamente.";
            return RedirectToAction("DetalleSuscripcionEmpresa", new { id = empresaId });
        }

        // ──── Helpers ─────────────────────────────────────────────────────────

        private async Task CargarViewBagSuscripciones(int empresaId, string nombreEmpresa)
        {
            var suscripciones = await repositorioSuscripciones.BuscarTodos();
            var select = suscripciones
                .Select(x => new SelectListItem(x.NombreSuscripcion, x.Id.ToString()))
                .ToList();
            select.Insert(0, new SelectListItem("-- Seleccione una suscripcion --", "0"));
            ViewBag.Suscripciones = select;
            ViewBag.EmpresaId = empresaId;
            ViewBag.NombreEmpresa = nombreEmpresa;
        }

        private async Task<string> GuardarArchivoPago(IFormFile archivo)
        {
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pagos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var ruta = Path.Combine(carpeta, nombreArchivo);
            using var stream = new FileStream(ruta, FileMode.Create);
            await archivo.CopyToAsync(stream);
            return nombreArchivo;
        }
    }
}
