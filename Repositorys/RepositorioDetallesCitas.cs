using AppCitasPsicologia.Models.Citas;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioDetallesCitas
    {
        Task<DetallesCitas> BuscarPorCitaId(int citaId);
        Task Crear(DetallesCitas detalle);
        Task Actualizar(DetallesCitas detalle);
        Task<bool> ExistePorCitaId(int citaId);
    }

    public class RepositorioDetallesCitas : IRepositorioDetallesCitas
    {
        private readonly string connectionString;

        public RepositorioDetallesCitas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<DetallesCitas> BuscarPorCitaId(int citaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<DetallesCitas>(
                @"SELECT * FROM DETALLESCITAS WHERE CitaId = @citaId",
                new { citaId });
        }

        public async Task Crear(DetallesCitas detalle)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO DETALLESCITAS (
                    CitaId, MotivoConsulta, EstadoEmocional, NivelAnsiedad, NivelDepresion,
                    Sintomas, ObservacionesSesion, TemasAbordados, TecnicaAplicada,
                    RespuestaPaciente, Diagnostico, PlanTerapeutico, TareasAsignadas,
                    ProximoObjetivo, NivelProgreso, FechaRegistro)
                VALUES (
                    @CitaId, @MotivoConsulta, @EstadoEmocional, @NivelAnsiedad, @NivelDepresion,
                    @Sintomas, @ObservacionesSesion, @TemasAbordados, @TecnicaAplicada,
                    @RespuestaPaciente, @Diagnostico, @PlanTerapeutico, @TareasAsignadas,
                    @ProximoObjetivo, @NivelProgreso, @FechaRegistro);
                SELECT SCOPE_IDENTITY();",
                detalle);
            detalle.Id = id;
        }

        public async Task Actualizar(DetallesCitas detalle)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE DETALLESCITAS SET
                    MotivoConsulta      = @MotivoConsulta,
                    EstadoEmocional     = @EstadoEmocional,
                    NivelAnsiedad       = @NivelAnsiedad,
                    NivelDepresion      = @NivelDepresion,
                    Sintomas            = @Sintomas,
                    ObservacionesSesion = @ObservacionesSesion,
                    TemasAbordados      = @TemasAbordados,
                    TecnicaAplicada     = @TecnicaAplicada,
                    RespuestaPaciente   = @RespuestaPaciente,
                    Diagnostico         = @Diagnostico,
                    PlanTerapeutico     = @PlanTerapeutico,
                    TareasAsignadas     = @TareasAsignadas,
                    ProximoObjetivo     = @ProximoObjetivo,
                    NivelProgreso       = @NivelProgreso,
                    FechaActualizacion  = @FechaActualizacion
                WHERE CitaId = @CitaId",
                detalle);
        }

        public async Task<bool> ExistePorCitaId(int citaId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1 FROM DETALLESCITAS WHERE CitaId = @citaId",
                new { citaId });
            return existe == 1;
        }
    }
}
