//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Core.Cache;
//using Core.Repositories;
//using Core.Repositories.Annotations;
//using Core.Repositories.Twitter;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace API.Controllers
//{
//    [Authorize]
//    [Route("api/[controller]")]
//    public class AnnotationTaskUserController : BaseController
//    {
//        [HttpGet]
//        public ActionResult<IEnumerable<AnnotationTaskUser>> GetAll()
//        {
//            var rd = Rd();
          
//            var l = P.AnnotationTaskUsers.GetAll(rd, out var total).ToList();
//            T(total, rd);
//            return l;
//        }

//        [HttpGet("{id}")]
//        public ActionResult<AnnotationTaskUser> Get(int id)
//        {
//            var i = P.AnnotationTaskUsers.Get(id);
//            if (i == null)
//                return NotFound();
//            return i;
//        }

//        [HttpPost]
//        public AnnotationTaskUser Post([FromBody] AnnotationTaskUser value)
//        {
//            P.AnnotationTaskUsers.Save(value);
//            return value;
//        }

//        // PUT api/values/5
//        [HttpPut("{id}")]
//        public IActionResult Put(int id, [FromBody] AnnotationTaskUser value)
//        {
//            var i = P.AnnotationTaskUsers.Get(id);
//            if (i == null)
//                return NotFound();
//            P.AnnotationTaskUsers.Save(value);
//            return Ok(value);
//        }

//        // DELETE api/values/5
//        [HttpDelete("{id}")]
//        public IActionResult Delete(int id)
//        {
//            var i = P.AnnotationTaskUsers.Get(id);
//            if (i == null)
//                return NotFound();

//            P.AnnotationTaskUsers.Delete(id);
//            return Ok(i);
//        }
//    }
//}
