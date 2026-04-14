using AppCitasPsicologia.Models.Citas;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioServiciosCitas
    {
        Task<IEnumerable<ServicioCitaItem>> ObtenerServiciosDeCita(int citaId, int empresaId);
        Task GuardarServiciosCita(int citaId, List<int> serviciosSeleccionados);
    }

    public class RepositorioServiciosCitas : IRepositorioServiciosCitas
    {
        private readonly string connectionString;

        public RepositorioServiciosCitas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<ServicioCitaItem>> ObtenerServiciosDeCita(int citaId, int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ServicioCitaItem>(@"
                SELECT S.Id, S.NombreServicio, S.CostoServicio,
                       CASE WHEN EXISTS (
                           SELECT 1 FROM SERVICIOSCITAS SC
                           WHERE SC.ServicioId = S.Id AND SC.CitaId = @citaId
                       ) THEN 1 ELSE 0 END AS Estado
                FROM SERVICIOS S
                WHERE S.EmpresaId = @empresaId AND S.FechaEliminado IS NULL
                ORDER BY S.NombreServicio",
                new { citaId, empresaId });
        }

        public async Task GuardarServiciosCita(int citaId, List<int> serviciosSeleccionados)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(
                "DELETE FROM SERVICIOSCITAS WHERE CitaId = @citaId",
                new { citaId }, transaction);

            if (serviciosSeleccionados.Count > 0)
            {
                var filas = serviciosSeleccionados.Select(servicioId => new { citaId, servicioId });
                await connection.ExecuteAsync(
                    "INSERT INTO SERVICIOSCITAS (CitaId, ServicioId) VALUES (@citaId, @servicioId)",
                    filas, transaction);
            }

            transaction.Commit();
        }
    }
}
