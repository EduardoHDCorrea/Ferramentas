namespace Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;

/// <summary>
/// Serviço estático para manipulação de arquivos locais.
/// </summary>
public static class ServiçoDeManipulaçãoDeArquivosLocais
{
    /// <summary>
    /// Procura e extrai o conteúdo de um arquivo <b>".json"</b>
    /// </summary>
    /// <param name="nomeDoArquivo">Nome do arquivo e sua extensão, não o caminho completo.</param>
    /// <returns>Conteúdo do arquivo em string</returns>
    /// <example><c>var conteúdoDoArquivo = "UmArquivoJson.json".ObterTextoDoArquivo()</c></example>
    /// <exception cref="FileNotFoundException">Caso o arquivo não seja encontrado.</exception>
    public static string ObterTextoDoArquivo(this string nomeDoArquivo)
    {
        var caminhoDoArquivo = Directory
            .EnumerateFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                nomeDoArquivo,
                SearchOption.AllDirectories
            ).ToHashSet();

        if (caminhoDoArquivo.Count == 0)
            throw new FileNotFoundException($"Não foi possível encontrar o arquivo '{nomeDoArquivo}'");

        return File.ReadAllText(caminhoDoArquivo.First());
    }
}