namespace AlmoxarifadoHospitalar.Dtos;

public class CriarRequisicaoDto
{
    public int SolicitanteId { get; set; }
    public string? Observacao { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = new();
}

public class ItemPedidoDto
{
    public int MaterialId { get; set; }
    public int Quantidade { get; set; }
}