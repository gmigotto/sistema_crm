using Microsoft.AspNetCore.Mvc;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using System.Threading.Tasks;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using MercadoPago.Error;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        public PaymentController()
        {
            // Defina o AccessToken da API Mercado Pago aqui
            MercadoPagoConfig.AccessToken = "APP_USR-2052178439112313-090420-f8a8150ea12d0f8cfbfdffd6fdbdcc7f-420083418";
        }

        public ActionResult FinalizarCompra()
        {
            return View();
        }

        
    }
}
