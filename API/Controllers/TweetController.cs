
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cache;
using Core.Repositories;
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
    }
}
