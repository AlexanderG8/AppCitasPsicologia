using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Roles;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioRoles
    {
        Task Actualizar(Roles rol);
        Task Borrar(int id);
        Task<IEnumerable<Roles>> Buscar(PaginacionViewModel paginacion);
        Task<int> Contar();
        Task<Roles> BuscarPorId(int id);
        Task<Roles> BuscarPorCodigo(string codigo);
        Task<Roles> Crear(Roles rol);
        Task<bool> ExisteCodigoRol(string codigoRol, int id);
        Task<bool> ExisteNombreRol(string nombreRol, int id);
        Task<IEnumerable<OpcionesRol>> ObtenerOpcionesDeRol(int id);
        Task GuardarOpcionesDeRol(int rolId, List<int> opcionesSeleccionadas);
    }
    public class RepositorioRoles : IRepositorioRoles
    {
        private readonly string connectionString;
        public RepositorioRoles(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Roles>> Buscar(PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Roles>(@$"SELECT * FROM Roles
                                                       WHERE FechaEliminado IS NULL
                                                       ORDER BY NombreRol
                                                       OFFSET {paginacion.RecordsASaltar}
                                                       ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                       ROWS ONLY");
        }

        public async Task<int> Contar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) 
                                                             FROM Roles 
                                                             WHERE FechaEliminado IS NULL");
        }

        public async Task<Roles> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Roles>(@"SELECT * FROM Roles WHERE Id = @Id", new { id });
        }

        public async Task<Roles> BuscarPorCodigo(string codigo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Roles>(@"SELECT * FROM Roles WHERE CodigoRol = @CodigoRol", new { CodigoRol = codigo });
        }

        public async Task<Roles> Crear(Roles rol)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Roles (CodigoRol, NombreRol, FechaCreacion) VALUES (@CodigoRol, @NombreRol, @FechaCreacion); SELECT SCOPE_IDENTITY();", rol);
            rol.Id = id;
            return rol;
        }

        public async Task Actualizar(Roles rol)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Roles SET CodigoRol = @CodigoRol, NombreRol = @NombreRol, FechaActualizacion = @FechaActualizacion WHERE Id = @Id", rol);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Roles SET FechaEliminado = GETDATE() WHERE Id = @Id", new { id });
        }

        public async Task<bool> ExisteCodigoRol(string codigoRol, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Roles WHERE CodigoRol = @CodigoRol AND Id <> @Id", new { codigoRol, id });
            return existe == 1;
        }

        public async Task<bool> ExisteNombreRol(string nombreRol, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Roles WHERE NombreRol = @NombreRol AND Id <> @Id", new { nombreRol, id });
            return existe == 1;
        }

        public async Task<IEnumerable<OpcionesRol>> ObtenerOpcionesDeRol(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var opcionesRol = await connection.QueryAsync<OpcionesRol>(@"SELECT
                                                                         O.Id,
                                                                         O.NombreOpcion,
                                                                         CASE WHEN EXISTS(SELECT 1 FROM OPCIONESROL RO WHERE RO.OpcionId = O.Id AND RO.RolId = @Id) THEN 1 ELSE 0 END AS Estado
                                                                         FROM OPCIONES O
                                                                         WHERE O.FechaEliminado IS NULL
                                                                         ORDER BY O.NombreOpcion", new { id });
            return opcionesRol;
        }

        public async Task GuardarOpcionesDeRol(int rolId, List<int> opcionesSeleccionadas)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            await connection.ExecuteAsync(
                "DELETE FROM OPCIONESROL WHERE RolId = @rolId",
                new { rolId }, transaction);
            if (opcionesSeleccionadas.Count > 0)
            {
                var filas = opcionesSeleccionadas.Select(opcionId => new { rolId, opcionId });
                await connection.ExecuteAsync(
                    "INSERT INTO OPCIONESROL (RolId, OpcionId) VALUES (@rolId, @opcionId)",
                    filas, transaction);
            }
            transaction.Commit();
        }
    }
}
