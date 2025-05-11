using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoMeta
    {
        [Description("Tempo Total")]
        TempoTotal = 0,

        [Description("Sessões Concluídas")]
        SessoesConcluidas = 1,

        [Description("Tópicos Estudados")]
        TopicosEstudados = 2,

        [Description("Categorias Completas")]
        CategoriasCompletas = 3
    }
}