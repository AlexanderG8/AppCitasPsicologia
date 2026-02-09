using AppCitasPsicologia.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioRoles
    {
        Task Actualizar(Roles rol);
        Task Borrar(int id);
        Task<IEnumerable<Roles>> Buscar();
        Task<Roles> BuscarPorId(int id);
        Task<Roles> Crear(Roles rol);
        Task<bool> ExisteCodigoRol(string codigoRol);
        Task<bool> ExisteNombreRol(string nombreRol);
    }
    public class RepositorioRoles : IRepositorioRoles
    {
        private readonly string connectionString;
        public RepositorioRoles(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Roles>> Buscar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Roles>(@"SELECT * FROM Roles");
        }

        public async Task<Roles> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Roles>(@"SELECT * FROM Roles WHERE Id = @Id", new { id });
        }

        public async Task<Roles> Crear(Roles rol)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Roles (CodigoRol, NombreRol) VALUES (@CodigoRol, @NombreRol); SELECT SCOPE_IDENTITY();", rol);
            rol.Id = id;
            return rol;
        }

        public async Task Actualizar(Roles rol)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Roles SET CodigoRol = @CodigoRol, NombreRol = @NombreRol WHERE Id = @Id", rol);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE FROM Roles WHERE Id = @Id", new { id });
        }

        public async Task<bool> ExisteCodigoRol(string codigoRol)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Roles WHERE CodigoRol = @CodigoRol", new { codigoRol });
            return existe == 1;
        }

        public async Task<bool> ExisteNombreRol(string nombreRol)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Roles WHERE NombreRol = @NombreRol", new { nombreRol });
            return existe == 1;
        }
    }
}
