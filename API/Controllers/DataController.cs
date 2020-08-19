using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : BaseController
    {
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            try
            {
                return Pool.I.Categorys.GetAll().First().Name;
            }
            catch (Exception e)
            {
                return e.Message + " Errorrooror " + e.StackTrace;
            }
        }


        [HttpPost("reset")]
        public IActionResult Reset([FromBody] ResetDataForm value)
        {
            if (!IsAdmin) return Unauthorized();

            if (value.resetTweets)
            {
                //delete tasks and annotations
                DeleteTasksAndAnnotations();
                //delete tweets
                DeleteTweets();
            }

            if (value.resetCategories)
            {
                if (!value.resetTweets)
                {
                    //delete tasks and annotations
                    DeleteTasksAndAnnotations();
                }
                //delete categories
                DeleteCategories();
            }

            if (value.resetTasksAndAnnotations)
            {
                if (!value.resetTweets && !value.resetCategories)
                {
                    //delete tasks and annotations
                    DeleteTasksAndAnnotations();
                }
            }

            return Ok(new {message = "Reset is complete"});
        }

        public void DeleteTasksAndAnnotations()
        {
            var users = P.Users.GetAll();
            foreach (User user in users)
            {
                if (user.RoleEnum != RoleEnum.Admin)
                {
                    P.AnnotationReasonWords[user.Id].DeleteAll();
                    P.AnnotationReasons[user.Id].DeleteAll();
                    P.Annotations[user.Id].DeleteAll();
                    P.AnnotationTaskUserTweets.DeleteAll();
                    P.AnnotationTasks.DeleteAll();
                }
            }
        }

        public void DeleteTweets()
        {
            P.TweetWords.DeleteAll();
            P.Tweets.DeleteAll();
        }

        public void DeleteCategories()
        {
            P.Dimensions.DeleteAll();
            P.Categorys.DeleteAll();
        }

        public class ResetDataForm
        {
            public bool resetTweets { get; set; }
            public bool resetCategories { get; set; }
            public bool resetTasksAndAnnotations { get; set; }
        }
    }
}
