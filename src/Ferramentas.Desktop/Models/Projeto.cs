using Ferramentas.Desktop.Models.Enumerados;

namespace Ferramentas.Desktop.Models;

public class Projeto
{
    public string Repositório { get; set; } = string.Empty;
    public string NomeDoProjeto { get; set; } = string.Empty;
    public TipoDoProjeto TipoDoProjeto { get; set; }
    public int QuantidadeDeAlterações { get; set; } = 1;
}