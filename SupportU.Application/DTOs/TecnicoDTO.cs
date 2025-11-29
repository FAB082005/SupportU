namespace SupportU.Application.DTOs
{
    public class TecnicoDTO
    {
        public int TecnicoId { get; set; }
        public int UsuarioId { get; set; }
        public int CargaTrabajo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal CalificacionPromedio { get; set; }

        public string? NombreUsuario { get; set; }
        public string? EmailUsuario { get; set; }
    }
}
