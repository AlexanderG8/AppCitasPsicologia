using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Usuarios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioPsicologos
    {
        Task<IEnumerable<Usuarios>> Buscar(PaginacionViewModel paginacion, int empresaId, int rolId);
        Task<int> Contar(int empresaId, int rolId);
        Task<Usuarios> BuscarPorId(int id);
    }

    public class RepositorioPsicologos : IRepositorioPsicologos
    {
        private readonly string connectionString;

        public RepositorioPsicologos(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Usuarios>> Buscar(PaginacionViewModel paginacion, int empresaId, int rolId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Usuarios>(@$"SELECT * FROM USUARIOS
                                                           WHERE EmpresaId = {empresaId} AND RolId = {rolId}
                                                           AND FechaEliminado IS NULL
                                                           ORDER BY Nombres
                                                           OFFSET {paginacion.RecordsASaltar}
                                                           ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                           ROWS ONLY");
        }

        public async Task<int> Contar(int empresaId, int rolId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM USUARIOS
                  WHERE EmpresaId = @EmpresaId AND RolId = @RolId AND FechaEliminado IS NULL",
                new { EmpresaId = empresaId, RolId = rolId });
        }

        public async Task<Usuarios> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuarios>(
                "SELECT * FROM USUARIOS WHERE Id = @Id AND FechaEliminado IS NULL",
                new { id });
        }
    }
}
