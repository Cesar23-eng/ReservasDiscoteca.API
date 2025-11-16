using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasDiscoteca.API.Data;
using ReservasDiscoteca.API.DTOs.Compras;
using ReservasDiscoteca.API.Entities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReservasDiscoteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Usuario")] // ¡SOLO USUARIOS!
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ComprasController(AppDbContext context) { _context = context; }

        // --- ACCIÓN 1: COMPRAR MANILLAS ---
        [HttpPost("manillas")] // POST /api/compras/manillas
        public async Task<ActionResult<DetalleCompraDto>> ComprarManillas(CrearCompraManillasDto compraDto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalPagado = 0;
                var boliche = await _context.Boliches.FindAsync(compraDto.BolicheId);
                if (boliche == null) return NotFound("Boliche no encontrado.");

                var nuevaCompra = new Compra
                {
                    UsuarioId = usuarioId,
                    BolicheId = compraDto.BolicheId,
                    TipoCompra = "Manillas" // Lógica de negocio
                };

                var manillasCompradas = new List<CompraManilla>();
                foreach (var item in compraDto.Manillas)
                {
                    var manillaTipo = await _context.ManillaTipos.FindAsync(item.ManillaTipoId);
                    if (manillaTipo == null) throw new Exception("Manilla no encontrada.");
                    if (manillaTipo.BolicheId != boliche.Id) throw new Exception("Manilla no pertenece a este boliche.");
                    if (manillaTipo.Stock < item.Cantidad) throw new Exception($"No hay stock para '{manillaTipo.Nombre}'.");

                    manillaTipo.Stock -= item.Cantidad; // Descontar stock
                    totalPagado += manillaTipo.Precio * item.Cantidad;

                    manillasCompradas.Add(new CompraManilla
                    {
                        Compra = nuevaCompra,
                        ManillaTipoId = item.ManillaTipoId,
                        Cantidad = item.Cantidad,
                        PrecioEnElMomento = manillaTipo.Precio
                    });
                }

                nuevaCompra.TotalPagado = totalPagado;
                nuevaCompra.ManillasCompradas = manillasCompradas;
                _context.Compras.Add(nuevaCompra);
                
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync(); // Confirmar transacción

                return await GetDetalleCompraDto(nuevaCompra.Id); // Devolver recibo
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync(); // Revertir todo si algo falló
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // --- ACCIÓN 2: RESERVAR MESA ---
        [HttpPost("mesas")] // POST /api/compras/mesas
        public async Task<ActionResult<DetalleCompraDto>> ReservarMesa(ReservarMesaDto reservaDto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var boliche = await _context.Boliches.FindAsync(reservaDto.BolicheId);
                if (boliche == null) return NotFound("Boliche no encontrado.");
                
                var mesa = await _context.Mesas.FindAsync(reservaDto.MesaId);
                if (mesa == null) throw new Exception("Mesa no encontrada.");
                if (mesa.BolicheId != boliche.Id) throw new Exception("Mesa no pertenece a este boliche.");
                if (!mesa.EstaDisponible) throw new Exception($"La mesa '{mesa.NombreONumero}' ya no está disponible.");

                var nuevaCompra = new Compra
                {
                    UsuarioId = usuarioId,
                    BolicheId = reservaDto.BolicheId,
                    TipoCompra = "Reserva de Mesa", // Lógica de negocio
                    TotalPagado = mesa.PrecioReserva
                };
                _context.Compras.Add(nuevaCompra);
                
                // Asignamos la mesa a la compra y la marcamos como "No Disponible"
                mesa.EstaDisponible = false;
                mesa.Compra = nuevaCompra; // Esto setea el CompraId en la mesa
                
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync(); // Confirmar transacción

                return await GetDetalleCompraDto(nuevaCompra.Id); // Devolver recibo
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync(); // Revertir todo si algo falló
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // --- HISTORIAL DEL PROPIO USUARIO ---
        [HttpGet("mi-historial")] // GET /api/compras/mi-historial
        public async Task<ActionResult<List<DetalleCompraDto>>> GetMiHistorial()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            return await _context.Compras
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Boliche)
                .Include(c => c.Usuario)
                .Include(c => c.MesaReservada)
                .Include(c => c.ManillasCompradas).ThenInclude(mc => mc.ManillaTipo)
                .Select(c => new DetalleCompraDto
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
                .OrderByDescending(h => h.FechaCompra)
                .ToListAsync();
        }

        // Método privado para obtener el DTO de una compra (para reusar)
        private async Task<DetalleCompraDto> GetDetalleCompraDto(int compraId)
        {
            var c = await _context.Compras
                .AsNoTracking() // No necesitamos rastrear cambios, solo leer
                .Include(c => c.Boliche)
                .Include(c => c.Usuario)
                .Include(c => c.MesaReservada)
                .Include(c => c.ManillasCompradas).ThenInclude(mc => mc.ManillaTipo)
                .FirstOrDefaultAsync(c => c.Id == compraId);

            return new DetalleCompraDto
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
            };
        }
    }
}