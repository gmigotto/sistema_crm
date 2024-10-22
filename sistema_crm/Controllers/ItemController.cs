using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;

namespace sistema_crm.Controllers
{
    public class ItemController : Controller
    {
        [HttpPost]
        public JsonResult Gravar([FromBody] ItemModel item)
        {
            if (ModelState.IsValid)
            {
                item.GravarItem();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Erro ao gravar o item." });
        }
    }
}
