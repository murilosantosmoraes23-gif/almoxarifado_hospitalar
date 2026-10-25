namespace AlmoxarifadoHospitalar.Dtos;

public class AtenderRequisicaoDto
{
    public List<ItemAtendimentoDto> Itens { get; set; } = new();
}

public class ItemAtendimentoDto
{
    public int MaterialId { get; set; }
    public int QuantidadeAtendida { get; set; }
}