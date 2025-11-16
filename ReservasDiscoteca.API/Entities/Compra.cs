using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ReservasDiscoteca.API.Entities
{
    // El "Historial" o "Recibo"
    public class Compra
    {
        [Key]
        public int Id { get; set; } // ID numérico secuencial
        public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
        public decimal TotalPagado { get; set; }
        
        // Columna para que el Staff sepa qué fue
        [Required]
        public string TipoCompra { get; set; } // "Manillas" o "Reserva de Mesa"

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; } // Clave foránea como int
        public Usuario Usuario { get; set; }

        [ForeignKey("Boliche")]
        public int BolicheId { get; set; } // Clave foránea como int
        public Boliche Boliche { get; set; }

        // Si la compra fue de manillas
        public ICollection<CompraManilla> ManillasCompradas { get; set; } = new List<CompraManilla>();
        
        // Si la compra fue de una mesa
        public Mesa MesaReservada { get; set; }
    }
}