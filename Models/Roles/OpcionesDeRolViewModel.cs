namespace AppCitasPsicologia.Models.Roles
{
    public class OpcionesDeRolViewModel
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; }
        public IEnumerable<OpcionesRol> Opciones { get; set; }
    }
}
