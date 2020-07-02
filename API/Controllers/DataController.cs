using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            try
            {
                return Pool.I.Categorys.GetAll().First().Name;
            }
            catch (Exception e)
            {
                return e.Message + " Errorrooror " + e.StackTrace;
            }
        }
    }
}
