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
    public class CategoryController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetAll()
        {
            var rd = Rd();
            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.Categorys.GetCount(rd);
                T(count, rd);
                return new List<Category>();
            }
            var l = P.Categorys.GetAll(rd, out var total).ToList();
            var dimensions = P.Dimensions.GetAll().ToList();
            l.ForEach(x => x.Dimensions = dimensions.Where(z => z.CategoryId == x.Id).OrderBy(z=>z.DisplayOrder).ToList());
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<Category> Get(int id)
        {
            var i = P.Categorys.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public Category Post([FromBody] Category value)
        {
            if (!IsAdmin) return null;
            P.Categorys.Save(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Category value)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Categorys.Get(id);
            if (i == null)
                return NotFound();
            P.Categorys.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Categorys.Get(id);
            if (i == null)
                return NotFound();

            var dimensions = P.Dimensions.GetWhere($"CategoryId={id}").ToList();
            if (!dimensions.Any())
            {
                P.Categorys.Delete(id);
            }
            return Ok(i);
        }
    }
}
