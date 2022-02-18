using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PhysiciansFormApi.Model.Connections;
using PhysiciansFormApi.Model.Services;

namespace PhysiciansFormApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatBoxController : Controller
    {
        
        [HttpGet("{nhName}")]
        public ActionResult<object> Get(string nhName)
        {
            var responseService = new ResponseService(new KrollPharmacyConnection());
            return responseService.Select(nhName);
        }
    }
}