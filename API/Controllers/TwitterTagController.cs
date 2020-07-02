using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cache;
using Core.Repositories;
using Core.Repositories.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TwitterTagController : BaseController
    {
        private readonly ICacheManager _cache;
        public TwitterTagController(ICacheManager cache)
        {
            this._cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TwitterTag>> GetAll()
        {
            var rd = Rd();
            rd.Cache = true;
            var l = P.TwitterTags.GetAllView( rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<TwitterTag> Get(string id)
        {
            var i = P.TwitterTags.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public TwitterTag Post([FromBody] TwitterTag value)
        {
            value.CreationDate = DateTime.Now;
            value.LastModified = DateTime.Now;
            P.TwitterTags.Insert(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] TwitterTag value)
        {
            var i = P.TwitterTags.Get(id);
            if (i == null)
                return NotFound();
            P.TwitterTags.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var i = P.TwitterTags.Get(id);
            if (i == null)
                return NotFound();

            P.TwitterTags.Delete(id);
            return Ok(i);
        }
    }
}
