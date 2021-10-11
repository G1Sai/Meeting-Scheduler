using Microsoft.AspNetCore.Mvc;

namespace Meeting_Scheduler.Controllers
{
    //ErrorController
    public class ErrorController : Controller
    {
        [Route("/error")]
        public IActionResult Error() => Problem();
    }
}
