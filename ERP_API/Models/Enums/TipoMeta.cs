using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoMeta
    {
        [Description("Tempo de estudo")]
        Tempo = 0,

        [Description("Quantidade de sessões")]
        QtdSessoes = 1,

        [Description("Tópicos concluídos")]
        Topicos = 2
    }
}