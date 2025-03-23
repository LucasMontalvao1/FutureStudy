using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ERP_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FrequenciaMeta
    {
        [Description("Diária")]
        Diaria = 0,

        [Description("Semanal")]
        Semanal = 1,

        [Description("Mensal")]
        Mensal = 2
    }
}