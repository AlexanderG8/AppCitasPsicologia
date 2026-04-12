using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Servicios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioServicios
    {
        Task<IEnumerable<Servicios>> Buscar(int empresaId, PaginacionViewModel paginacion);
        Task<int> Contar(int empresaId);
        Task<Servicios> BuscarPorId(int id);
        Task<int> Crear(Servicios servicio);
        Task Actualizar(Servicios servicio);
        Task Borrar(int id);
    }

    public class RepositorioServicios : IRepositorioServicios
    {
        private readonly string connectionString;

        public RepositorioServicios(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Servicios>> Buscar(int empresaId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Servicios>(
                @$"SELECT Id, EmpresaId, NombreServicio, CostoServicio, Descripcion, FechaCreacion, FechaActualizacion
                   FROM SERVICIOS
                   WHERE EmpresaId = @EmpresaId AND FechaEliminado IS NULL
                   ORDER BY NombreServicio
                   OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY",
                new { empresaId });
        }

        public async Task<int> Contar(int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SERVICIOS WHERE EmpresaId = @EmpresaId AND FechaEliminado IS NULL",
                new { empresaId });
        }

        public async Task<Servicios> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Servicios>(
                "SELECT * FROM SERVICIOS WHERE Id = @Id AND FechaEliminado IS NULL",
                new { id });
        }

        public async Task<int> Crear(Servicios servicio)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleAsync<int>(
                @"INSERT INTO SERVICIOS (EmpresaId, NombreServicio, CostoServicio, Descripcion, FechaCreacion)
                  VALUES (@EmpresaId, @NombreServicio, @CostoServicio, @Descripcion, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", servicio);
        }

        public async Task Actualizar(Servicios servicio)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE SERVICIOS SET NombreServicio = @NombreServicio, CostoServicio = @CostoServicio,
                  Descripcion = @Descripcion, FechaActualizacion = @FechaActualizacion
                  WHERE Id = @Id", servicio);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE SERVICIOS SET FechaEliminado = GETDATE() WHERE Id = @Id", new { id });
        }
    }
}
