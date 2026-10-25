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

app.MapPost("/requisicoes", async (
    CriarRequisicaoDto dados, AppDbContext db) =>
{
    if (dados.Itens is null || dados.Itens.Count == 0)
    {
        return Results.BadRequest("Informe pelo menos um material.");
    }

    if (dados.Itens.Any(i => i is null || i.Quantidade <= 0))
    {
        return Results.BadRequest("Informe itens com quantidades positivas.");
    }

    var ids = dados.Itens.Select(i => i.MaterialId).ToList();

    if (ids.Distinct().Count() != ids.Count)
    {
        return Results.BadRequest("Não repita materiais na mesma requisição.");
    }

    var solicitante = await db.Usuarios
        .Include(u => u.Setor)
        .FirstOrDefaultAsync(u => u.Id == dados.SolicitanteId);

    if (solicitante is null)
    {
        return Results.BadRequest("Solicitante não encontrado.");
    }

    if (solicitante.Perfil != PerfilUsuario.Colaborador)
    {
        return Results.BadRequest("O solicitante deve ser um colaborador.");
    }

    if (solicitante.Setor is null || !solicitante.Setor.Ativo)
    {
        return Results.BadRequest("O colaborador precisa de um setor ativo.");
    }

    int materiaisEncontrados = await db.Materiais
        .CountAsync(m => ids.Contains(m.Id) && m.Ativo);

    if (materiaisEncontrados != ids.Count)
    {
        return Results.BadRequest(
            "Um ou mais materiais não existem ou estão inativos.");
    }

    var requisicao = new Requisicao
    {
        SolicitanteId = solicitante.Id,
        SetorId = solicitante.Setor.Id,
        Observacao = dados.Observacao?.Trim(),
        Status = StatusRequisicao.Pendente
    };

    db.Requisicoes.Add(requisicao);

    foreach (var item in dados.Itens)
    {
        db.ItensRequisicao.Add(new ItemRequisicao
        {
            Requisicao = requisicao,
            MaterialId = item.MaterialId,
            QuantidadeSolicitada = item.Quantidade,
            QuantidadeAtendida = 0,
            Status = StatusItemRequisicao.Pendente
        });
    }

    await db.SaveChangesAsync();

    return Results.Json(new
    {
        requisicao.Id,
        requisicao.SolicitanteId,
        requisicao.SetorId,
        requisicao.CriadaEm,
        requisicao.Observacao,
        Status = requisicao.Status.ToString(),
        Itens = dados.Itens
    }, statusCode: StatusCodes.Status201Created);
});

app.MapGet("/requisicoes/{id:int}", async (int id, AppDbContext db) =>
{
    var requisicao = await db.Requisicoes
        .AsNoTracking()
        .Where(r => r.Id == id)
        .Select(r => new
        {
            r.Id,
            r.SolicitanteId,
            Solicitante = r.Solicitante.Nome,
            r.SetorId,
            Setor = r.Setor.Nome,
            r.CriadaEm,
            r.Observacao,
            r.Status,
            r.GestorResponsavelId,
            r.EncerradaEm
        })
        .FirstOrDefaultAsync();

    if (requisicao is null)
    {
        return Results.NotFound("Requisição não encontrada.");
    }

    var itens = await db.ItensRequisicao
        .AsNoTracking()
        .Where(i => i.RequisicaoId == id)
        .Select(i => new
        {
            i.Id,
            i.MaterialId,
            Material = i.Material.Nome,
            UnidadeMedida = i.Material.UnidadeMedida,
            i.QuantidadeSolicitada,
            i.QuantidadeAtendida,
            i.Status
        })
        .ToListAsync();

    return Results.Ok(new
    {
        requisicao.Id,
        requisicao.SolicitanteId,
        requisicao.Solicitante,
        requisicao.SetorId,
        requisicao.Setor,
        requisicao.CriadaEm,
        requisicao.Observacao,
        Status = requisicao.Status.ToString(),
        requisicao.GestorResponsavelId,
        requisicao.EncerradaEm,
        Itens = itens.Select(i => new
        {
            i.Id,
            i.MaterialId,
            i.Material,
            i.UnidadeMedida,
            i.QuantidadeSolicitada,
            i.QuantidadeAtendida,
            Status = i.Status.ToString()
        })
    });
});

