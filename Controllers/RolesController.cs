using AppCitasPsicologia.Models;
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
        public async Task<IActionResult> Index()
        {
            var roles = await repositorioRoles.Buscar();
            return View(roles);
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
            var yaExisteCodigoRol = await repositorioRoles.ExisteCodigoRol(rol.CodigoRol);
            if (yaExisteCodigoRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El código de rol {rol.CodigoRol} ya existe.");
            }
            var yaExisteNombreRol = await repositorioRoles.ExisteNombreRol(rol.NombreRol);
            if (yaExisteNombreRol)
            {
                ModelState.AddModelError(nameof(rol.CodigoRol), $"El nombre de rol {rol.CodigoRol} ya existe.");
            }
            await repositorioRoles.Crear(rol);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rol = await repositorioRoles.BuscarPorId(id);
            if (rol is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
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
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioRoles.Actualizar(rol);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var rol = await repositorioRoles.BuscarPorId(id);
            if (rol is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(rol);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarRol(int id)
        {
            var rol = await repositorioRoles.BuscarPorId(id);
            if (rol is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioRoles.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteCodigoRol(string codigoRol) 
        {
            var yaExisteCodigoRol = await repositorioRoles.ExisteCodigoRol(codigoRol);
            if (yaExisteCodigoRol)
            {
                return Json($"El código de rol {codigoRol} ya existe.");
            }
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteNombreRol(string nombreRol)
        {
            var yaExisteNombreRol = await repositorioRoles.ExisteNombreRol(nombreRol);
            if (yaExisteNombreRol)
            {
                return Json($"El código de rol {nombreRol} ya existe.");
            }
            return Json(true);
        }
    }
}
