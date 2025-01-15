namespace Ferramentas.Infraestrutura.ManipulaçãoDeTexto;

public static class ExtensõesDeTextoComDataHora
{
    public static string ObterTextoDeTempoDecorrido(this DateTime inicio, DateTime fim)
    {
        var tempoDecorrido = fim - inicio;
        return tempoDecorrido.Hours < 1
            ? $@"{tempoDecorrido:mm\:ss}"
            : $@"{tempoDecorrido:hh\:mm\:ss}";
    }

    public static string ObterTextoDaHora(this DateTime data) => $@"{data.TimeOfDay:hh\:mm\:ss}";
}