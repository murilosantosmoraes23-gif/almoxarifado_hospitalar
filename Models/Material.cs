namespace AlmoxarifadoHospitalar.Models;

public class Material
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public  int QuantidadeEstoque { get; set; }

    public bool Ativo { get; set; } = true;
}