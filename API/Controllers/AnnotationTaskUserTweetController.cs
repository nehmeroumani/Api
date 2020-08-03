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
    public class AnnotationTaskUserTweetController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<AnnotationTaskUserTweet>> GetAll()
        {
            var rd = Rd();       


            if (User.IsInRole(((int)RoleEnum.Annotator).ToString()))
            {
                rd.Filter.Add(new FilterData("UserId", "eq", User.Identity.Name));
            }

            if (rd.Filter.Any(x => x.Key.ToLower() == "statistics-status"))
            {
                var ls = P.AnnotationTaskUserTweets.GetStatisticsStatus(rd, out int t).ToList();
                T(t, rd);
                return ls;
            }

            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.AnnotationTaskUserTweets.GetCount(rd);
                T(count, rd);
                return new List<AnnotationTaskUserTweet>();
            }



            var l = P.AnnotationTaskUserTweets.GetAllView(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<AnnotationTaskUserTweet> Get(int id)
        {
            var i = P.AnnotationTaskUserTweets.Get(id);
            if (i == null)
                return NotFound();

            var userid = i.UserId;

            i.UserName = P.Users.Get(userid)?.Name;
            
            i.Annotations = P.Annotations[userid].GetWhere("AnnotationTaskUserTweetId=" + id);
            
            var reasons = P.AnnotationReasons[userid].GetByIds(i.Annotations.Select(x => x.Id).ToArray(), "AnnotationId").ToList();

            var words = P.AnnotationReasonWords[userid].GetByIds(reasons.Select(x => x.Id).ToArray(), "AnnotationReasonId");

            reasons.ForEach(x =>
            {
                x.AnnotationReasonWords = words.Where(z => z.AnnotationReasonId == x.Id).ToList();
            });

            i.Annotations.ForEach(x =>
            {
                x.AnnotationReasons = reasons.Where(z => z.AnnotationId == x.Id).ToList();
            });
            return i;
        }     


        //Finish Task
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] AnnotationTaskUserTweet value)
        {
            var i = P.AnnotationTaskUserTweets.Get(id);
            if (i == null)
                return NotFound();
            if (!value.LevelOfConfidenceId.HasValue && value.LevelOfConfidenceId<=0)
                return Error("LevelOfConfidenceId is not provided");

            if (
                i.Status == (int)AnnotationTaskUserStatusEnum.InProgress) // && User.Identity.Name == i.UserId.ToString()
            {
                i.Status = (int)AnnotationTaskUserStatusEnum.Done;
                i.FinishTime = DateTime.Now;
                i.TaskDuration = (int)(i.FinishTime.Value.Subtract(i.StartTime.Value)).TotalSeconds;
                i.IsIrrelevant = value.IsIrrelevant;
                i.LevelOfConfidenceId = value.LevelOfConfidenceId;
                P.AnnotationTaskUserTweets.Save(i);
                value.NextAnnotationId = P.AnnotationTaskUserTweets.GetNext(User.Identity.Name)?.Id ?? 0;
            }
            else
            {
               return Error("cannot edit Done Task");
            }

            return Ok(value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.AnnotationTaskUserTweets.Get(id);
            if (i == null)
                return NotFound();
           

            return Ok(i);
        }
    }
}
