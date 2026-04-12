using AppCitasPsicologia.Repositorys;
using System.Security.Claims;

namespace ManejoPresupuesto.Services
{
    public interface IServicioUsuario
    {
        int ObtenerUsuarioId();
        Task<int> ObtenerEmpresaIdAsync();
        Task<int> ObtenerRolIdAsync();
    }

    public class ServicioUsuarios : IServicioUsuario
    {
        private readonly HttpContext httpContext;
        private readonly IRepositorioUsuarios repositorioUsuarios;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor, IRepositorioUsuarios repositorioUsuarios)
        {
            httpContext = httpContextAccessor.HttpContext;
            this.repositorioUsuarios = repositorioUsuarios;
        }

        public int ObtenerUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                return int.Parse(idClaim.Value);
            }
            throw new ApplicationException("El usuario no está autenticado");
        }

        public async Task<int> ObtenerEmpresaIdAsync()
        {
            var usuario = await repositorioUsuarios.BuscarPorId(ObtenerUsuarioId());
            return usuario?.EmpresaId ?? 0;
        }

        public async Task<int> ObtenerRolIdAsync()
        {
            var usuario = await repositorioUsuarios.BuscarPorId(ObtenerUsuarioId());
            return usuario?.RolId ?? 0;
        }
    }
}
