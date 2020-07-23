using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgreementController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<TweetAgreement>> Get(int userId1, int userId2)
        {
            var annotationsTaskUser1 = Pool.I.AnnotationTaskUserTweets.GetView("Status = 30 AND UserId =" + userId1).ToList();
            var annotationsTaskUser2 = Pool.I.AnnotationTaskUserTweets.GetView("Status = 30 AND UserId =" + userId2).ToList();

            var annotationsUser1 = Pool.I.Annotations.GetView("UserId =" + userId1).ToList();
            var annotationsUser2 = Pool.I.Annotations.GetView("UserId =" + userId2).ToList();

            var categories = Pool.I.Categorys.GetAll().ToList();
            var intersectedTweets = new List<int>();

            foreach (var a in annotationsTaskUser1)
            {
                if (annotationsTaskUser2.Any(x => x.TweetId == a.TweetId))
                {
                    intersectedTweets.Add(a.TweetId);
                }
            }

            var lst = new List<TweetAgreement>();

            foreach (var t in intersectedTweets)
            {
                int true_positive = 0, true_negative = 0, false_positive = 0, false_negative = 0;

                var tweetAnnotation1 = annotationsUser1.Where(x => x.TweetId == t).ToList();
                var tweetAnnotation2 = annotationsUser2.Where(x => x.TweetId == t).ToList();

                foreach (var category in categories)
                {
                    var tc1 = tweetAnnotation1.Any(x => x.CategoryId == category.Id) ? 1 : 0;
                    var tc2 = tweetAnnotation2.Any(x => x.CategoryId == category.Id) ? 1 : 0;


                    if (tc1 == 1 && tc2 == 1)
                        true_positive++;
                    else if (tc1 == 1 && tc2 == 0)
                        false_positive++;
                    else if (tc1 == 0 && tc2 == 1)
                        false_negative++;
                    else if (tc1 == 0 && tc2 == 0)
                        true_negative++;

                }

                var precision = true_positive + false_positive == 0 ? -1 : true_positive / (true_positive + false_positive);
                var recall = true_positive + false_negative == 0 ? -1 : true_positive / (true_positive + false_negative);
                var f1Measure = precision + recall == 0 ? 0 : (2 * (precision * recall) / (precision + recall));

                var ta = new TweetAgreement
                {
                    TweetId = t,
                    CategoryAgreement = f1Measure,
                    Precision = precision,
                    Recall = recall
                };

                lst.Add(ta);
            }
            return lst;
        }
    }

    public class TweetAgreement
    {
        public int TweetId { get; set; }
        public int CategoryAgreement { get; set; }//F1 measure
        public int Precision { get; set; }
        public int Recall { get; set; }
    }
}
