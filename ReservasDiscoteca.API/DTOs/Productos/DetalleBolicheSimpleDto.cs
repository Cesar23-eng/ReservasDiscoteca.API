namespace ReservasDiscoteca.API.DTOs.Productos
{
    public class DetalleBolicheSimpleDto
    {
        public int Id { get; set; } // <--- ID como int
        public string Nombre { get; set; }
        public string Direccion { get; set; }
    }
}