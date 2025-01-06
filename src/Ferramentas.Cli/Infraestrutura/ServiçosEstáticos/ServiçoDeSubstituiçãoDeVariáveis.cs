using Ferramentas.Cli.Infraestrutura.Dtos;

namespace Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;

public static class ServiçoDeSubstituiçãoDeVariáveis
{
    public static string SubstituirVariáveisNoTexto(this string texto) =>
        SubstituirTexto(
                texto,
                Delimitadores.VariávelParaObterSemÁspasInício,
                Delimitadores.VariávelParaObterSemÁspasFinal,
                true
            )
            .SubstituirTexto(
                Delimitadores.VariávelParaObterInício,
                Delimitadores.VariávelParaObterFinal,
                false
            );

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

            if (!Variáveis.VariáveisDefinidas.TryGetValue(variavel, out var valor))
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
            Delimitadores.VariávelParaArmazenarSemÁspasInício,
            Delimitadores.VariávelParaArmazenarSemÁspasFinal,
            out var parcialTexto,
            out var parcialValores
        );

        TratarAtribuiçõesParaRemoção(
            parcialTexto,
            parcialValores,
            Delimitadores.VariávelParaArmazenarInicio,
            Delimitadores.VariávelParaArmazenarFinal,
            out textoAtualizado,
            out valoresExtraidos
        );
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
            if (variável == Delimitadores.VariávelParaArmazenarIgnorada)
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

            Variáveis.DefinirVariável(variável, valor);
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