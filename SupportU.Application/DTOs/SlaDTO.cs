namespace SupportU.Application.DTOs
{
    public class SlaDTO
    {
        public int SlaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TiempoRespuestaMinutos { get; set; }
        public int TiempoResolucionMinutos { get; set; }
        public bool Activo { get; set; }
    }
}
