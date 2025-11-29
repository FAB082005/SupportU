namespace SupportU.Application.DTOs
{
    public class CategoriaDTO
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int SlaId { get; set; }
        public string CriterioAsignacion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string? SlaNombre { get; set; }
    }
}
//////////////////////////////////