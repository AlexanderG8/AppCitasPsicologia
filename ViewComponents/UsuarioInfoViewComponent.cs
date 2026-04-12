using AppCitasPsicologia.Repositorys;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppCitasPsicologia.ViewComponents
{
    public class UsuarioInfoViewModel
    {
        public string NombreCompleto { get; set; }
        public string NombreRol { get; set; }
        public char Inicial => string.IsNullOrEmpty(NombreCompleto) ? 'U' : char.ToUpper(NombreCompleto[0]);
    }

    public class UsuarioInfoViewComponent : ViewComponent
    {
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioRoles repositorioRoles;

        public UsuarioInfoViewComponent(
            IServicioUsuario servicioUsuario,
            IRepositorioUsuarios repositorioUsuarios,
            IRepositorioRoles repositorioRoles)
        {
            this.servicioUsuario = servicioUsuario;
            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioRoles = repositorioRoles;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool soloHeader = false)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var usuario = await repositorioUsuarios.BuscarPorId(usuarioId);
            var rol = usuario?.RolId > 0 ? await repositorioRoles.BuscarPorId(usuario.RolId) : null;

            var vm = new UsuarioInfoViewModel
            {
                NombreCompleto = usuario != null ? $"{usuario.Nombres} {usuario.Apellidos}".Trim() : "Usuario",
                NombreRol = rol?.NombreRol ?? "Sin rol"
            };
            return View(soloHeader ? "Header" : "Default", vm);
        }
    }
}
