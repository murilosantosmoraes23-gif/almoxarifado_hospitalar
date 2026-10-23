using AlmoxarifadoHospitalar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlmoxarifadoHospitalar.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Setor> Setores { get; set; }

    public DbSet<Material> Materiais { get; set; }

    public DbSet<Requisicao> Requisicoes { get; set; }

    public DbSet<ItemRequisicao> ItensRequisicao { get; set; }   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Setor)
            .WithMany()
            .HasForeignKey(u => u.SetorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicao>()
            .HasOne(r => r.Solicitante)
            .WithMany()
            .HasForeignKey(r => r.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicao>()
            .HasOne(r => r.Setor)
            .WithMany()
            .HasForeignKey(r => r.SetorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicao>()
            .HasOne(r => r.GestorResponsavel)
            .WithMany()
            .HasForeignKey(r => r.GestorResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemRequisicao>()
            .HasOne(i => i.Requisicao)
            .WithMany()
            .HasForeignKey(i => i.RequisicaoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemRequisicao>()
            .HasOne(i => i.Material)
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemRequisicao>()
            .HasIndex(i => new { i.RequisicaoId, i.MaterialId })
            .IsUnique();

    }
}