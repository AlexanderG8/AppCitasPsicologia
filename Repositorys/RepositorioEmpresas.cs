using AppCitasPsicologia.Models.Empresas;
using AppCitasPsicologia.Models.Paginacion;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioEmpresas
    {
        Task<IEnumerable<Empresas>> Buscar(PaginacionViewModel paginacion);
        Task<int> Contar();
        Task<Empresas> BuscarPorId(int id);
        Task<Empresas> Crear(Empresas empresa);
        Task Actualizar(Empresas empresa);
        Task Borrar(int id);
        Task<bool> ExisteRuc(string ruc, int id);
        Task<bool> ExisteNombreEmpresa(string nombreEmpresa, int id);
        Task<DetalleSuscripciones> GuardarDetalleSuscripcionEmpresa(DetalleSuscripciones detalle);
        Task<DetalleSuscripciones> BuscarDetalleSuscripcionEmpresa(int empresaId);
    }

    public class RepositorioEmpresas : IRepositorioEmpresas
    {
        private readonly string connectionString;

        public RepositorioEmpresas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Empresas>> Buscar(PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Empresas>(@$"SELECT * FROM EMPRESAS
                                                            ORDER BY NombreEmpresa
                                                            OFFSET {paginacion.RecordsASaltar}
                                                            ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                            ROWS ONLY");
        }

        public async Task<int> Contar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EMPRESAS");
        }

        public async Task<Empresas> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Empresas>(
                "SELECT * FROM EMPRESAS WHERE Id = @Id", new { id });
        }

        public async Task<Empresas> Crear(Empresas empresa)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO EMPRESAS (Ruc, NombreEmpresa, Encargado, Direccion, FechaCreacion)
                  VALUES (@Ruc, @NombreEmpresa, @Encargado, @Direccion, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", empresa);
            empresa.Id = id;
            return empresa;
        }

        public async Task Actualizar(Empresas empresa)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE EMPRESAS SET Ruc = @Ruc, NombreEmpresa = @NombreEmpresa,
                  Encargado = @Encargado, Direccion = @Direccion,
                  FechaActualizacion = @FechaActualizacion
                  WHERE Id = @Id", empresa);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM EMPRESAS WHERE Id = @Id", new { id });
        }

        public async Task<bool> ExisteRuc(string ruc, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM EMPRESAS WHERE Ruc = @Ruc AND Id <> @Id", new { ruc, id });
            return existe == 1;
        }

        public async Task<bool> ExisteNombreEmpresa(string nombreEmpresa, int id)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM EMPRESAS WHERE NombreEmpresa = @NombreEmpresa AND Id <> @Id",
                new { nombreEmpresa, id });
            return existe == 1;
        }

        public async Task<DetalleSuscripciones> GuardarDetalleSuscripcionEmpresa(DetalleSuscripciones detalle) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO DetalleSuscripciones (EmpresaId, SuscripcionId, FechaInicio, FechaFin, DocPago, FechaCreacion)
                  VALUES (@EmpresaId, @SuscripcionId, @FechaInicio, @FechaFin, @DocPago, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", detalle);
            detalle.Id = id;
            return detalle;
        }

        public async Task<DetalleSuscripciones> BuscarDetalleSuscripcionEmpresa(int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<DetalleSuscripciones>(
                "SELECT * FROM DetalleSuscripciones WHERE EmpresaId = @EmpresaId AND FechaEliminado IS NULL", new { empresaId });
        }
    }
}
