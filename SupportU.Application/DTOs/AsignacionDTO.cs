using SupportU.Infraestructure.Models;

namespace SupportU.Application.DTOs
{
    public record AsignacionDTO
    {
        public int AsignacionId { get; set; }
        public int TicketId { get; set; }
        public int TecnicoId { get; set; }
        public string MetodoAsignacion { get; set; } = null!;
        public DateTime FechaAsignacion { get; set; }
        public int UsuarioAsignadorId { get; set; }
		public string? NombreTecnico { get; set; }
		public string? TituloTicket { get; set; }
		// Navegación
		public virtual Tecnico Tecnico { get; set; } = null!;
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual Usuario UsuarioAsignador { get; set; } = null!;

        // ========== PROPIEDADES CALCULADAS ==========

        public int TiempoRestanteSLAMinutos
        {
            get
            {
                if (Ticket?.Categoria?.Sla == null || Ticket.Estado == "Cerrado")
                    return 0;

                var fechaLimite = Ticket.FechaCreacion.AddMinutes(Ticket.Categoria.Sla.TiempoResolucionMinutos);
                var tiempoRestante = (fechaLimite - DateTime.Now).TotalMinutes;

                // Si ya venció, retornar 0
                return (int)Math.Max(0, tiempoRestante);
            }
        }

        public string TiempoRestanteTexto
        {
            get
            {
                var minutos = TiempoRestanteSLAMinutos;

                if (minutos == 0)
                    return "Vencido";

                if (minutos < 60)
                    return $"{minutos} min";

                if (minutos < 1440)
                    return $"{minutos / 60}h {minutos % 60}min";

                var dias = minutos / 1440;
                var horas = (minutos % 1440) / 60;
                return $"{dias}d {horas}h";
            }
        }


        public int PorcentajeSLAUsado
        {
            get
            {
                if (Ticket?.Categoria?.Sla == null) return 0;

                var tiempoTotal = Ticket.Categoria.Sla.TiempoResolucionMinutos;
                var transcurrido = (DateTime.Now - Ticket.FechaCreacion).TotalMinutes;

                // Calcula el porcentaje, para la barra de tiempo de SLA
                var porcentaje = (transcurrido / tiempoTotal) * 100;

                // Limita entre 0 y 100 para la barra visual
                return (int)Math.Min(100, Math.Max(0, porcentaje));
            }
        }

        
        public int DiaSemana { get { int dia = (int)FechaAsignacion.DayOfWeek;  return dia == 0 ? 7 : dia; }}

        public string NombreDia => FechaAsignacion.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
    }
}