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
    public class AnnotationTaskController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<AnnotationTask>> GetAll()
        {
            var rd = Rd();
            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.AnnotationTasks.GetCount(rd);
                T(count, rd);
                return new List<AnnotationTask>();
            }

            var l = P.AnnotationTasks.GetAllView(rd, out var total).ToList();

            //if (l.Any() && (rd.Ids == null || rd.Ids.Count <= 1))
            //{
            //    var related = P.AnnotationTaskUsers.GetByIds(l.Select(x => x.Id).ToArray(), "AnnotationTaskId").ToList();
            //    foreach (var c in l)
            //        c.UserIds = related.Where(x => !x.IsDeleted && x.AnnotationTaskId == c.Id).Select(z => z.UserId).ToArray();
            //}

            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<AnnotationTask> Get(int id)
        {
            var i = P.AnnotationTasks.Get(id);

            if (i == null)
                return NotFound();

            //i.UserIds = Pool.I.AnnotationTaskUsers.GetWhere("AnnotationTaskId=" + id).Select(z => z.UserId).ToArray();
            return i;
        }


        [HttpPost]
        public IActionResult Post([FromBody] AnnotationTask value)
        {
            value.CreatedByUserId = int.Parse(User.Identity.Name);
            value.Status = (int)AnnotationTaskUserStatusEnum.New;

            if (value.StartTweetId > value.EndTweetId)
            {
                return BadRequest(new { message = "StartTweetId should be less than or equal EndTweetId" });
            }

            var startTweet = Pool.I.Tweets.Get(value.StartTweetId);
            if (startTweet == null)
            {
                return BadRequest(new { message = "Start Tweet Not Found" });
            }
            var endTweet = Pool.I.Tweets.Get(value.EndTweetId);

            if (endTweet == null)
            {
                return BadRequest(new { message = "End Tweet Not Found" });
            }

            value.CreatedByUserId = int.Parse(User.Identity.Name);

            P.AnnotationTasks.Save(value);

            var annotations = new List<AnnotationTaskUserTweet>();

            var tweets = Pool.I.Tweets.GetRange(value.StartTweetId, value.EndTweetId);

            foreach (var t in tweets)
            {
                var annotation = new AnnotationTaskUserTweet()
                {
                    CreationDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    AnnotationTaskId = value.Id,
                    TweetId = t.Id,
                    UserId = value.UserId,
                    Status = (int)AnnotationTaskUserStatusEnum.New
                };
                annotations.Add(annotation);
            }

            if (annotations.Any())
                P.AnnotationTaskUserTweets.Insert(annotations);

            return Ok(value);
        }

        // PUT api/values/5
        //[HttpPut("{id}")]
        //public IActionResult Put(int id, [FromBody] AnnotationTask value)
        //{
        //    var i = P.AnnotationTasks.Get(id);
        //    if (i == null)
        //        return NotFound();

        //    //if (!i.IsFinished)
        //    //{
        //    value.CreatedByUserId = int.Parse(User.Identity.Name);

        //    //var old = Pool.I.AnnotationTaskUsers.GetWhere("AnnotationTaskId=" + id);
        //    //foreach (var u in value.UserIds)
        //    //    if (old.All(x => x.UserId != u))
        //    //        P.AnnotationTaskUsers.Save(new AnnotationTaskUser
        //    //        {
        //    //            AnnotationTaskId = value.Id,
        //    //            UserId = u
        //    //        });

        //    //foreach (var o in old)
        //    //{
        //    //    if (value.UserIds.All(x => x != o.UserId))
        //    //        P.AnnotationTaskUsers.Delete(o.UserId);
        //    //}

        //    //if (value.IsFinished)
        //    //{
        //    var annotations = new List<AnnotationTaskUserTweet>();
        //    var tweets = Pool.I.Tweets.GetRange(value.StartTweetId, value.EndTweetId);
        //    //foreach (var userId in value.UserIds)
        //    //{
        //    foreach (var t in tweets)
        //    {
        //        var annotation = new AnnotationTaskUserTweet()
        //        {
        //            CreationDate = DateTime.Now,
        //            LastModified = DateTime.Now,
        //            AnnotationTaskId = id,
        //            TweetId = t.Id,
        //            UserId = value.UserId,
        //            Status = (int)AnnotationTaskUserStatusEnum.New
        //        };
        //        annotations.Add(annotation);
        //    }
        //    // }
        //    if (annotations.Any())
        //        P.AnnotationTaskUserTweets.Insert(annotations);
        //    // }
        //    P.AnnotationTasks.Save(value);
        //    // }

        //    return Ok(value);
        //}

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var i = P.AnnotationTasks.Get(id);
            if (i == null)
                return NotFound();

            var annotations = P.Annotations[i.UserId].GetWhere($"AnnotationTaskId={id}");
            if (annotations.Any())
            {
                return Error("Cannot Delete Task/it have already annotations");
            }

            var subUserTweets = P.AnnotationTaskUserTweets.GetWhere($"AnnotationTaskId={id}");
            foreach (var annotationTaskUserTweet in subUserTweets)
            {
                P.AnnotationTaskUserTweets.Delete(annotationTaskUserTweet.Id);
            }
            P.AnnotationTasks.Delete(id);

            return Ok(i);
        }
    }
}
