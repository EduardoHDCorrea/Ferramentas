namespace Ferramentas.Infraestrutura.Sistema;

/// <summary>
/// Serviço estático para manipulação de arquivos locais.
/// </summary>
public static class ManipuladorDeArquivos
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
            )
            .ToHashSet();

        if (caminhoDoArquivo.Count == 0)
            throw new FileNotFoundException($"Não foi possível encontrar o arquivo '{nomeDoArquivo}'");

        return File.ReadAllText(caminhoDoArquivo.First());
    }

    public static List<FileInfo> ObterProjetosDeTesteDoDiretório(this DirectoryInfo diretório)
    {
        var listaDeProjetos = new List<string>();
        using var processo = ExtensõesDeProcessos.ObterProcessoParaListaDeProjetosDeTestesDaSolução(
            listaDeProjetos, diretório.FullName
        );

        processo.Start();
        processo.WaitForExit();

        return listaDeProjetos
            .ConvertAll(projeto => new FileInfo(
                Path.GetFullPath(projeto, diretório.FullName))
            );
    }

    public static FileInfo? ObterPrimeiroArquivoComExtensão(this DirectoryInfo diretório, string extensão) =>
        diretório
            .EnumerateFiles(
                $"*.{extensão}",
                SearchOption.AllDirectories
            )
            .FirstOrDefault();
}