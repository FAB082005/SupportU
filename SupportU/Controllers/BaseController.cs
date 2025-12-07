using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace SupportU.Web.Controllers
{
    public class BaseController : Controller
    {
        // Diccionario por petición (case-insensitive)
        protected Dictionary<string, string> Translations = new(StringComparer.OrdinalIgnoreCase);

        // Se ejecuta antes de cada acción: aquí la cultura ya está aplicada por RequestLocalization
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            try
            {
                var culture = CultureInfo.CurrentUICulture.Name ?? "es-CR";
                var jsonPath = System.IO.Path.Combine(AppContext.BaseDirectory, "locales", $"{culture}.json");

                // Log: cultura y ruta
                Log.Information("i18n: CurrentUICulture = {Culture}; Looking for JSON at {JsonPath}", culture, jsonPath);

                if (System.IO.File.Exists(jsonPath))
                {
                    Log.Information("i18n: JSON file exists: {JsonPath}", jsonPath);

                    try
                    {
                        var json = System.IO.File.ReadAllText(jsonPath);
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (parsed != null)
                        {
                            Translations = new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
                            Log.Information("i18n: Loaded {Count} translations for culture {Culture}", Translations.Count, culture);
                        }
                        else
                        {
                            Translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            Log.Warning("i18n: JSON parsed to null for {JsonPath}", jsonPath);
                        }
                    }
                    catch (Exception exJson)
                    {
                        Translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        Log.Error(exJson, "i18n: Error deserializing JSON at {JsonPath}", jsonPath);
                    }
                }
                else
                {
                    Translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    Log.Warning("i18n: JSON file NOT found at {JsonPath}", jsonPath);
                }
            }
            catch (Exception ex)
            {
                Translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Log.Error(ex, "i18n: Unexpected error loading translations");
            }

            // Exponer a la vista
            try
            {
                ViewData["Translations"] = Translations;
            }
            catch (Exception exView)
            {
                Log.Warning(exView, "i18n: Could not set ViewData[\"Translations\"]");
            }
        }

        // Método auxiliar para uso interno si lo necesitas
        protected string T(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return Translations.TryGetValue(key, out var value) ? value : key;
        }
    }
}
