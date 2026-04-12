namespace AppCitasPsicologia.Models.Servicios
{
    public class ServiciosPsicologosViewModel
    {
        public int IdPsicologo { get; set; }
        public string NombrePsicologo { get; set; }
        public IEnumerable<ServicioPsicologoItem> Servicios { get; set; }
    }
}
