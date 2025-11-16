using System.Collections.Generic;
namespace ReservasDiscoteca.API.DTOs.Productos
{
    public class DetalleBolicheDto
    {
        public int Id { get; set; } // <--- ID como int
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public List<DetalleManillaTipoDto> Manillas { get; set; } = new List<DetalleManillaTipoDto>();
        public List<DetalleMesaDto> Mesas { get; set; } = new List<DetalleMesaDto>();
    }
}