namespace ERP_API.Models
{
    public class Materia
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cor { get; set; } = "#CCCCCC";
        public int CategoriaId { get; set; } 
        public DateTime? CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}
