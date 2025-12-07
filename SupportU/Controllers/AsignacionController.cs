using Microsoft.AspNetCore.Mvc;
using SupportU.Application.Services.Interfaces;

namespace SupportU.Web.Controllers
{
    public class AsignacionController : BaseController
    {
        private readonly IServiceAsignacion _serviceAsignacion;

        public AsignacionController(IServiceAsignacion serviceAsignacion)
        {
            _serviceAsignacion = serviceAsignacion;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int offsetSemanas = 0)
        {
            ViewBag.OffsetSemanas = offsetSemanas;

          
            var hoy = DateTime.Now.Date;
            int diaSemana = (int)hoy.DayOfWeek;
            int diasDesdeInicio = (diaSemana == 0) ? 6 : diaSemana - 1;

            var inicioSemanaActual = hoy.AddDays(-diasDesdeInicio);

            // Aplicar el offset
            var inicioSemana = inicioSemanaActual.AddDays(offsetSemanas * 7);
            var finSemana = inicioSemana.AddDays(6); // Domingo de esa semana

            ViewBag.InicioSemana = inicioSemana;
            ViewBag.FinSemana = finSemana;

            var todasLasAsignaciones = await _serviceAsignacion.ListAsync();

            // Filtrar por la semana seleccionada 
            var asignacionesSemana = todasLasAsignaciones
                .Where(a => a.FechaAsignacion.Date >= inicioSemana
                         && a.FechaAsignacion.Date <= finSemana)
                .OrderBy(a => a.FechaAsignacion)
                .ToList();

            ViewBag.TotalAsignaciones = asignacionesSemana.Count;

            return View(asignacionesSemana);
        }
    }
}