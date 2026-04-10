namespace AppCitasPsicologia.Models.Servicios
{
    public class ServiciosPsicologos
    {
        public int Id { get; set; }
        public int PsicologoId { get; set; }
        public int ServicioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
    }
}
