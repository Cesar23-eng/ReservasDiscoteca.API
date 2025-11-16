using System.ComponentModel.DataAnnotations;
namespace ReservasDiscoteca.API.DTOs.Admin
{
    public class CrearManillaTipoDto
    {
        [Required] public string Nombre { get; set; }
        [Range(0, 500)] public decimal Precio { get; set; }
        [Range(0, 100)] public int Stock { get; set; }
    }
}