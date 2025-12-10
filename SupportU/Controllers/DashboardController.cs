using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportU.Application.Services;
using SupportU.Application.Services.Interfaces;

namespace SupportU.Web.Controllers
{
	[Authorize(Roles = "Administrador")]
	public class DashboardController : Controller
	{
		private readonly IServiceTicket _serviceTicket;
		private readonly IServiceValoracion _serviceValoracion;
		private readonly IServiceTecnico _serviceTecnico;
		private readonly IServiceCategoria _serviceCategoria;

		public DashboardController(
			IServiceTicket serviceTicket,
			IServiceValoracion serviceValoracion,
			IServiceTecnico serviceTecnico,
			IServiceCategoria serviceCategoria)
		{
			_serviceTicket = serviceTicket;
			_serviceValoracion = serviceValoracion;
			_serviceTecnico = serviceTecnico;
			_serviceCategoria = serviceCategoria;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> GetTicketsPorEstado()
		{
			var tickets = await _serviceTicket.ListAsync();

			var data = tickets
				.GroupBy(t => t.Estado)
				.ToDictionary(g => g.Key, g => g.Count());

			return Json(data);
		}

		[HttpGet]
		public async Task<IActionResult> GetTicketsPorMes()
		{
			var tickets = await _serviceTicket.ListAsync();

			var data = tickets
				.GroupBy(t => new {
					Mes = t.FechaCreacion.Month,
					Anio = t.FechaCreacion.Year
				})
				.Select(g => new {
					mes = GetNombreMes(g.Key.Mes) + " " + g.Key.Anio,
					cantidad = g.Count()
				})
				.OrderBy(x => x.mes)
				.ToList();

			return Json(data);
		}

		[HttpGet]
		public async Task<IActionResult> GetPromedioValoraciones()
		{
			var valoraciones = await _serviceValoracion.ListAsync();

			var promedio = valoraciones.Any()
				? valoraciones.Average(v => v.Puntaje)
				: 0;

			return Json(new { promedio = Math.Round(promedio, 2) });
		}

		[HttpGet]
		public async Task<IActionResult> GetCumplimientoSLA()
		{
			var tickets = await _serviceTicket.ListAsync();

			var ticketsCerrados = tickets.Where(t => t.FechaCierre != null).ToList();
			var total = ticketsCerrados.Count;
			var cumplidos = ticketsCerrados.Count(t => t.CumplimientoResolucion == true);

			var porcentaje = total > 0 ? (cumplidos * 100.0 / total) : 0;

			return Json(new
			{
				total,
				cumplidos,
				porcentaje = Math.Round(porcentaje, 2)
			});
		}

		[HttpGet]
		public async Task<IActionResult> GetRankingTecnicos()
		{
			var tecnicos = await _serviceTecnico.ListAsync();
			var tickets = await _serviceTicket.ListAsync();

			var ranking = tecnicos
				.Select(t => new {
					nombre = t.NombreUsuario,
					calificacion = t.CalificacionPromedio,
					ticketsResueltos = tickets.Count(tk =>
						tk.TecnicoAsignadoId == t.TecnicoId &&
						tk.Estado == "Cerrado")
				})
				.OrderByDescending(t => t.calificacion)
				.Take(5)
				.ToList();

			return Json(ranking);
		}

		[HttpGet]
		public async Task<IActionResult> GetCategoriasIncumplimientos()
		{
			var tickets = await _serviceTicket.ListAsync();
			var categorias = await _serviceCategoria.ListAsync();

			var data = tickets
				.Where(t => t.CumplimientoResolucion == false)
				.GroupBy(t => t.CategoriaId)
				.Select(g => new {
					categoria = categorias.FirstOrDefault(c => c.CategoriaId == g.Key)?.Nombre ?? "Sin categoría",
					incumplimientos = g.Count()
				})
				.OrderByDescending(x => x.incumplimientos)
				.Take(5)
				.ToList();

			return Json(data);
		}

		// Método auxiliar para nombres de meses
		private string GetNombreMes(int mes)
		{
			var meses = new[] {
				"", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
				"Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
			};
			return meses[mes];
		}
	}
}