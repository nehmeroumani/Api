
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
    public class TweetController : BaseController
    {
        [HttpGet]
        public ActionResult<IEnumerable<Tweet>> GetAll()
        {
            var rd = Rd();

            if (rd.Filter.Any(x => x.Key.ToLower() == "statistics"))
            {
                var ls = P.Tweets.GetStatistics(rd, out int totalTweets).ToList();
                T(totalTweets, rd);
                return ls;
            }
            var countFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "count");
            if (countFilter != null)
            {
                rd.Filter.Remove(countFilter);
                var count = P.Tweets.GetCount(rd);
                T(count, rd);
                return new List<Tweet>();
            }

            var l = P.Tweets.GetAllView(rd, out var total).ToList();
            T(total, rd);
            return l;
        }

        [HttpGet("{id}")]
        public ActionResult<Tweet> Get(int id)
        {
            var i = P.Tweets.Get(id);

            if (i == null)
                return NotFound();

            //var words = P.Words.GetAll().ToList();
            var tweetWords = P.TweetWords.GetWhere($"TweetId='{id}'");

            i.WordIds = tweetWords.OrderBy(x => x.Position).Select(z => z.WordId).ToArray();
            var words = P.Words.GetByIds(i.WordIds).ToList();
            i.TweetWords = new List<TweetWord>();
            foreach (var tw in tweetWords)
            {
                var word = words.Single(x => x.Id == tw.WordId);
                tw.WordName = word.Name;
                i.TweetWords.Add(tw);
            }
            return i;
        }

        [HttpGet("{id}/annotations")]
        public ActionResult<TweetAnnotationsData> GetAnnotations(int id)
        {
            var tweet = P.Tweets.Get(id);

            if (tweet == null)
                return NotFound();

            var users = P.Users.GetAll();
            var allCategories = P.Categorys.GetAll();
            var allSubCategories = P.Dimensions.GetAll();
            var tweetAnnotationsData = new TweetAnnotationsData();
            foreach(var user in users)
            {
                if (user.RoleEnum != RoleEnum.Admin)
                {
                     var annotations = P.Annotations[user.Id].GetAllTweetAnnotationsByUser(tweet.Id, user.Id);
                    if (annotations.Count > 0)
                    {
                        foreach (Annotation annotation in annotations)
                        {
                            if (!tweetAnnotationsData.Categories.ContainsKey(annotation.CategoryId.Value.ToString()))
                            {
                                tweetAnnotationsData.Categories[annotation.CategoryId.Value.ToString()] = allCategories.FirstOrDefault((c) => c.Id == annotation.CategoryId);
                            }
                            if (!tweetAnnotationsData.SubCategories.ContainsKey(annotation.DimensionId.Value.ToString()))
                            {
                                tweetAnnotationsData.SubCategories[annotation.DimensionId.Value.ToString()] = allSubCategories.FirstOrDefault((sc) => sc.Id == annotation.DimensionId);
                            }
                        }
                        tweetAnnotationsData.Users[user.Id.ToString()] = user;
                        tweetAnnotationsData.UserAnnotations[user.Id.ToString()] = annotations;
                    }
                }
            }
            
            return tweetAnnotationsData;
        }



        [HttpPost]
        public Tweet Post([FromBody] Tweet value)
        {
            if (!IsAdmin) return null;
            P.Tweets.Save(value);
            return value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id //[FromBody] string factStatus
            )
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Tweets.Get(id);
            if (i == null)
                return NotFound();
            P.Tweets.Save(i);
            return Ok(i);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin) return Unauthorized();
            var i = P.Tweets.Get(id);
            if (i == null)
                return NotFound();

            P.Tweets.Delete(id);
            return Ok(i);
        }


        public class TweetAnnotationsData
        {
            public Dictionary<string, List<Annotation>> UserAnnotations { get; }
            public Dictionary<string, Category> Categories { get; }
            public Dictionary<string, Dimension> SubCategories { get; }
            public Dictionary<string, User> Users { get; }

            public TweetAnnotationsData()
            {
                UserAnnotations = new Dictionary<string, List<Annotation>>();
                Categories = new Dictionary<string, Category>();
                SubCategories = new Dictionary<string, Dimension>();
                Users = new Dictionary<string, User>();
            }
        }
    }
}
