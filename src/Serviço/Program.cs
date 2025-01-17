using System.Text.Json;
using Ferramentas.Domínio;
using Ferramentas.Domínio.Comandos.CriarComandosCrudBase;
using Ferramentas.Domínio.Comandos.CriarOrganizaçãoTeste;
using Ferramentas.Domínio.Comandos.ExecutarTestesDaSolução;
using Ferramentas.Domínio.Comandos.ObterCaminhoRelativo;
using Ferramentas.Domínio.Comandos.ResumirPr;
using Ferramentas.Domínio.Dtos;
using Ferramentas.Infraestrutura.ManipulaçãoDeTexto;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(
    configurator =>
    {
        ObterCaminhoRelativoComando.InjetarComando(configurator);
        CriarOrganizaçãoTesteComando.InjetarComando(configurator);
        CriarComandosCrudBaseComando.InjetarComando(configurator);
        ResumirPrComando.InjetarComando(configurator);
        ExecutarTestesDaSoluçãoComando.InjetarComando(configurator);
    }
);

args =
[
    "executar-testes",
    @"C:\Sky\TerraMedia\infra"
];

InicializarVariáveisGlobais();
app.Run(args);
return;

static void InicializarVariáveisGlobais()
{
    try
    {
        var arquivoJson = new FileInfo(
            Path.Combine(Diretórios.DiretórioDeConfiguração.FullName, $"{nameof(VariáveisGlobais)}.json")
        );
        if (!arquivoJson.Exists)
            return;

        var variáveisGlobais = JsonSerializer.Deserialize<VariáveisGlobais>(File.ReadAllText(arquivoJson.FullName));
        if (variáveisGlobais is null)
            return;

        foreach (var variável in variáveisGlobais.Itens)
            Variáveis.DefinirVariável(variável.Nome, variável.Valor);
    }
    catch (Exception e)
    {
        AnsiConsole.MarkupLine("[red]Erro ao inicializar variáveis globais:[/]");
        AnsiConsole.WriteException(e);
    }
}