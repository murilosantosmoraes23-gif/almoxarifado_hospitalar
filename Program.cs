using AlmoxarifadoHospitalar.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

app.MapGet("/materiais", async (AppDbContext db) =>
{
    return await db.Materiais.AsNoTracking().ToListAsync();
});

app.MapPost("/materiais", async (AlmoxarifadoHospitalar.Models.Material material, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(material.Nome))
    {
        return Results.BadRequest("O nome do material é obrigatório.");
    }

    if (material.QuantidadeEstoque < 0)
    {
        return Results.BadRequest("A quantidade não pode ser negativa.");
    }

    db.Materiais.Add(material);

    await db.SaveChangesAsync();

    return Results.Created($"/materiais/{material.Id}", material);
});

app.MapPut("/materiais/{id:int}", async (int id, AlmoxarifadoHospitalar.Models.Material dados, AppDbContext db) =>
{
    var material = await db.Materiais.FindAsync(id);

    if (material == null)
    {
        return Results.NotFound("Material não encontrado.");
    }

    if (string.IsNullOrWhiteSpace(dados.Nome))
    {
        return Results.BadRequest("O nome do material é obrigatório.");
    }

    if (dados.QuantidadeEstoque < 0)
    {
        return Results.BadRequest("A quantidade não pode ser negativa.");
    }

    material.Nome = dados.Nome;
    material.UnidadeMedida = dados.UnidadeMedida;
    material.QuantidadeEstoque = dados.QuantidadeEstoque;
    material.Ativo = dados.Ativo;

    await db.SaveChangesAsync();

    return Results.Ok(material);
});

app.MapDelete("/materiais/{id:int}", async (int id, AppDbContext db) =>
{
    var material = await db.Materiais.FindAsync(id);

    if (material == null)
    {
        return Results.NotFound("Material não encontrado.");
    }

    if (!material.Ativo)
    {
        return Results.Conflict("Material já está inativo.");
    }

    material.Ativo = false;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
