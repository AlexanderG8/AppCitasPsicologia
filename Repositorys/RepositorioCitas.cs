using AppCitasPsicologia.Models.Citas;
using AppCitasPsicologia.Models.Paginacion;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioCitas
    {
        Task<IEnumerable<Citas>> Buscar(PaginacionViewModel paginacion, int empresaId, int? psicologoId = null, int? clienteId = null);
        Task<int> Contar(int empresaId, int? psicologoId = null, int? clienteId = null);
        Task<Citas> BuscarPorId(int id);
        Task<int> Crear(Citas cita);
        Task Actualizar(Citas cita);
        Task Borrar(int id);
    }

    public class RepositorioCitas : IRepositorioCitas
    {
        private readonly string connectionString;

        public RepositorioCitas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Citas>> Buscar(PaginacionViewModel paginacion, int empresaId, int? psicologoId = null, int? clienteId = null)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Citas>(@$"
                SELECT C.*,
                       CL.Nombres + ' ' + CL.Apellidos AS NombreCliente,
                       PS.Nombres + ' ' + PS.Apellidos AS NombrePsicologo
                FROM CITAS C
                INNER JOIN USUARIOS CL ON C.ClienteId   = CL.Id
                INNER JOIN USUARIOS PS ON C.PsicologoId = PS.Id
                WHERE PS.EmpresaId          = @empresaId
                  AND C.FechaEliminado      IS NULL
                  AND (@psicologoId IS NULL OR C.PsicologoId = @psicologoId)
                  AND (@clienteId   IS NULL OR C.ClienteId   = @clienteId)
                ORDER BY C.FechaReserva DESC, C.HoraInicio DESC
                OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY",
                new { empresaId, psicologoId, clienteId });
        }

        public async Task<int> Contar(int empresaId, int? psicologoId = null, int? clienteId = null)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM CITAS C
                INNER JOIN USUARIOS PS ON C.PsicologoId = PS.Id
                WHERE PS.EmpresaId          = @empresaId
                  AND C.FechaEliminado      IS NULL
                  AND (@psicologoId IS NULL OR C.PsicologoId = @psicologoId)
                  AND (@clienteId   IS NULL OR C.ClienteId   = @clienteId)",
                new { empresaId, psicologoId, clienteId });
        }

        public async Task<Citas> BuscarPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Citas>(@"
                SELECT C.*,
                       CL.Nombres + ' ' + CL.Apellidos AS NombreCliente,
                       PS.Nombres + ' ' + PS.Apellidos AS NombrePsicologo
                FROM CITAS C
                INNER JOIN USUARIOS CL ON C.ClienteId   = CL.Id
                INNER JOIN USUARIOS PS ON C.PsicologoId = PS.Id
                WHERE C.Id = @id AND C.FechaEliminado IS NULL",
                new { id });
        }

        public async Task<int> Crear(Citas cita)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleAsync<int>(@"
                INSERT INTO CITAS (ClienteId, PsicologoId, DetalleCita, FechaReserva, HoraInicio, HoraFin, Asistencia, MotivoPostergacion, DocPago, FechaCreacion)
                VALUES (@ClienteId, @PsicologoId, @DetalleCita, @FechaReserva, @HoraInicio, @HoraFin, @Asistencia, @MotivoPostergacion, @DocPago, @FechaCreacion);
                SELECT SCOPE_IDENTITY();",
                cita);
        }

        public async Task Actualizar(Citas cita)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE CITAS SET
                    ClienteId           = @ClienteId,
                    PsicologoId         = @PsicologoId,
                    DetalleCita         = @DetalleCita,
                    FechaReserva        = @FechaReserva,
                    HoraInicio          = @HoraInicio,
                    HoraFin             = @HoraFin,
                    Asistencia          = @Asistencia,
                    MotivoPostergacion  = @MotivoPostergacion,
                    DocPago             = @DocPago,
                    FechaCancelacion    = @FechaCancelacion,
                    MotivoCancelacion   = @MotivoCancelacion,
                    FechaActualizacion  = @FechaActualizacion
                WHERE Id = @Id",
                cita);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE CITAS SET FechaEliminado = GETDATE() WHERE Id = @id",
                new { id });
        }
    }
}
