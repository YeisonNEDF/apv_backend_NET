

using APV.Entities;
using Microsoft.EntityFrameworkCore;

namespace APV
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Definimos el campo email como indice único
            modelBuilder.Entity<Veterinario>()
                .HasIndex(v => v.email)
                .IsUnique();

            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.Veterinario)
                .WithMany(v => v.Pacientes)
                .HasForeignKey(p => p.VeterinarioId);
        }

        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
    }
}
