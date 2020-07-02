using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AnnotationReasonController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<AnnotationReason>> GetAll()
        {
            var rd = Rd();

            var l = P.AnnotationReasons.GetAll(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<AnnotationReason> Get(int id)
        {
            var i = P.AnnotationReasons.Get(id);
            if (i == null)
                return NotFound();
            return i;
        }

        [HttpPost]
        public IActionResult Save([FromBody] AnnotationReason value)
        {
            var annotation = P.Annotations.Get(value.AnnotationId);
            var annotationTaskUserTweet = P.AnnotationTaskUserTweets.Get(annotation.AnnotationTaskUserTweetId);

            if (annotationTaskUserTweet.Status == (int)AnnotationTaskUserStatusEnum.Done)
               return Error("Cannot Edit done task");

            //if (annotationTaskUserTweet.UserId.ToString() != User.Identity.Name.ToString())
            //    return Error("Cannot Edit task only by its user");

            if (value.Id == 0)
            {
                P.AnnotationReasons.Save(value);

                int i = 0;
                var oldWords = P.AnnotationReasonWords.GetWhere("AnnotationReasonId=" + value.Id);
                if (value.AnnotationReasonWords != null)
                {
                    foreach (var u in value.AnnotationReasonWords)
                    {
                        if (oldWords.All(x => x.TweetWordId != u.TweetWordId))
                            P.AnnotationReasonWords.Save(new AnnotationReasonWord
                            {
                                TweetWordId = u.TweetWordId,
                                AnnotationReasonId = value.Id
                            });
                        if (i == 0)
                        {
                            var tw = P.TweetWords.Get(u.TweetWordId);
                            value.StartWordId = tw.Id;
                            value.StartWordPosition = tw.Position;
                        }
                        else if (i == value.AnnotationReasonWords.Count() - 1)
                        {
                            var tw = P.TweetWords.Get(u.TweetWordId);
                            value.EndWordId = tw.Id;
                            value.EndWordPosition = tw.Position;
                        }
                    }
                    foreach (var o in oldWords)
                    {
                        if (value.AnnotationReasonWords.All(x => x.TweetWordId != o.TweetWordId))
                            P.AnnotationReasonWords.Delete(o.Id);
                    }
                }

                P.AnnotationReasons.Save(value);
            }
            return Ok(value);
        }

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public IActionResult Put(int id, [FromBody] AnnotationReason value)
        //{
        //    var i = P.AnnotationReasons.Get(id);
        //    if (i == null)
        //        return NotFound();
        //    P.AnnotationReasons.Save(value);
        //    return Ok(value);
        //}

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var i = P.AnnotationReasons.Get(id);
            if (i == null)
                return NotFound();

            var annotation = P.Annotations.Get(i.AnnotationId);
            var annotationTaskUserTweet = P.AnnotationTaskUserTweets.Get(annotation.AnnotationTaskUserTweetId);

            if (annotationTaskUserTweet.Status == (int)AnnotationTaskUserStatusEnum.Done)
                return Error("Cannot Edit done task");
           
            //if (annotationTaskUserTweet.UserId.ToString() != User.Identity.Name.ToString())
            //    return Error("Cannot Edit task only by its user");

            P.AnnotationReasons.Delete(id);
            return Ok(i);
        }
    }
}
