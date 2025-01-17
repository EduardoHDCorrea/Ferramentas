namespace Ferramentas.Infraestrutura.Reflexão;

public static class ExtensõesDeReflexão
{
    public static bool PossuiPropriedadeDoTipo<T>(this object objeto)
    {
        var tipo = typeof(T);
        var propriedades = objeto.GetType().GetProperties();

        return propriedades.Any(propriedade =>
            propriedade.PropertyType == tipo);
    }

    public static string? ObterNomeDaPropriedadeDoTipo<T>(this object objeto)
    {
        var tipo = typeof(T);
        var propriedades = objeto.GetType().GetProperties();

        var propriedade = propriedades
            .FirstOrDefault(propriedade =>
                propriedade.PropertyType == tipo);

        return propriedade?.Name;
    }

    public static T? ObterValorDaPropriedadeDoTipo<T>(this object objeto)
    {
        var tipo = typeof(T);
        var propriedades = objeto.GetType().GetProperties();

        var propriedade = propriedades
            .FirstOrDefault(propriedade =>
                propriedade.PropertyType == tipo);

        if (propriedade is null)
            return default;

        return (T?)propriedade.GetValue(objeto);
    }
}