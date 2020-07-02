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
    public class LevelOfConfidenceController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<LevelOfConfidence>> GetAll()
        {
            var rd = Rd();
            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.LevelOfConfidences.GetCount(rd);
                T(count, rd);
                return new List<LevelOfConfidence>();
            }
            var l = P.LevelOfConfidences.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<LevelOfConfidence> Get(int id)
        {
            var i = P.LevelOfConfidences.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public LevelOfConfidence Post([FromBody] LevelOfConfidence value)
        {
            if (!IsAdmin) return null;
            P.LevelOfConfidences.Save(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] LevelOfConfidence value)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.LevelOfConfidences.Get(id);
            if (i == null)
                return NotFound();
            P.LevelOfConfidences.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.LevelOfConfidences.Get(id);
            if (i == null)
                return NotFound();

            P.LevelOfConfidences.Delete(id);
            return Ok(i);
        }
    }
}
