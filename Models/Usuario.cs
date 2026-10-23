namespace AlmoxarifadoHospitalar.Models;

public enum PerfilUsuario
{
    Colaborador = 0,
    Gestor = 1
}

public class Usuario 
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Colaborador;

    public int? SetorId { get; set; }

    public Setor? Setor { get; set; }
}