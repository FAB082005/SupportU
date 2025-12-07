using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SupportU.Web.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        public IActionResult Set(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
                culture = "es-CR";

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, HttpOnly = false }
            );

            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }
    }
}
