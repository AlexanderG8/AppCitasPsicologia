using AppCitasPsicologia.Models.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCitasPsicologia.Models.Usuarios
{
    public class Usuarios
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int RolId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Nombres { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Apellidos { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime FechaNacimiento { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string TipoDocumento { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Remote(action: "VerificarNroDocumento", controller: "Usuarios", AdditionalFields = nameof(Id))]
        public string NroDocumento { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NroCelular { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120)]
        [PrimeraLetraMayuscula]
        public string Direccion { get; set; }
        public string ContrasenaHash { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaEliminado { get; set; }
        public string TokenActivacion { get; set; }
        public DateTime? TokenExpiracion { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }
        [NotMapped]
        public string ConfirmarContrasena { get; set; }
        [NotMapped]
        public string NombreRol { get; set; }
    }
}
