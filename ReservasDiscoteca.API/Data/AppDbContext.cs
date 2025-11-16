using Microsoft.EntityFrameworkCore;
using ReservasDiscoteca.API.Entities;

namespace ReservasDiscoteca.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Registro de todas las tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Boliche> Boliches { get; set; }
        public DbSet<ManillaTipo> ManillaTipos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraManilla> CompraManillas { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación: Usuario (1) -> Compras (*)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Compras) 
                .WithOne(c => c.Usuario) 
                .HasForeignKey(c => c.UsuarioId) 
                .OnDelete(DeleteBehavior.Cascade); 

            // Relación: Boliche (1) -> ManillaTipos (*)
            modelBuilder.Entity<Boliche>()
                .HasMany(b => b.ManillaTipos)
                .WithOne(m => m.Boliche)
                .HasForeignKey(m => m.BolicheId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación: Boliche (1) -> Mesas (*)
            modelBuilder.Entity<Boliche>()
                .HasMany(b => b.Mesas)
                .WithOne(m => m.Boliche)
                .HasForeignKey(m => m.BolicheId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Relación: Compra (1) -> Boliche (1)
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Boliche) 
                .WithMany() // Boliche no necesita una lista de compras
                .HasForeignKey(c => c.BolicheId)
                .OnDelete(DeleteBehavior.Restrict); // No borrar boliche si tiene compras

            // Relación: Compra (1) <-> Mesa (1) (Opcional)
            modelBuilder.Entity<Mesa>()
                .HasOne(m => m.Compra) // Una Mesa tiene una Compra (o null)
                .WithOne(c => c.MesaReservada) // Una Compra tiene una Mesa (o null)
                .HasForeignKey<Mesa>(m => m.CompraId) // La clave foránea está en Mesa
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull); // Si se borra la compra, la mesa queda libre

            // Relación "Muchos a Muchos": Compra <-> ManillaTipo
            modelBuilder.Entity<Compra>()
                .HasMany(c => c.ManillasCompradas)
                .WithOne(cm => cm.Compra)
                .HasForeignKey(cm => cm.CompraId);

            modelBuilder.Entity<CompraManilla>()
                .HasOne(cm => cm.ManillaTipo)
                .WithMany() // ManillaTipo no necesita lista de CompraManillas
                .HasForeignKey(cm => cm.ManillaTipoId);
        }
    }
}