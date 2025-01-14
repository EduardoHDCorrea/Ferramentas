namespace Ferramentas.Infraestrutura.ManipulaçãoDeTexto;

public static class Variáveis
{
    private const string DelimitadorParaArmazenarSemÁspasInício = "[[[";
    private const string DelimitadorParaArmazenarSemÁspasFinal = "]]]";
    private const string DelimitadorParaArmazenarIgnorada = "***";
    private const string DelimitadorParaObterSemÁspasInício = "{{{";
    private const string DelimitadorParaObterSemÁspasFinal = "}}}";
    private const string DelimitadorParaObterInício = "{{";
    private const string DelimitadorParaObterFinal = "}}";
    private const string DelimitadorParaArmazenarInicio = "[[";
    private const string DelimitadorParaArmazenarFinal = "]]";
    public static readonly Dictionary<string, string> VariáveisDefinidas = new();

    public static void DefinirVariável(string nomeDaVariável, string valorDaVariável)
    {
        if (!VariáveisDefinidas.TryAdd(nomeDaVariável, valorDaVariável))
            VariáveisDefinidas[nomeDaVariável] = valorDaVariável;
    }

    public static string ObterVariável(string nomeDaVariável) =>
        VariáveisDefinidas[nomeDaVariável];

    public static string SubstituirVariáveisNoTexto(this string texto) =>
        SubstituirTexto(
                texto,
                DelimitadorParaObterSemÁspasInício,
                DelimitadorParaObterSemÁspasFinal,
                true
            )
            .SubstituirTexto(
                DelimitadorParaObterInício,
                DelimitadorParaObterFinal,
                false
            );

    public static void RemoverAtribuiçõesDeVariáveis(
        string textoComVariavel,
        string textoOriginal,
        out string textoAtualizado,
        out string valoresExtraidos
    )
    {
        TratarAtribuiçõesParaRemoção(
            textoComVariavel,
            textoOriginal,
            DelimitadorParaArmazenarSemÁspasInício,
            DelimitadorParaArmazenarSemÁspasFinal,
            out var parcialTexto,
            out var parcialValores
        );

        TratarAtribuiçõesParaRemoção(
            parcialTexto,
            parcialValores,
            DelimitadorParaArmazenarInicio,
            DelimitadorParaArmazenarFinal,
            out textoAtualizado,
            out valoresExtraidos
        );
    }

    private static string SubstituirTexto(
        this string texto,
        string delimitadorInicio,
        string delimitadorFim,
        bool removerAspas
    )
    {
        var resultado = texto;

        while (true)
        {
            var variavel = resultado.ExtrairTextoEntreDelimitadores(delimitadorInicio, delimitadorFim);
            if (string.IsNullOrEmpty(variavel))
                break;

            if (!VariáveisDefinidas.TryGetValue(variavel, out var valor))
                throw new InvalidOperationException($"Variável '{variavel}' não definida.");

            var inicio = resultado.IndexOf(delimitadorInicio, StringComparison.InvariantCulture);
            var fim = resultado.IndexOf(delimitadorFim, inicio, StringComparison.InvariantCulture);
            fim = fim == -1 ? resultado.Length : fim + delimitadorFim.Length;

            if (removerAspas)
            {
                inicio--;
                fim++;
            }

            resultado = resultado.Replace(resultado[inicio..fim], valor, StringComparison.InvariantCulture);
        }

        return resultado;
    }

    private static void TratarAtribuiçõesParaRemoção(
        string textoVariável,
        string textoValor,
        string delimitadorInicio,
        string delimitadorFim,
        out string textoAtualizado,
        out string valoresAtualizados
    )
    {
        textoAtualizado = textoVariável;
        valoresAtualizados = textoValor;

        while (true)
        {
            var variável = textoAtualizado.ExtrairTextoEntreDelimitadores(delimitadorInicio, delimitadorFim);
            if (string.IsNullOrEmpty(variável))
                break;

            var inicio = textoAtualizado.IndexOf(delimitadorInicio, StringComparison.InvariantCulture);
            if (inicio == -1)
                throw new InvalidOperationException($"Delimitador '{delimitadorInicio}' não encontrado.");

            if (inicio >= valoresAtualizados.Length)
                throw new InvalidOperationException(
                    $"Índice calculado ({inicio}) excede o tamanho de 'valoresAtualizados' ({valoresAtualizados.Length})."
                );

            var fim = valoresAtualizados.IndexOf('"', inicio);
            fim = fim == -1 ? valoresAtualizados.IndexOf(',', inicio) : fim;
            fim = fim == -1 ? valoresAtualizados.Length : fim;

            var valor = valoresAtualizados[inicio..fim];
            if (variável == DelimitadorParaArmazenarIgnorada)
            {
                textoAtualizado = textoAtualizado.SubstituirPrimeiraOcorrência(
                    string.Concat(delimitadorInicio, variável, delimitadorFim),
                    valor
                );
                continue;
            }

            textoAtualizado = textoAtualizado.Replace(
                string.Concat(delimitadorInicio, variável, delimitadorFim),
                valor,
                StringComparison.InvariantCulture
            );

            DefinirVariável(variável, valor);
        }
    }

    private static string ExtrairTextoEntreDelimitadores(this string texto, string inicio, string fim)
    {
        var posiçãoInicial = texto.IndexOf(inicio, StringComparison.InvariantCulture);
        if (posiçãoInicial == -1) return string.Empty;

        posiçãoInicial += inicio.Length;
        var posiçãoFinal = texto.IndexOf(fim, posiçãoInicial, StringComparison.InvariantCulture);
        return posiçãoFinal == -1
            ? string.Empty
            : texto[posiçãoInicial..posiçãoFinal];
    }

    private static string SubstituirPrimeiraOcorrência(this string texto, string busca, string substituição)
    {
        var pos = texto.IndexOf(busca, StringComparison.InvariantCulture);
        return pos == -1 ? texto : texto[..pos] + substituição + texto[(pos + busca.Length)..];
    }
}