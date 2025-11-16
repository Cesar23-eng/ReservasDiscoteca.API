using System.ComponentModel.DataAnnotations;
namespace ReservasDiscoteca.API.DTOs.Admin
{
    public class CrearBolicheDto
    {
        [Required] public string Nombre { get; set; }
        public string Direccion { get; set; }
    }
}