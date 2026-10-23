namespace AlmoxarifadoHospitalar.Models;

public enum StatusRequisicao
{
    Pendente = 0,
    Atendida = 1,
    AtendidaParcialmente = 2,
    NaoAtendida = 3,
    Recusada = 4,
    Cancelada = 5
}

public class Requisicao
{
    public int Id { get; set; }

    public int SolicitanteId { get; set; }

    public Usuario Solicitante { get; set; } = null!;

    public int SetorId { get; set; }

    public Setor Setor { get; set; } = null!;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public string? Observacao { get; set; }

    public StatusRequisicao Status { get; set; } = StatusRequisicao.Pendente;

    public int? GestorResponsavelId { get; set; }

    public Usuario? GestorResponsavel { get; set; }

    public DateTime? EncerradaEm { get; set; }

    public string? MotivoEncerramento { get; set; }
}