using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UnidadeMeta
    {
        [Description("Minutos")]
        Minutos = 0,

        [Description("Horas")]
        Horas = 1,

        [Description("Tópicos")]
        Topicos = 2,

        [Description("Sessões")]
        Sessoes = 3,

        [Description("Categorias")]
        Categorias = 4
    }
}