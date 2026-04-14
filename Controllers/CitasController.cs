using AppCitasPsicologia.Models.Citas;
using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppCitasPsicologia.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly IRepositorioCitas repositorioCitas;
        private readonly IRepositorioEmpresas repositorioEmpresas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioPacientes repositorioPacientes;
        private readonly IRepositorioPsicologos repositorioPsicologos;
        private readonly IRepositorioRoles repositorioRoles;
        private readonly IRepositorioServiciosPsicologos repositorioServiciosPsicologos;
        private readonly IRepositorioServiciosCitas repositorioServiciosCitas;
        private readonly IRepositorioDetallesCitas repositorioDetallesCitas;

        public CitasController(
            IRepositorioCitas repositorioCitas,
            IRepositorioEmpresas repositorioEmpresas,
            IServicioUsuario servicioUsuario,
            IRepositorioPacientes repositorioPacientes,
            IRepositorioPsicologos repositorioPsicologos,
            IRepositorioRoles repositorioRoles,
            IRepositorioServiciosPsicologos repositorioServiciosPsicologos,
            IRepositorioServiciosCitas repositorioServiciosCitas,
            IRepositorioDetallesCitas repositorioDetallesCitas)
        {
            this.repositorioCitas = repositorioCitas;
            this.repositorioEmpresas = repositorioEmpresas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioPacientes = repositorioPacientes;
            this.repositorioPsicologos = repositorioPsicologos;
            this.repositorioRoles = repositorioRoles;
            this.repositorioServiciosPsicologos = repositorioServiciosPsicologos;
            this.repositorioServiciosCitas = repositorioServiciosCitas;
            this.repositorioDetallesCitas = repositorioDetallesCitas;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            if (empresa is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            var citas = await repositorioCitas.Buscar(paginacion, empresaId);
            var total = await repositorioCitas.Contar(empresaId);

            var respuestaVM = new PaginacionRespuesta<Citas>
            {
                Elementos = citas,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = total,
                BaseURL = Url.Action("Index", "Citas")
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
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La empresa del usuario no existe." });

            await CargarViewBagDropdowns(empresaId, empresa.NombreEmpresa);
            return View(new Citas { FechaReserva = DateTime.Today });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Citas modelo, List<int> serviciosSeleccionados, IFormFile docPago)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
                await CargarViewBagDropdowns(empresaId, empresa?.NombreEmpresa);
                return View(modelo);
            }

            if (docPago != null && docPago.Length > 0)
                modelo.DocPago = await GuardarArchivoPagoCita(docPago);

            modelo.FechaCreacion = DateTime.Now;
            var citaId = await repositorioCitas.Crear(modelo);

            serviciosSeleccionados ??= new List<int>();
            await repositorioServiciosCitas.GuardarServiciosCita(citaId, serviciosSeleccionados);

            TempData["Toast"] = "Cita creada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
            await CargarViewBagDropdowns(empresaId, empresa?.NombreEmpresa);

            // Servicios del psicólogo con Estado=1 si ya están en SERVICIOSCITAS para esta cita
            var psicServicios = await repositorioServiciosPsicologos.ObtenerServiciosDePsicologo(cita.PsicologoId, empresaId);
            var citaServiciosIds = (await repositorioServiciosCitas.ObtenerServiciosDeCita(id, empresaId))
                .Where(s => s.Estado == 1)
                .Select(s => s.Id)
                .ToHashSet();

            var serviciosCita = psicServicios
                .Where(s => s.Estado)
                .Select(s => new ServicioCitaItem
                {
                    Id = s.Id,
                    NombreServicio = s.NombreServicio,
                    CostoServicio = s.CostoServicio,
                    Estado = citaServiciosIds.Contains(s.Id) ? 1 : 0
                });

            ViewBag.ServiciosCita = serviciosCita;
            return View(cita);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Citas modelo, List<int> serviciosSeleccionados, IFormFile docPago)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();

            if (!ModelState.IsValid)
            {
                var empresa = await repositorioEmpresas.BuscarPorId(empresaId);
                await CargarViewBagDropdowns(empresaId, empresa?.NombreEmpresa);
                ViewBag.ServiciosCita = Enumerable.Empty<ServicioCitaItem>();
                return View(modelo);
            }

            var citaDB = await repositorioCitas.BuscarPorId(modelo.Id);
            if (citaDB is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            if (docPago != null && docPago.Length > 0)
            {
                // Eliminar archivo anterior si existe
                if (!string.IsNullOrEmpty(citaDB.DocPago))
                {
                    var rutaViejo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pagoscitas", citaDB.DocPago);
                    if (System.IO.File.Exists(rutaViejo))
                        System.IO.File.Delete(rutaViejo);
                }
                modelo.DocPago = await GuardarArchivoPagoCita(docPago);
            }
            else
            {
                modelo.DocPago = citaDB.DocPago; // conservar el archivo actual
            }

            modelo.FechaActualizacion = DateTime.Now;
            await repositorioCitas.Actualizar(modelo);

            serviciosSeleccionados ??= new List<int>();
            await repositorioServiciosCitas.GuardarServiciosCita(modelo.Id, serviciosSeleccionados);

            TempData["Toast"] = "Cita actualizada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerCita(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            return View(cita);
        }

        [HttpGet]
        public async Task<IActionResult> PostergarCita(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            var vm = new PostergarCitaViewModel
            {
                Id = cita.Id,
                NombreCliente = cita.NombreCliente,
                NombrePsicologo = cita.NombrePsicologo,
                FechaReservaActual = cita.FechaReserva,
                HoraInicioActual = cita.HoraInicio,
                HoraFinActual = cita.HoraFin,
                NuevaFechaReserva = cita.FechaReserva,
                NuevaHoraInicio = cita.HoraInicio,
                NuevaHoraFin = cita.HoraFin
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PostergarCita(PostergarCitaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var cita = await repositorioCitas.BuscarPorId(modelo.Id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            cita.FechaReserva = modelo.NuevaFechaReserva;
            cita.HoraInicio = modelo.NuevaHoraInicio;
            cita.HoraFin = modelo.NuevaHoraFin;
            cita.MotivoPostergacion = modelo.MotivoPostergacion;
            cita.FechaActualizacion = DateTime.Now;
            await repositorioCitas.Actualizar(cita);

            TempData["Toast"] = "Cita postergada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            var vm = new CancelarCitaViewModel
            {
                Id = cita.Id,
                NombreCliente = cita.NombreCliente,
                NombrePsicologo = cita.NombrePsicologo,
                FechaReserva = cita.FechaReserva,
                HoraInicio = cita.HoraInicio,
                HoraFin = cita.HoraFin
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CancelarCita(CancelarCitaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var cita = await repositorioCitas.BuscarPorId(modelo.Id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            cita.MotivoCancelacion = modelo.MotivoCancelacion;
            cita.FechaCancelacion = DateTime.Now;
            cita.FechaActualizacion = DateTime.Now;
            await repositorioCitas.Actualizar(cita);

            TempData["Toast"] = "Cita cancelada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Documentar(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            var detalle = await repositorioDetallesCitas.BuscarPorCitaId(id);
            if (detalle is null)
                detalle = new DetallesCitas { CitaId = id };

            ViewBag.NombreCliente = cita.NombreCliente;
            ViewBag.NombrePsicologo = cita.NombrePsicologo;
            ViewBag.FechaReserva = cita.FechaReserva;
            ViewBag.Asistencia = cita.Asistencia;
            return View(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> Documentar(DetallesCitas modelo, string asistencia)
        {
            if (!ModelState.IsValid)
            {
                var citaErr = await repositorioCitas.BuscarPorId(modelo.CitaId);
                ViewBag.NombreCliente = citaErr?.NombreCliente;
                ViewBag.NombrePsicologo = citaErr?.NombrePsicologo;
                ViewBag.FechaReserva = citaErr?.FechaReserva;
                ViewBag.Asistencia = asistencia;
                return View(modelo);
            }

            var cita = await repositorioCitas.BuscarPorId(modelo.CitaId);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            // Actualizar asistencia en la cita
            cita.Asistencia = string.IsNullOrEmpty(asistencia) ? null : asistencia;
            cita.FechaActualizacion = DateTime.Now;
            await repositorioCitas.Actualizar(cita);

            var existe = await repositorioDetallesCitas.ExistePorCitaId(modelo.CitaId);
            if (existe)
            {
                modelo.FechaActualizacion = DateTime.Now;
                await repositorioDetallesCitas.Actualizar(modelo);
            }
            else
            {
                modelo.FechaRegistro = DateTime.Now;
                await repositorioDetallesCitas.Crear(modelo);
            }

            TempData["Toast"] = "Documentación guardada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id)
        {
            var cita = await repositorioCitas.BuscarPorId(id);
            if (cita is null)
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "La cita no existe." });

            await repositorioCitas.Borrar(id);
            TempData["Toast"] = "Cita eliminada correctamente.";
            return RedirectToAction("Index");
        }

        // ──── AJAX ───────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ObtenerServiciosPsicologo(int psicologoId, int? citaId = null)
        {
            var empresaId = await servicioUsuario.ObtenerEmpresaIdAsync();
            var psicServicios = await repositorioServiciosPsicologos.ObtenerServiciosDePsicologo(psicologoId, empresaId);

            HashSet<int> citaServiciosIds = new();
            if (citaId.HasValue)
            {
                citaServiciosIds = (await repositorioServiciosCitas.ObtenerServiciosDeCita(citaId.Value, empresaId))
                    .Where(s => s.Estado == 1)
                    .Select(s => s.Id)
                    .ToHashSet();
            }

            var resultado = psicServicios
                .Where(s => s.Estado)
                .Select(s => new
                {
                    id = s.Id,
                    nombreServicio = s.NombreServicio,
                    costoServicio = s.CostoServicio,
                    estado = citaServiciosIds.Contains(s.Id) ? 1 : 0
                });

            return Json(resultado);
        }

        // ──── Helpers ────────────────────────────────────────────────────────

        private async Task CargarViewBagDropdowns(int empresaId, string nombreEmpresa)
        {
            var rolPaci = await repositorioRoles.BuscarPorCodigo("PACI");
            var rolPsic = await repositorioRoles.BuscarPorCodigo("PSIC");

            var paginacionTodos = new PaginacionViewModel { RecordsPorPagina = 500 };

            var pacientes = await repositorioPacientes.Buscar(paginacionTodos, empresaId, rolPaci?.Id ?? 0);
            var psicologos = await repositorioPsicologos.Buscar(paginacionTodos, empresaId, rolPsic?.Id ?? 0);

            var listaPacientes = pacientes.Select(p => new SelectListItem($"{p.Nombres} {p.Apellidos}", p.Id.ToString())).ToList();
            listaPacientes.Insert(0, new SelectListItem("-- Seleccione un paciente --", "0"));

            var listaPsicologos = psicologos.Select(p => new SelectListItem($"{p.Nombres} {p.Apellidos}", p.Id.ToString())).ToList();
            listaPsicologos.Insert(0, new SelectListItem("-- Seleccione un psicólogo --", "0"));

            ViewBag.Pacientes = listaPacientes;
            ViewBag.Psicologos = listaPsicologos;
            ViewBag.NombreEmpresa = nombreEmpresa;
        }

        private async Task<string> GuardarArchivoPagoCita(IFormFile archivo)
        {
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pagoscitas");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var ruta = Path.Combine(carpeta, nombreArchivo);
            using var stream = new FileStream(ruta, FileMode.Create);
            await archivo.CopyToAsync(stream);
            return nombreArchivo;
        }
    }
}
