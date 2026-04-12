using AppCitasPsicologia.Models.Servicios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AppCitasPsicologia.Repositorys
{
    public interface IRepositorioServiciosPsicologos
    {
        Task<IEnumerable<ServicioPsicologoItem>> ObtenerServiciosDePsicologo(int psicologoId, int empresaId);
        Task GuardarServiciosPsicologo(int psicologoId, List<int> serviciosSeleccionados);
    }

    public class RepositorioServiciosPsicologos : IRepositorioServiciosPsicologos
    {
        private readonly string connectionString;

        public RepositorioServiciosPsicologos(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<ServicioPsicologoItem>> ObtenerServiciosDePsicologo(int psicologoId, int empresaId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ServicioPsicologoItem>(@"
                SELECT S.Id, S.NombreServicio, S.CostoServicio,
                       CASE WHEN EXISTS (
                           SELECT 1 FROM SERVICIOSPSICOLOGOS SP
                           WHERE SP.ServicioId = S.Id AND SP.PsicologoId = @psicologoId
                           AND SP.FechaEliminado IS NULL
                       ) THEN 1 ELSE 0 END AS Estado
                FROM SERVICIOS S
                WHERE S.EmpresaId = @empresaId AND S.FechaEliminado IS NULL
                ORDER BY S.NombreServicio",
                new { psicologoId, empresaId });
        }

        public async Task GuardarServiciosPsicologo(int psicologoId, List<int> serviciosSeleccionados)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            await connection.ExecuteAsync(
                "DELETE FROM SERVICIOSPSICOLOGOS WHERE PsicologoId = @psicologoId",
                new { psicologoId }, transaction);
            if (serviciosSeleccionados.Count > 0)
            {
                var filas = serviciosSeleccionados.Select(servicioId => new { psicologoId, servicioId, FechaCreacion = DateTime.Now });
                await connection.ExecuteAsync(
                    "INSERT INTO SERVICIOSPSICOLOGOS (PsicologoId, ServicioId, FechaCreacion) VALUES (@psicologoId, @servicioId, @FechaCreacion)",
                    filas, transaction);
            }
            transaction.Commit();
        }
    }
}
