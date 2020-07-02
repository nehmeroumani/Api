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
    public class WordController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<Word>> GetAll()
        {
            var rd = Rd();
          
            var l = P.Words.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<Word> Get(int id)
        {
            var i = P.Words.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public Word Post([FromBody] Word value)
        {
            if (!IsAdmin) return null;
            P.Words.Save(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Word value)
        {
            if(!IsAdmin) return Unauthorized();

            var i = P.Words.Get(id);
            if (i == null)
                return NotFound();
            P.Words.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Words.Get(id);
            if (i == null)
                return NotFound();

            P.Words.Delete(id);
            return Ok(i);
        }
    }
}
