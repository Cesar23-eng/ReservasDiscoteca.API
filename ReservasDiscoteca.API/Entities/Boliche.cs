using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ReservasDiscoteca.API.Entities
{
    public class Boliche
    {
        [Key]
        public int Id { get; set; } // ID numérico secuencial
        [Required]
        public string Nombre { get; set; }
        public string Direccion { get; set; }

        public ICollection<ManillaTipo> ManillaTipos { get; set; } = new List<ManillaTipo>();
        public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
    }
}