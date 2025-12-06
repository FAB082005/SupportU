using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
	public interface IServiceAutoTriage
	{
		Task<AutoTriageDTO> AsignarTicketAutomaticoAsync(int ticketId);
		Task AsignarTicketPendienteAsync(int ticketId);
		Task<List<AutoTriageDTO>> AsignarTicketsPendientesAsync();
	}
}
