using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasDiscoteca.API.Data;
using ReservasDiscoteca.API.DTOs.Productos;

namespace ReservasDiscoteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere estar logueado (cualquier rol)
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductosController(AppDbContext context) { _context = context; }

        // Ver TODOS los boliches
        [HttpGet("boliches")] // GET /api/productos/boliches
        public async Task<ActionResult<List<DetalleBolicheSimpleDto>>> GetBoliches()
        {
            return await _context.Boliches
                .Select(b => new DetalleBolicheSimpleDto
                {
                    Id = b.Id,
                    Nombre = b.Nombre,
                    Direccion = b.Direccion
                })
                .ToListAsync();
        }

        // Ver UN boliche CON TODOS sus productos disponibles
        [HttpGet("boliches/{bolicheId}")] // GET /api/productos/boliches/ID
        public async Task<ActionResult<DetalleBolicheDto>> GetBolicheDetalle(int bolicheId)
        {
            var boliche = await _context.Boliches
                .Where(b => b.Id == bolicheId)
                .Select(b => new DetalleBolicheDto
                {
                    Id = b.Id,
                    Nombre = b.Nombre,
                    Direccion = b.Direccion,
                    // Cargamos solo manillas con stock
                    Manillas = b.ManillaTipos
                        .Where(m => m.Stock > 0) 
                        .Select(m => new DetalleManillaTipoDto
                        {
                            Id = m.Id,
                            Nombre = m.Nombre,
                            Precio = m.Precio,
                            Stock = m.Stock,
                            BolicheId = m.BolicheId
                        }).ToList(),
                    // Cargamos solo mesas disponibles
                    Mesas = b.Mesas
                        .Where(m => m.EstaDisponible) 
                        .Select(m => new DetalleMesaDto
                        {
                            Id = m.Id,
                            NombreONumero = m.NombreONumero,
                            Ubicacion = m.Ubicacion,
                            PrecioReserva = m.PrecioReserva,
                            EstaDisponible = m.EstaDisponible,
                            BolicheId = m.BolicheId
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (boliche == null) return NotFound("Boliche no encontrado.");
            return Ok(boliche);
        }
    }
}