namespace AppCitasPsicologia.Models.Citas
{
    public class ServicioCitaItem
    {
        public int Id { get; set; }
        public string NombreServicio { get; set; }
        public decimal CostoServicio { get; set; }
        public int Estado { get; set; } // 1 = seleccionado, 0 = no seleccionado
    }
}
