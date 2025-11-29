namespace SupportU.Web.Util
{
    public class SweetAlertHelper
    {
        public static string Mensaje(string titulo, string mensaje, SweetAlertMessageType
        tipoMensaje)
        {
            return "Swal.fire({ title: '" + titulo + "',text: '" + mensaje + "',icon: '" +
            tipoMensaje + "'});";
        }
    }
    public enum SweetAlertMessageType
    {
        success, error, warning, info, question
    }
}
