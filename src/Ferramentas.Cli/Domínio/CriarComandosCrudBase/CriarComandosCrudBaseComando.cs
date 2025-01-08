using System.ComponentModel;
using Ferramentas.Cli.Infraestrutura;
using Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ferramentas.Cli.Domínio.CriarComandosCrudBase;

public class CriarComandosCrudBaseComando : Command<CriarComandosCrudBaseComando.Parâmetros>
{
    private const string PrefixoBase = "AdicionarAtualizar";

    private static readonly string[] Prefixos =
    [
        "Adicionar",
        "Atualizar",
        "Remover"
    ];

    private static readonly string CaminhoDosComandosTemplate = Path.Combine(
        Diretórios.DiretórioDeTemplates,
        "Comandos"
    );

    private static readonly string CaminhoDaTemplateBase = Path.Combine(
        CaminhoDosComandosTemplate,
        "ComandoBaseTemplate.txt"
    );

    private static readonly string CaminhoDaTemplateSemId = Path.Combine(
        CaminhoDosComandosTemplate,
        "ComandoNãoBaseTemplate.txt"
    );

    private static readonly string CaminhoDaTemplateComId = Path.Combine(
        CaminhoDosComandosTemplate,
        "ComandoNãoBaseComIdTemplate.txt"
    );

    private static string ObterRetornoDoPrefixo(string prefixo) =>
        prefixo switch
        {
            "Adicionar" => "IId",
            "Atualizar" => "IAtualizarComandoRetorno",
            "Remover"   => "IRemoverComandoRetorno",
            _           => string.Empty
        };

    public override int Execute(CommandContext context, Parâmetros parâmetros)
    {
        var diretórioDeExecução = new DirectoryInfo(parâmetros.Diretório);
        var nomeDaEntidade = parâmetros.NomeDaEntidade;
        AnsiConsole.MarkupLine($"Criando comandos CRUD para a entidade [bold]{nomeDaEntidade}[/]");

        var projetoNoDiretório = diretórioDeExecução
            .EnumerateFiles("*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (projetoNoDiretório is null)
        {
            AnsiConsole.MarkupLine("[red]Erro:[/] Nenhum arquivo .csproj encontrado no diretório atual.");
            return 1;
        }

        var arquivoDaEntidade = new FileInfo(
            Path.Combine(diretórioDeExecução.FullName, "Entidades", $"{nomeDaEntidade}.cs")
        );

        if (!arquivoDaEntidade.Exists)
        {
            AnsiConsole.MarkupLine("[red]Erro:[/] Arquivo da entidade não encontrado.");
            return 1;
        }

        var namespaceDoProjeto = projetoNoDiretório.Name
            .Replace(".csproj", string.Empty);

        var diretórioDeComandosManipuladores = Directory.CreateDirectory(
            Path.Combine(diretórioDeExecução.FullName, "ComandosManipuladores")
        );

        var diretórioDeComandos = Directory.CreateDirectory(
            Path.Combine(diretórioDeComandosManipuladores.FullName, "Comandos")
        );

        var usingsDaEntidade = File
            .ReadAllLines(arquivoDaEntidade.FullName)
            .Where(l => l.Contains("using"))
            .Select(l => l.Split(" ")[1])
            .Select(u => $"using {u}")
            .ToArray();

        var conteúdoDaEntidade = File.ReadAllLines(arquivoDaEntidade.FullName);
        var propriedadesDaEntidade = conteúdoDaEntidade
            .Where(l => l.Contains("public") && l.Contains("get; set;"))
            .ToArray();

        var namespaceDosComandos = $"{namespaceDoProjeto}.ComandosManipuladores.Comandos;";
        var namespaceDaEntidade = $"{namespaceDoProjeto}.Entidades;";

        Variáveis.DefinirVariável("NamespaceDoProjeto", namespaceDosComandos);
        Variáveis.DefinirVariável("NamespaceDaEntidade", namespaceDaEntidade);
        Variáveis.DefinirVariável("UsingsDaEntidade", string.Join(Environment.NewLine, usingsDaEntidade));
        Variáveis.DefinirVariável("NomeDaEntidade", nomeDaEntidade);
        Variáveis.DefinirVariável("Propriedades", string.Join(Environment.NewLine, propriedadesDaEntidade));

        CriarComandoDaEntidade(
            nomeDaEntidade,
            PrefixoBase,
            diretórioDeComandos.FullName
        );
        foreach (var prefixo in Prefixos)
        {
            Variáveis.DefinirVariável("RetornoDoComando", ObterRetornoDoPrefixo(prefixo));

            CriarComandoDaEntidade(
                nomeDaEntidade,
                prefixo,
                diretórioDeComandos.FullName
            );
        }

        return 0;
    }

    private static void CriarComandoDaEntidade(
        string nomeDaEntidade,
        string prefixo,
        string diretórioDeComandos
    )
    {
        var ehBase = prefixo is PrefixoBase;
        var template = prefixo switch
        {
            PrefixoBase     => File.ReadAllText(CaminhoDaTemplateBase),
            not "Adicionar" => File.ReadAllText(CaminhoDaTemplateComId),
            _               => File.ReadAllText(CaminhoDaTemplateSemId)
        };

        Variáveis.DefinirVariável("Prefixo", prefixo);
        Variáveis.DefinirVariável("Sufixo", ObterSufixoDoComando(ehBase));

        var nomeDoComando = $"{prefixo}{nomeDaEntidade}{ObterSufixoDoComando(ehBase)}";
        if (ehBase)
            Variáveis.DefinirVariável("ComandoBase", nomeDoComando);

        var caminhoDoComando = Path.Combine(
            diretórioDeComandos,
            $"{nomeDoComando}.cs"
        );

        File.WriteAllText(caminhoDoComando, template.SubstituirVariáveisNoTexto());
    }

    private static string ObterSufixoDoComando(bool ehBase = false) =>
        ehBase ? "ComandoBase" : "Comando";

    public class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<NOME_DA_ENTIDADE>")]
        [Description("Nome da entidade para a qual os comandos CRUD serão criados.")]
        public required string NomeDaEntidade { get; init; }

        [CommandOption("-d|--diretorio")]
        [Description("Diretório do projeto.")]
        public string Diretório { get; init; } = Diretórios.DiretórioDeExecução;
    }
}