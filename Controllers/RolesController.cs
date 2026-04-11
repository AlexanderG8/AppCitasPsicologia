using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Roles;
using AppCitasPsicologia.Repositorys;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppCitasPsicologia.Controllers
{
    public class RolesController : Controller
    {
        private readonly IRepositorioRoles repositorioRoles;
        public RolesController(IRepositorioRoles repositorioRoles)
        {
            this.repositorioRoles = repositorioRoles;
        }
        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var roles = await repositorioRoles.Buscar(paginacion);
            var totalRoles = await repositorioRoles.Contar();
            var respuestaVM = new PaginacionRespuesta<Roles>()
            {
                Elementos = roles,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalRecords = totalRoles,
                BaseURL = Url.Action()
            };
            return View(respuestaVM);
        }

        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(Roles rol)
        {
            if (!ModelState.IsValid)
            {
                return View(rol);
            }

            #region Validaciones
            var yaExisteCodigoRol = await repositorioRoles.ExisteCodigoRol(rol.CodigoRol, rol.Id);
            if (yaExisteCodigoRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El código de rol {rol.CodigoRol} ya existe.");
            }
            var yaExisteNombreRol = await repositorioRoles.ExisteNombreRol(rol.NombreRol, rol.Id);
            if (yaExisteNombreRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El nombre de rol {rol.CodigoRol} ya existe.");
            }
            #endregion

            await repositorioRoles.Crear(rol);
            TempData["Toast"] = "Rol creado correctamente";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rol = await repositorioRoles.BuscarPorId(id);
            if (rol is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol no existe." });
            }
            return View(rol);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Roles rol)
        {
            if (!ModelState.IsValid)
            {
                return View(rol);
            }
            var rolDB = await repositorioRoles.BuscarPorId(rol.Id);
            if (rolDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol no existe." });
            }

            #region Validaciones
            var yaExisteCodigoRol = await repositorioRoles.ExisteCodigoRol(rol.CodigoRol, rol.Id);
            if (yaExisteCodigoRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El código de rol {rol.CodigoRol} ya existe.");
            }
            var yaExisteNombreRol = await repositorioRoles.ExisteNombreRol(rol.NombreRol, rol.Id);
            if (yaExisteNombreRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El nombre de rol {rol.CodigoRol} ya existe.");
            }
            #endregion

            rol.FechaActualizacion = DateTime.Now;
            await repositorioRoles.Actualizar(rol);
            TempData["Toast"] = "Rol actualizado correctamente";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BorrarRol(int id)
        {
            var rol = await repositorioRoles.BuscarPorId(id);
            if (rol is null)
            {
                return RedirectToAction("NoEncontrado", "Home", new { mensaje = "El rol no existe." });
            }
            TempData["Toast"] = "Rol eliminado correctamente";
            await repositorioRoles.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteCodigoRol(string codigoRol, int id) 
        {
            var yaExisteCodigoRol = await repositorioRoles.ExisteCodigoRol(codigoRol, id);
            if (yaExisteCodigoRol)
            {
                return Json($"El código de rol {codigoRol} ya existe.");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteNombreRol(string nombreRol, int id)
        {
            var yaExisteNombreRol = await repositorioRoles.ExisteNombreRol(nombreRol, id);
            if (yaExisteNombreRol)
            {
                return Json($"El código de rol {nombreRol} ya existe.");
            }
            return Json(true);
        }
    }
}
