using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Cache;
using Core.Repositories;
using Core.Repositories.Annotations;
using Core.Repositories.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AnnotationController : BaseController
    {
        //[HttpGet]
        //public ActionResult<IEnumerable<Annotation>> GetAll()
        //{
        //    var rd = Rd();

        //    var l = P.Annotations[UserId()].GetAll(rd, out var total).ToList();
        //    T(total, rd);
        //    return l;
        //}

        //[HttpGet("{id}")]
        //public ActionResult<Annotation> Get(int id)
        //{
        //    var i = P.Annotations[UserId()].Get(id);
        //    if (i == null)
        //        return NotFound();

        //    i.AnnotationReasons = P.AnnotationReasons[UserId()].GetWhere("AnnotationId=" + id);

        //    var words = P.AnnotationReasonWords[UserId()].GetByIds(i.AnnotationReasons.Select(x => x.Id).ToArray(),
        //        "AnnotationReasonId");

        //    i.AnnotationReasons.ForEach(x =>
        //        {
        //            x.AnnotationReasonWords = words.Where(z => z.AnnotationReasonId == x.Id).ToList();
        //        });

        //    return i;
        //}

        [HttpPost]
        public IActionResult Post([FromBody] Annotation annotation)
        {
            var annotationTaskUserTweet = Pool.I.AnnotationTaskUserTweets.Get(annotation.AnnotationTaskUserTweetId);

            var userid = annotationTaskUserTweet.UserId;

            var annotationTask = Pool.I.AnnotationTasks.Get(annotationTaskUserTweet.AnnotationTaskId);

            if (annotationTaskUserTweet.Status == (int)AnnotationTaskUserStatusEnum.Done)
                return BadRequest(new { message = "Cannot Edit Finished task" });

            //if (annotationTaskUserTweet.UserId.ToString() != User.Identity.Name.ToString())
            //    return BadRequest(new { message = "Task can be updated only by its annotater" });

            if (!annotationTask.StartTime.HasValue)
            {
                annotationTask.StartTime = DateTime.Now;
                annotationTask.Status = (int)AnnotationTaskUserStatusEnum.InProgress;
            }

            if (!annotationTaskUserTweet.StartTime.HasValue)
            {
                annotationTaskUserTweet.StartTime = DateTime.Now;
                annotationTaskUserTweet.Status = (int)AnnotationTaskUserStatusEnum.InProgress;
            }

            var duplicate = P.Annotations[userid]
                .GetWhere($"AnnotationTaskUserTweetId = {annotation.AnnotationTaskUserTweetId}" +
                          $" and CategoryId={annotation.CategoryId}" +
                          $" and DimensionId={annotation.DimensionId}").FirstOrDefault();
            if (duplicate != null && duplicate.Id != annotation.Id)
                return BadRequest(new { message = "Duplicate annotation" });

            P.Annotations[userid].Save(annotation);
            P.AnnotationTaskUserTweets.Save(annotationTaskUserTweet);
            P.AnnotationTasks.Save(annotationTask);
            return Ok(annotation);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Annotation value)
        {
            var userid = value.UserId;

            var i = P.Annotations[userid].Get(id);
            if (i == null)
                return NotFound();
            P.Annotations[userid].Save(value);
            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id,int userid)
        {
            var annotation = P.Annotations[userid].Get(id);
            if (annotation == null)
                return NotFound();

            var annotationTaskUserTweet = P.AnnotationTaskUserTweets.Get(annotation.AnnotationTaskUserTweetId);

            if (annotationTaskUserTweet.Status == (int)AnnotationTaskUserStatusEnum.Done)
                throw new Exception("Cannot Delete done task");

            var rs = P.AnnotationReasons[userid].GetWhere($"AnnotationId={id}").ToList();
            foreach (var r in rs)
            {
                var ws = P.AnnotationReasonWords[userid].GetWhere($"AnnotationReasonId={r.Id}").ToList();
                foreach (var w in ws)
                {
                    P.AnnotationReasonWords[userid].Delete(w.Id);
                }
                P.AnnotationReasons[userid].Delete(r.Id);
            }
            P.Annotations[userid].Delete(id);
            return Ok(annotation);
        }
    }
}
