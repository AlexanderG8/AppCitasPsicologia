using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Roles;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioOpciones
    {
        Task Actualizar(Opciones opcion);
        Task Borrar(int id);
        Task<IEnumerable<Opciones>> Buscar(PaginacionViewModel paginacion);
        Task<int> Contar();
        Task<Opciones> BuscarPorId(int id);
        Task<Opciones> Crear(Opciones opcion);
        Task<bool> ExisteNombreOpcion(string nombreOpcion, int id);
    }
    public class RepositorioOpciones : IRepositorioOpciones
    {
        private readonly string connectionString;
        public RepositorioOpciones(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Opciones>> Buscar(PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Opciones>(@$"SELECT * FROM Opciones
                                                       ORDER BY NombreOpcion
                                                       OFFSET {paginacion.RecordsASaltar}
                                                       ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                       ROWS ONLY");
        }

        public async Task<int> Contar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) 
                  FROM Opciones");
        }

        public async Task<Opciones> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Opciones>(@"SELECT * FROM Opciones WHERE Id = @Id", new { id });
        }

        public async Task<Opciones> Crear(Opciones opcion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Opciones (NombreOpcion, Controlador, Accion, FechaCreacion) VALUES (@NombreOpcion, @Controlador, @Accion, @FechaCreacion); SELECT SCOPE_IDENTITY();", opcion);
            opcion.Id = id;
            return opcion;
        }

        public async Task Actualizar(Opciones opcion)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Opciones SET NombreOpcion = @NombreOpcion, Controlador = @Controlador, Accion = @Accion, FechaActualizacion = @FechaActualizacion WHERE Id = @Id", opcion);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE FROM Opciones WHERE Id = @Id", new { id });
        }

        public async Task<bool> ExisteNombreOpcion(string nombreOpcion, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Opciones WHERE NombreOpcion = @NombreOpcion AND Id <> @Id", new { nombreOpcion, id });
            return existe == 1;
        }
    }
}
