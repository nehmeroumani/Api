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
    public class RoleController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<Role>> GetAll()
        {
            var lst = Role.List();
            Response.Headers.Add("Content-Range", $"records 1-1/" + lst.Count);
            return lst;
        }

        [HttpGet("{id}")]
        public ActionResult<Role> Get(int id)
        {
            return Role.List().SingleOrDefault(x => x.Id == id);
        }
    }
}