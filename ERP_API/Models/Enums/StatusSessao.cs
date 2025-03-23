using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusSessao
    {
        [Description("em_andamento")]
        EmAndamento = 0,

        [Description("pausada")]
        Pausada = 1,

        [Description("concluída")]
        Concluida = 2
    }
}