using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasDiscoteca.API.Data;
using ReservasDiscoteca.API.DTOs.Admin;
using ReservasDiscoteca.API.DTOs.Auth;
using ReservasDiscoteca.API.DTOs.Productos;
using ReservasDiscoteca.API.Entities;
using System.Security.Cryptography;
using System.Text;

namespace ReservasDiscoteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")] // ¡SOLO ADMINS!
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        [HttpPost("boliches")]
        public async Task<ActionResult<DetalleBolicheDto>> CrearBoliche(CrearBolicheDto bolicheDto)
        {
            var boliche = new Boliche
            {
                // El 'Id' (int) lo genera la BD
                Nombre = bolicheDto.Nombre,
                Direccion = bolicheDto.Direccion
            };
            _context.Boliches.Add(boliche);
            await _context.SaveChangesAsync();

            return Ok(new DetalleBolicheDto
            {
                Id = boliche.Id,
                Nombre = boliche.Nombre,
                Direccion = boliche.Direccion
            });
        }

        [HttpPost("boliches/{bolicheId}/manillas")]
        public async Task<ActionResult<DetalleManillaTipoDto>> CrearManilla(int bolicheId, CrearManillaTipoDto manillaDto)
        {
            if (!await _context.Boliches.AnyAsync(b => b.Id == bolicheId))
                return NotFound("Boliche no encontrado.");

            var manillaTipo = new ManillaTipo
            {
                Nombre = manillaDto.Nombre,
                Precio = manillaDto.Precio,
                Stock = manillaDto.Stock,
                BolicheId = bolicheId
            };
            _context.ManillaTipos.Add(manillaTipo);
            await _context.SaveChangesAsync();
            
            return Ok(new DetalleManillaTipoDto
            {
                Id = manillaTipo.Id,
                Nombre = manillaTipo.Nombre,
                Precio = manillaTipo.Precio,
                Stock = manillaTipo.Stock,
                BolicheId = manillaTipo.BolicheId
            });
        }

        [HttpPost("boliches/{bolicheId}/mesas")]
        public async Task<ActionResult<DetalleMesaDto>> CrearMesa(int bolicheId, CrearMesaDto mesaDto)
        {
            if (!await _context.Boliches.AnyAsync(b => b.Id == bolicheId))
                return NotFound("Boliche no encontrado.");

            var mesa = new Mesa
            {
                NombreONumero = mesaDto.NombreONumero,
                Ubicacion = mesaDto.Ubicacion,
                PrecioReserva = mesaDto.PrecioReserva,
                EstaDisponible = true,
                BolicheId = bolicheId
            };
            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            return Ok(new DetalleMesaDto
            {
                Id = mesa.Id,
                NombreONumero = mesa.NombreONumero,
                Ubicacion = mesa.Ubicacion,
                PrecioReserva = mesa.PrecioReserva,
                EstaDisponible = mesa.EstaDisponible,
                BolicheId = mesa.BolicheId
            });
        }
        
        [HttpPost("crear-staff")]
        public async Task<ActionResult<UsuarioDto>> CreateStaff(CrearStaffDto staffDto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == staffDto.Email.ToLower()))
                return BadRequest("El email ya está en uso.");

            using var hmac = new HMACSHA512();
            var usuario = new Usuario
            {
                Nombre = staffDto.Nombre,
                Email = staffDto.Email.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(staffDto.Password)),
                PasswordSalt = hmac.Key,
                Rol = "Staff"
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Token = "" // No se genera token aquí
            });
        }
    }
}