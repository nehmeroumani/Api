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
    public class TweetAccountController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<TweetAccount>> GetAll()
        {
            var rd = Rd();

            var currentUser = P.Users.Get(int.Parse(User.Identity.Name));

            if (currentUser.RoleEnum == RoleEnum.Annotator)
                rd.Filter.Add(new FilterData("UserId", "eq", User.Identity.Name));

            var l = P.TweetAccounts.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<TweetAccount> Get(int id)
        {
            var i = P.TweetAccounts.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public TweetAccount Post([FromBody] TweetAccount value)
        {
            if (!IsAdmin) return null;
            value.CreationDate = DateTime.Now;
            value.LastModified = DateTime.Now;
            P.TweetAccounts.Insert(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] TweetAccount value)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.TweetAccounts.Get(id);
            if (i == null)
                return NotFound();
            P.TweetAccounts.Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.TweetAccounts.Get(id);
            if (i == null)
                return NotFound();

            P.TweetAccounts.Delete(id);
            return Ok(i);
        }
    }
}
