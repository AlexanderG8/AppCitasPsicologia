using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Roles;
using AppCitasPsicologia.Models.Usuarios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioAdministradores
    {
        Task<IEnumerable<Usuarios>> Buscar(PaginacionViewModel paginacion, int empresaId, int rolId);
        Task<int> Contar(int empresaId, int rolId);
        Task<Usuarios> BuscarPorId(int id);
    }

    public class RepositorioAdministradores : IRepositorioAdministradores
    {
        private readonly string connectionString;
        public RepositorioAdministradores(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Usuarios>> Buscar(PaginacionViewModel paginacion, int empresaId, int rolId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Usuarios>(@$"SELECT * FROM Usuarios
                                                       WHERE EmpresaId = {empresaId} and RolId = {rolId} and FechaEliminado IS NULL
                                                       ORDER BY Nombres
                                                       OFFSET {paginacion.RecordsASaltar}
                                                       ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                       ROWS ONLY");
        }

        public async Task<int> Contar(int empresaId, int rolId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) 
                  FROM Usuarios
                  WHERE EmpresaId = @EmpresaId and RolId = @RolId and FechaEliminado IS NULL", new { EmpresaId = empresaId, RolId = rolId });
        }

        public async Task<Usuarios> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuarios>(@"SELECT * FROM Usuarios WHERE Id = @Id", new { id });
        }
    }
}
