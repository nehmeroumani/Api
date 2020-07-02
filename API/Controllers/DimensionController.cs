using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cache;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class DimensionController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<Dimension>> GetAll()
        {
            var rd = Rd();
            rd.Cache = true;
            var l = P.Dimensions.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<Dimension> Get(int id)
        {
            var i = P.Dimensions.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public Dimension Post([FromBody] Dimension value)
        {
            if (!IsAdmin) return null;
            P.Dimensions.Save(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Dimension value)
        {
            var i = P.Dimensions.Get(id);
            if (i == null)
                return NotFound();
            P.Dimensions.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Dimensions.Get(id);
            if (i == null)
                return NotFound();

            var annotations = P.Annotations.GetWhere($"DimensionId={id}").ToList();
            if (!annotations.Any())
            {
                P.Dimensions.Delete(id);
            }
            return Ok(i);
        }
    }
}
