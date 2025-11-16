using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasDiscoteca.API.Data;
using ReservasDiscoteca.API.DTOs.Compras;
using System.Linq;
using System.Threading.Tasks;

namespace ReservasDiscoteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Administrador")] // Staff y Admin pueden ver
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;
        public StaffController(AppDbContext context) { _context = context; }

        // Endpoint para que el Staff vea el historial de compras
        [HttpGet("historial")] // GET /api/staff/historial
        public async Task<ActionResult> GetHistorialCompras()
        {
            var historial = await _context.Compras
                .AsNoTracking() // Es solo lectura
                .Include(c => c.Usuario) 
                .Include(c => c.Boliche) 
                .Include(c => c.MesaReservada) 
                .Include(c => c.ManillasCompradas)
                    .ThenInclude(mc => mc.ManillaTipo)
                .Select(c => new DetalleCompraDto // Reusamos el DTO de "recibo"
                {
                    CompraId = c.Id,
                    FechaCompra = c.FechaCompra,
                    TotalPagado = c.TotalPagado,
                    TipoCompra = c.TipoCompra,
                    BolicheId = c.Boliche.Id,
                    NombreBoliche = c.Boliche.Nombre,
                    UsuarioId = c.Usuario.Id,
                    NombreUsuario = c.Usuario.Nombre,
                    EmailUsuario = c.Usuario.Email,
                    MesaReservada = c.MesaReservada != null 
                        ? $"{c.MesaReservada.NombreONumero} - {c.MesaReservada.Ubicacion}" 
                        : null,
                    ManillasCompradas = c.ManillasCompradas.Select(mc => new ItemCompradoDto
                    {
                        NombreManilla = mc.ManillaTipo.Nombre,
                        Cantidad = mc.Cantidad,
                        PrecioPagado = mc.PrecioEnElMomento
                    }).ToList()
                })
                .OrderByDescending(h => h.FechaCompra) // Más nuevas primero
                .ToListAsync();

            return Ok(historial);
        }
    }
}