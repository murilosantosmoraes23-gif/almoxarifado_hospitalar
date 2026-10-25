using AlmoxarifadoHospitalar.Data;
using Microsoft.EntityFrameworkCore;
using AlmoxarifadoHospitalar.Dtos;
using AlmoxarifadoHospitalar.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await DadosIniciais.CadastrarSetoresAsync(db);
}

app.MapGet("/setores", async (AppDbContext db) =>
{
    var setores = await db.Setores
        .AsNoTracking()
        .Where(s => s.Ativo)
        .OrderBy(s => s.Nome)
        .ToListAsync();

    return Results.Ok(setores);
});

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

app.MapPost("/usuarios", async (CadastroUsuarioDto dados, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dados.Nome) ||
        string.IsNullOrWhiteSpace(dados.Email) ||
        string.IsNullOrWhiteSpace(dados.Senha))
    {
        return Results.BadRequest("Nome, e-mail e senha são obrigatórios.");
    }

    if (dados.Senha.Length < 8)
    {
        return Results.BadRequest("A senha deve ter pelo menos 8 caracteres.");
    }

    string email = dados.Email.Trim().ToLowerInvariant();

    bool emailExiste = await db.Usuarios.AnyAsync(u => u.Email == email);

    if (emailExiste)
    {
        return Results.Conflict("Este e-mail já está cadastrado.");
    }

    bool setorExiste = await db.Setores
        .AnyAsync(s => s.Id == dados.SetorId && s.Ativo);

    if (!setorExiste)
    {
        return Results.BadRequest("Selecione um setor ativo existente.");
    }

    var usuario = new Usuario
    {
        Nome = dados.Nome.Trim(),
        Email = email,
        SetorId = dados.SetorId,
        Perfil = PerfilUsuario.Colaborador
    };

    var geradorHash = new PasswordHasher<Usuario>();
    usuario.SenhaHash = geradorHash.HashPassword(usuario, dados.Senha);

    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    return Results.Json(new
    {
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.SetorId,
        Perfil = usuario.Perfil.ToString()
    }, statusCode: StatusCodes.Status201Created);
});

app.Run();
