using AppCitasPsicologia.Models.Paginacion;
using AppCitasPsicologia.Models.Usuarios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioUsuarios
    {
        Task<IEnumerable<Usuarios>> Buscar(int empresaId, PaginacionViewModel paginacion);
        Task<int> Contar(int empresaId);
        Task<int> CrearUsuario(Usuarios usuario);
        Task<Usuarios> BuscarPorId(int id);
        Task Actualizar(Usuarios usuario);
        Task Borrar(int id);
        Task<Usuarios> BuscarUsuarioPorEmail(string email);
        Task GuardarTokenActivacion(int id, string token, DateTime expiracion);
        Task<Usuarios> BuscarPorToken(string token);
        Task ActualizarContrasena(int id, string contrasenaHash);
        Task<bool> ExisteNroDocumento(string numeroDocumento, int id = 0);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Usuarios>> Buscar(int empresaId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Usuarios>(
                @$"SELECT u.Id, u.EmpresaId, u.RolId, u.Nombres, u.Apellidos, u.FechaNacimiento,
                          u.TipoDocumento, u.NroDocumento, u.Email, u.NroCelular, u.Direccion,
                          u.FechaCreacion, u.FechaActualizacion, r.NombreRol
                   FROM USUARIOS u
                   INNER JOIN Roles r ON u.RolId = r.Id
                   WHERE u.EmpresaId = @EmpresaId AND u.FechaEliminado IS NULL
                   ORDER BY u.Nombres, u.Apellidos
                   OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY",
                new { empresaId });
        }

        public async Task<int> Contar(int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM USUARIOS WHERE EmpresaId = @EmpresaId AND FechaEliminado IS NULL",
                new { empresaId });
        }

        public async Task<int> CrearUsuario(Usuarios usuario)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO USUARIOS (EmpresaId, RolId, Nombres, Apellidos, FechaNacimiento, TipoDocumento,
                                        NroDocumento, Email, NroCelular, Direccion, FechaCreacion)
                  VALUES (@EmpresaId, @RolId, @Nombres, @Apellidos, @FechaNacimiento, @TipoDocumento,
                          @NroDocumento, @Email, @NroCelular, @Direccion, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", usuario);
            return id;
        }

        public async Task<Usuarios> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuarios>(
                @"SELECT u.*, r.NombreRol
                  FROM USUARIOS u
                  INNER JOIN Roles r ON u.RolId = r.Id
                  WHERE u.Id = @Id AND u.FechaEliminado IS NULL", new { id });
        }

        public async Task Actualizar(Usuarios usuario)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE USUARIOS SET RolId = @RolId, Nombres = @Nombres, Apellidos = @Apellidos,
                  FechaNacimiento = @FechaNacimiento, TipoDocumento = @TipoDocumento,
                  NroDocumento = @NroDocumento, Email = @Email, NroCelular = @NroCelular,
                  Direccion = @Direccion, FechaActualizacion = @FechaActualizacion
                  WHERE Id = @Id", usuario);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE USUARIOS SET FechaEliminado = GETDATE() WHERE Id = @Id", new { id });
        }

        public async Task<Usuarios> BuscarUsuarioPorEmail(string email)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuarios>(
                "SELECT * FROM USUARIOS WHERE Email = @Email AND FechaEliminado IS NULL",
                new { Email = email });
        }

        public async Task GuardarTokenActivacion(int id, string token, DateTime expiracion)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE USUARIOS SET TokenActivacion = @token, TokenExpiracion = @expiracion WHERE Id = @id",
                new { id, token, expiracion });
        }

        public async Task<Usuarios> BuscarPorToken(string token)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuarios>(
                "SELECT * FROM USUARIOS WHERE TokenActivacion = @token AND TokenExpiracion > GETDATE() AND FechaEliminado IS NULL",
                new { token });
        }

        public async Task ActualizarContrasena(int id, string contrasenaHash)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE USUARIOS SET ContrasenaHash = @contrasenaHash, TokenActivacion = NULL, TokenExpiracion = NULL WHERE Id = @id",
                new { id, contrasenaHash });
        }

        public async Task<bool> ExisteNroDocumento(string numeroDocumento, int id = 0)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM Usuarios WHERE NroDocumento = @NroDocumento AND Id <> @Id  AND FechaEliminado IS NULL", new { NroDocumento = numeroDocumento, Id = id });
            return existe == 1;
        }
    }
}
