using Microsoft.AspNetCore.Mvc;
using SupportU.Application.Services;

namespace SupportU.Web.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly IServiceTecnico _service;
        public TecnicoController(IServiceTecnico service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }


        public async Task<IActionResult> Index()
        {
            var list = await _service.ListAsync();
            return View(list);
        }
    }
}
