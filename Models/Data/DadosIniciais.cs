using AlmoxarifadoHospitalar.Models;
using Microsoft.EntityFrameworkCore;

namespace AlmoxarifadoHospitalar.Data;

public static class DadosIniciais
{
    public static async Task CadastrarSetoresAsync(AppDbContext db)
    {
        string[] nomes =
        {
            "UTI Azul",
            "UTI Verde",
            "UTI Laranja",
            "UTI Lilás",
            "Ambulatório",
            "Farmácia Central",
            "PS - Pronto Socorro",
            "CC - Centro Cirúrgico"
        };

        foreach (string nome in nomes)
        {
            bool existe = await db.Setores.AnyAsync(s => s.Nome == nome);

            if (!existe)
            {
                db.Setores.Add(new Setor
                {
                    Nome = nome,
                    Ativo = true
                });
            }
        }

        await db.SaveChangesAsync();
    }
}