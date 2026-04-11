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
        // Detalle suscripciones
        Task<IEnumerable<DetalleSuscripciones>> BuscarDetallesSuscripcionEmpresa(int empresaId, PaginacionViewModel paginacion);
        Task<int> ContarDetallesSuscripcionEmpresa(int empresaId);
        Task<DetalleSuscripciones> BuscarDetalleSuscripcionPorId(int id);
        Task<DetalleSuscripciones> CrearDetalleSuscripcionEmpresa(DetalleSuscripciones detalle);
        Task ActualizarDetalleSuscripcionEmpresa(DetalleSuscripciones detalle);
        Task BorrarDetalleSuscripcionEmpresa(int id);
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
                                                            WHERE FechaEliminado IS NULL
                                                            ORDER BY NombreEmpresa
                                                            OFFSET {paginacion.RecordsASaltar}
                                                            ROWS FETCH NEXT {paginacion.RecordsPorPagina}
                                                            ROWS ONLY");
        }

        public async Task<int> Contar()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EMPRESAS WHERE FechaEliminado IS NULL");
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
            await connection.ExecuteAsync("UPDATE EMPRESAS SET FechaEliminado = @FechaEliminado WHERE Id = @Id", new { id, FechaEliminado = DateTime.UtcNow });
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

        public async Task<IEnumerable<DetalleSuscripciones>> BuscarDetallesSuscripcionEmpresa(int empresaId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<DetalleSuscripciones>(
                @$"SELECT ds.Id, ds.EmpresaId, ds.SuscripcionId, ds.FechaInicio, ds.FechaFin,
                         ds.DocPago, ds.FechaCreacion, ds.FechaActualizacion, ds.FechaEliminado,
                         s.NombreSuscripcion
                  FROM DetalleSuscripciones ds
                  INNER JOIN SUSCRIPCIONES s ON ds.SuscripcionId = s.Id
                  WHERE ds.EmpresaId = @EmpresaId AND ds.FechaEliminado IS NULL
                  ORDER BY ds.FechaInicio DESC
                  OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY",
                new { empresaId });
        }

        public async Task<int> ContarDetallesSuscripcionEmpresa(int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM DetalleSuscripciones WHERE EmpresaId = @EmpresaId AND FechaEliminado IS NULL",
                new { empresaId });
        }

        public async Task<DetalleSuscripciones> BuscarDetalleSuscripcionPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<DetalleSuscripciones>(
                @"SELECT ds.Id, ds.EmpresaId, ds.SuscripcionId, ds.FechaInicio, ds.FechaFin,
                         ds.DocPago, ds.FechaCreacion, ds.FechaActualizacion, ds.FechaEliminado,
                         s.NombreSuscripcion
                  FROM DetalleSuscripciones ds
                  INNER JOIN SUSCRIPCIONES s ON ds.SuscripcionId = s.Id
                  WHERE ds.Id = @Id AND ds.FechaEliminado IS NULL", new { id });
        }

        public async Task<DetalleSuscripciones> CrearDetalleSuscripcionEmpresa(DetalleSuscripciones detalle)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO DetalleSuscripciones (EmpresaId, SuscripcionId, FechaInicio, FechaFin, DocPago, FechaCreacion)
                  VALUES (@EmpresaId, @SuscripcionId, @FechaInicio, @FechaFin, @DocPago, @FechaCreacion);
                  SELECT SCOPE_IDENTITY();", detalle);
            detalle.Id = id;
            return detalle;
        }

        public async Task ActualizarDetalleSuscripcionEmpresa(DetalleSuscripciones detalle)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE DetalleSuscripciones
                  SET SuscripcionId = @SuscripcionId, FechaInicio = @FechaInicio,
                      FechaFin = @FechaFin, DocPago = @DocPago,
                      FechaActualizacion = @FechaActualizacion
                  WHERE Id = @Id", detalle);
        }

        public async Task BorrarDetalleSuscripcionEmpresa(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "UPDATE DetalleSuscripciones SET FechaEliminado = @FechaEliminado WHERE Id = @Id", new { id, FechaEliminado = DateTime.UtcNow });
        }
    }
}
