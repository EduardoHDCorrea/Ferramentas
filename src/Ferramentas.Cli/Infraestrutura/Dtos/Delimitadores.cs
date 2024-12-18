namespace Ferramentas.Cli.Infraestrutura.Dtos;

/// <summary>
/// Classe exclusiva para armazenar o identificador das variáveis utilizadas para extrair
/// dos comandos e retornos nos testes de aceitação.
/// </summary>
/// <example>
/// <code lang="json">
/// {
///     "VariávelRecebidaQueDeveSerArmazenada": "[[VariávelImportante]]",
///     "VariávelArmazenadaQueDeveSerUtilizadaAqui": "{{VariávelImportanteJáArmazenada}}",
///     "VariávelÚtilParaDadosDinâmicosQueNãoImportamParaOTeste": "[[***]]""
/// }
/// </code>
/// </example>
public static class Delimitadores
{
    public const string VariávelParaArmazenarSemÁspasInício = "[[[";
    public const string VariávelParaArmazenarSemÁspasFinal = "]]]";
    public const string VariávelParaArmazenarIgnorada = "***";
    public const string VariávelParaObterSemÁspasInício = "{{{";
    public const string VariávelParaObterSemÁspasFinal = "}}}";
    public const string VariávelParaObterInício = "{{";
    public const string VariávelParaObterFinal = "}}";
    public const string VariávelParaArmazenarInicio = "[[";
    public const string VariávelParaArmazenarFinal = "]]";
}