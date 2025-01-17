namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Dtos;

public class ResultadoDosTestes
{
    public bool Sucesso { get; set; }
    public ContadoresDoResultadoDosTestes? Contadores { get; set; }

    public string Mensagem { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;

    public string Rota { get; set; } = string.Empty;
    public string Retorno { get; set; } = string.Empty;
}