using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class BaseController : ControllerBase
    {
    }
}
