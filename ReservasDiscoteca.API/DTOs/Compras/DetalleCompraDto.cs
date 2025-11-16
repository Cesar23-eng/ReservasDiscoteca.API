using System;
using System.Collections.Generic;
namespace ReservasDiscoteca.API.DTOs.Compras
{
    // El "recibo" (para el historial del Usuario y del Staff)
    public class DetalleCompraDto
    {
        public int CompraId { get; set; } // <--- ID como int
        public DateTime FechaCompra { get; set; }
        public decimal TotalPagado { get; set; }
        public string TipoCompra { get; set; } // "Manillas" o "Reserva de Mesa"
        
        public int BolicheId { get; set; } // <--- ID como int
        public string NombreBoliche { get; set; }
        
        public int UsuarioId { get; set; } // <--- ID como int
        public string NombreUsuario { get; set; }
        public string EmailUsuario { get; set; }
        
        public string MesaReservada { get; set; } // Ej: "Mesa 10 - Pista Principal"
        
        public List<ItemCompradoDto> ManillasCompradas { get; set; } = new List<ItemCompradoDto>();
    }
}