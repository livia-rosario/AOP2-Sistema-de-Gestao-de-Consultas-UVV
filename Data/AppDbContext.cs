using GestaoConsultasUVV.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoConsultasUVV.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Usuario).WithMany(u => u.Consultas)
            .HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Consulta>().HasIndex(c => new { c.UsuarioId, c.DataHora });
    }
}
