using System;
using System.Collections.Generic;
using System.Linq;
using API.Services;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            var rd = Rd();
            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.Users.GetCount(rd);
                T(count, rd);
                return new List<User>();
            }

            var l = P.Users.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var i = P.Users.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public IActionResult Post([FromBody] User value)
        {
            if (!IsAdmin) return Unauthorized();
            value.SetPassword(value.Password);
            var old = P.Users.GetByUsername(value.Username);
            if (old != null)
            {
                return NotFound("Username already exists");
            }
            P.Users.Save(value);
            return Ok(value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] User value)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Users.Get(id);
            if (i == null)
                return NotFound();
            if (i.Username != "admin")
            {
                var old = P.Users.GetByUsername(value.Username);
                if (old != null && old.Id != value.Id)
                {
                    return NotFound("Username already exists");
                }

                i.Name = value.Name;
                i.IsActive = value.IsActive;
                i.Username = value.Username;

                if (!string.IsNullOrEmpty(value.Password))
                    i.SetPassword(value.Password);
                P.Users.Save(i);
            }

            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Users.Get(id);
            if (i == null)
                return NotFound();

            if (i.Username == "admin")
                return Error("cannot delete admin record");

            var userTasks = P.AnnotationTasks.GetWhere($"UserId={id}").ToList();
            if (!userTasks.Any())
            {
                P.Users.Delete(id);
            }
            else
            {
                return Error("cannot delete admin record");
            }


            return Ok(i);
        }
    }
}