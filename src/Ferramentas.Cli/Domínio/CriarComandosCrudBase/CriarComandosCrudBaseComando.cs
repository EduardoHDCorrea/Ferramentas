using System.ComponentModel;
using Ferramentas.Cli.Infraestrutura;
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

    public override int Execute(CommandContext context, Parâmetros parâmetros)
    {
        var diretórioDeExecução = new DirectoryInfo(Diretórios.DiretórioDeExecução);
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

        var propriedadesDaEntidade = File
            .ReadAllLines(arquivoDaEntidade.FullName)
            .Where(l => l.Contains("public"))
            .Select(l => l.Split(" ")[2])
            .ToArray();

        var namespaceDosComandos = $"{namespaceDoProjeto}.ComandosManipuladores.Comandos;";

        CriarComandoDaEntidade(
            nomeDaEntidade,
            namespaceDosComandos,
            usingsDaEntidade,
            propriedadesDaEntidade,
            PrefixoBase,
            diretórioDeComandos.FullName
        );
        foreach (var prefixo in Prefixos)
        {
            CriarComandoDaEntidade(
                nomeDaEntidade,
                namespaceDosComandos,
                usingsDaEntidade,
                propriedadesDaEntidade,
                prefixo,
                diretórioDeComandos.FullName
            );
        }

        return 0;
    }

    // TODO: Corrigir geração dos comandos
    private static void CriarComandoDaEntidade(
        string nomeDaEntidade,
        string namespaceDosComandos,
        string[] usingsDaEntidade,
        string[] propriedadesDaEntidade,
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

        var comando = template
            .Replace("{{Namespace}}", namespaceDosComandos)
            .Replace("{{Usings}}", string.Join(Environment.NewLine, usingsDaEntidade))
            .Replace("{{NomeDaEntidade}}", nomeDaEntidade)
            .Replace("{{Propriedades}}", string.Join(Environment.NewLine, propriedadesDaEntidade));

        var caminhoDoComando = Path.Combine(
            diretórioDeComandos,
            $"{prefixo}{nomeDaEntidade}{ObterSufixoDoComando(ehBase)}.cs"
        );

        File.WriteAllText(caminhoDoComando, comando);
    }

    private static string ObterSufixoDoComando(bool ehBase = false) =>
        ehBase ? "ComandoBase" : "Comando";

    public class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<NOME_DA_ENTIDADE>")]
        [Description("Nome da entidade para a qual os comandos CRUD serão criados.")]
        public required string NomeDaEntidade { get; init; }
    }
}