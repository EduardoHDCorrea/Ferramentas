namespace Ferramentas.Domínio.Dtos;

public class VariáveisGlobais
{
    public class VariávelGlobal
    {
        public string Nome { get; set; }
        public string Valor { get; set; }
    }

    public List<VariávelGlobal> Itens { get; set; } = [];
}