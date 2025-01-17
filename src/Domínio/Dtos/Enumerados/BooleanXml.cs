using System.Text.Json.Serialization;

namespace Ferramentas.Domínio.Dtos.Enumerados;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BooleanXml
{
    False,
    True
}