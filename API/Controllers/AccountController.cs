using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Services;
using Core;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult<AccountView> Authenticate([FromBody]User userParam)
        {
            var user = _service.Authenticate(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            if (user.Role == (RoleEnum.Annotator).ToString().ToLower())
            {
                user.NextAnnotationId = Pool.I.AnnotationTaskUserTweets.GetNext(user.Id.ToString())?.Id ?? 0;
            }

            return user;
        }
    }
}