using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.ViewComponents
{
    public class NavegacionViewComponent : ViewComponent
    {
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioOpciones repositorioOpciones;

        public NavegacionViewComponent(IServicioUsuario servicioUsuario, IRepositorioOpciones repositorioOpciones)
        {
            this.servicioUsuario = servicioUsuario;
            this.repositorioOpciones = repositorioOpciones;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rolId = await servicioUsuario.ObtenerRolIdAsync();
            var opciones = await repositorioOpciones.ObtenerOpcionesPorRol(rolId);
            return View(opciones);
        }
    }
}
