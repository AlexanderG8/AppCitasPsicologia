using AppCitasPsicologia.Models.Empresas;
using AppCitasPsicologia.Models.Paginacion;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioSuscripciones
    {
        Task<IEnumerable<Suscripciones>> Buscar(PaginacionViewModel paginacion);
        Task<int> Contar();
        Task<Suscripciones> BuscarPorId(int id);
        Task<Suscripciones> Crear(Suscripciones suscripcion);
        Task Actualizar(Suscripciones suscripcion);
        Task Borrar(int id);
        Task<bool> ExisteNombreSuscripcion(string nombreSuscripcion, int id);
        Task<IEnumerable<Suscripciones>> BuscarTodos();
    }

    public class RepositorioSuscripciones : IRepositorioSuscripciones
    {
        private readonly string connectionString;

        public RepositorioSuscripciones(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Suscripciones>> BuscarTodos() 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Suscripciones>("SELECT * FROM SUSCRIPCIONES WHERE FechaEliminado IS NULL ORDER BY NombreSuscripcion");
        }

        public async Task<IEnumerable<Suscripciones>> Buscar(PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Suscripciones>(@$"SELECT * FROM SUSCRIPCIONES
                                                                 ORDER BY NombreSuscripcion
                                                                 OFFSET {paginacion.RecordsASaltar}
                                                                 ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                                 ROWS ONLY");
        }

        public async Task<int> Contar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SUSCRIPCIONES");
        }

        public async Task<Suscripciones> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Suscripciones>(
                "SELECT * FROM SUSCRIPCIONES WHERE Id = @Id", new { id });
        }

        public async Task<Suscripciones> Crear(Suscripciones suscripcion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO SUSCRIPCIONES (NombreSuscripcion, CantMeses, FechaCreacion)
                  VALUES (@NombreSuscripcion, @CantMeses, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", suscripcion);
            suscripcion.Id = id;
            return suscripcion;
        }

        public async Task Actualizar(Suscripciones suscripcion)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE SUSCRIPCIONES SET NombreSuscripcion = @NombreSuscripcion,
                  CantMeses = @CantMeses, FechaActualizacion = @FechaActualizacion
                  WHERE Id = @Id", suscripcion);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM SUSCRIPCIONES WHERE Id = @Id", new { id });
        }

        public async Task<bool> ExisteNombreSuscripcion(string nombreSuscripcion, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM SUSCRIPCIONES WHERE NombreSuscripcion = @NombreSuscripcion AND Id <> @Id",
                new { nombreSuscripcion, id });
            return existe == 1;
        }
    }
}
