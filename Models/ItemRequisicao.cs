namespace AlmoxarifadoHospitalar.Models;

public enum StatusItemRequisicao
{
    Pendente = 0,
    Atendido = 1,
    AtendidoParcialmente = 2,
    SemEstoque = 3,
    Recusado = 4,
    Cancelado = 5
}

public class ItemRequisicao
{
    public int Id { get; set; }
    
    public int RequisicaoId { get; set; }

    public Requisicao Requisicao { get; set; } = null!;

    public int MaterialId { get; set; }

    public Material Material { get; set; } = null!;

    public int QuantidadeSolicitada { get; set; }

    public int  QuantidadeAtendida { get; set; }

    public StatusItemRequisicao Status { get; set; } = 
        StatusItemRequisicao.Pendente;
}