app.MapGet("/requisicoes", async (AppDbContext db) =>
{
    var requisicoes = await db.Requisicoes
        .AsNoTracking()
        .OrderByDescending(r => r.CriadaEm)
        .Select(r => new
        {
            r.Id,
            Solicitante = r.Solicitante.Nome,
            Setor = r.Setor.Nome,
            r.CriadaEm,
            r.Status
        })
        .ToListAsync();

    return Results.Ok(requisicoes.Select(r => new
    {
        r.Id,
        r.Solicitante,
        r.Setor,
        r.CriadaEm,
        Status = r.Status.ToString()
    }));
});

app.MapPost("/requisicoes/{id:int}/atender", async (
    int id, AtenderRequisicaoDto dados, AppDbContext db) =>
{
    if (dados.Itens is null || dados.Itens.Count == 0)
    {
        return Results.BadRequest("Informe os itens do atendimento.");
    }

    if (dados.Itens.Any(i => i is null || i.QuantidadeAtendida < 0))
    {
        return Results.BadRequest("As quantidades não podem ser negativas.");
    }

    var ids = dados.Itens.Select(i => i.MaterialId).ToList();

    if (ids.Distinct().Count() != ids.Count)
    {
        return Results.BadRequest("Não repita materiais no atendimento.");
    }

    await using var transacao = await db.Database.BeginTransactionAsync();

    var requisicao = await db.Requisicoes.FindAsync(id);

    if (requisicao is null)
    {
        return Results.NotFound("Requisição não encontrada.");
    }

    if (requisicao.Status != StatusRequisicao.Pendente)
    {
        return Results.Conflict("Esta requisição já foi encerrada.");
    }

    var itens = await db.ItensRequisicao
        .Include(i => i.Material)
        .Where(i => i.RequisicaoId == id)
        .ToListAsync();

    if (itens.Count != dados.Itens.Count ||
        itens.Any(i => !ids.Contains(i.MaterialId)))
    {
        return Results.BadRequest(
            "Informe exatamente todos os materiais desta requisição.");
    }

    var quantidades = dados.Itens.ToDictionary(
        i => i.MaterialId,
        i => i.QuantidadeAtendida);

    foreach (var item in itens)
    {
        int quantidade = quantidades[item.MaterialId];

        if (quantidade > item.QuantidadeSolicitada)
        {
            return Results.BadRequest(
                $"Quantidade superior à solicitada: {item.Material.Nome}.");
        }

        if (quantidade > item.Material.QuantidadeEstoque)
        {
            return Results.BadRequest(
                $"Estoque insuficiente: {item.Material.Nome}.");
        }

        if (quantidade > 0 && !item.Material.Ativo)
        {
            return Results.BadRequest(
                $"Material inativo: {item.Material.Nome}.");
        }

        if (quantidade == 0 && item.Material.QuantidadeEstoque > 0)
        {
            return Results.BadRequest(
                $"O material {item.Material.Nome} possui estoque. " +
                "Informe uma quantidade para atendimento.");
        }
    }

    foreach (var item in itens)
    {
        int quantidade = quantidades[item.MaterialId];

        item.QuantidadeAtendida = quantidade;
        item.Material.QuantidadeEstoque -= quantidade;

        item.Status = quantidade == 0
            ? StatusItemRequisicao.SemEstoque
            : quantidade == item.QuantidadeSolicitada
                ? StatusItemRequisicao.Atendido
                : StatusItemRequisicao.AtendidoParcialmente;
    }

    requisicao.Status = itens.All(
        i => i.QuantidadeAtendida == i.QuantidadeSolicitada)
        ? StatusRequisicao.Atendida
        : itens.Any(i => i.QuantidadeAtendida > 0)
            ? StatusRequisicao.AtendidaParcialmente
            : StatusRequisicao.NaoAtendida;

    requisicao.EncerradaEm = DateTime.UtcNow;

    await db.SaveChangesAsync();
    await transacao.CommitAsync();

    return Results.Ok(new
    {
        requisicao.Id,
        Status = requisicao.Status.ToString(),
        requisicao.EncerradaEm
    });
});

app.Run();
