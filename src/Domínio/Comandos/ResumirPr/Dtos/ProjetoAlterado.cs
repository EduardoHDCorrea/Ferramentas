namespace Ferramentas.Domínio.Comandos.ResumirPr.Dtos;

public class ProjetoAlterado
{
    public string NomeDoProjeto { get; set; } = string.Empty;
    public TipoDoProjeto TipoDoProjeto { get; set; }
    public int QuantidadeDeAlterações { get; set; } = 1;
}