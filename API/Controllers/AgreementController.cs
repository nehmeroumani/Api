using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Repositories;
using Core.Repositories.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgreementController : BaseController
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<TweetAgreement>> GetAll()
        {
            var rd = Rd();
            var userId1 =int.Parse(rd.Filter.Single(x => x.Key.ToLower() == "userid1").Value);
            var userId2 = int.Parse(rd.Filter.Single(x => x.Key.ToLower() == "userid2").Value);

            var annotationsTaskUser1 = Pool.I.AnnotationTaskUserTweets.GetAllView( "Status = 30 AND UserId =" + userId1).ToList();
            var annotationsTaskUser2 = Pool.I.AnnotationTaskUserTweets.GetAllView("Status = 30 AND UserId =" + userId2).ToList();

            var annotationsUser1 = Pool.I.Annotations[userId1].GetAllView("UserId =" + userId1).ToList();
            var annotationsUser2 = Pool.I.Annotations[userId2].GetAllView("UserId =" + userId2).ToList();

            var annotationsUserReason1 = Pool.I.AnnotationReasons[userId1].GetAllView("UserId =" + userId1).ToList();
            var annotationsUserReason2 = Pool.I.AnnotationReasons[userId2].GetAllView("UserId =" + userId2).ToList();


            var categories = Pool.I.Categorys.GetAll().ToList();
            var dimensions = P.Dimensions.GetAll().ToList();
            categories.ForEach(x => x.Dimensions = dimensions.Where(z => z.CategoryId == x.Id).OrderBy(z => z.DisplayOrder).ToList());

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

                var tweetAnnotation1 = annotationsUser1.Where(x => x.TweetId == t).ToList();
                var tweetAnnotation2 = annotationsUser2.Where(x => x.TweetId == t).ToList();

                var tweetAnnotationReason1 = annotationsUserReason1.Where(x => x.TweetId == t).ToList();
                var tweetAnnotationReason2 = annotationsUserReason2.Where(x => x.TweetId == t).ToList();

                var words = Pool.I.TweetWords.GetWhere("TweetId=" + t).ToList();

                var ta = new TweetAgreement
                {
                    Id = t,
                    TweetId = t
                };

                AgreementCategory(ta, categories, words, tweetAnnotation1, tweetAnnotation2, tweetAnnotationReason1, tweetAnnotationReason2);

                lst.Add(ta);
            }
            T(lst.Count, rd);
            return lst;
        }

        private void AgreementCategory(TweetAgreement ta, List<Category> categories, List<TweetWord> words,
            List<Annotation> tweetAnnotation1, List<Annotation> tweetAnnotation2,
            List<AnnotationReason> tweetAnnotationReason1,
            List<AnnotationReason> tweetAnnotationReason2)
        {
            Matrix matrix = new Matrix();
            foreach (var category in categories)
            {
                var tc1 = tweetAnnotation1.Any(x => x.CategoryId == category.Id) ? 1 : 0;
                var tc2 = tweetAnnotation2.Any(x => x.CategoryId == category.Id) ? 1 : 0;
                FillMatrix(matrix, tc1, tc2);
            }
            var calc = CalculateMetrics(matrix);
            ta.CategoryAgreement = calc.F1Measure;


            Matrix matrixDimension = new Matrix();
            var dimensions = categories.SelectMany(x => x.Dimensions).ToList();
            foreach (var d in dimensions)
            {
                var tc1 = tweetAnnotation1.Any(x => x.DimensionId == d.Id) ? 1 : 0;
                var tc2 = tweetAnnotation2.Any(x => x.DimensionId == d.Id) ? 1 : 0;
                FillMatrix(matrixDimension, tc1, tc2);
            }
            var calcDimension = CalculateMetrics(matrix);
            ta.DimensionAgreement = calcDimension.F1Measure;


            Matrix matrixReason = new Matrix();
            foreach (var w in words)
            {
                foreach (var dimension in dimensions)
                {
                    var tc1 = tweetAnnotationReason1.Any(x => x.DimensionId == dimension.Id && x.StartWordId == w.Id) ? 1 : 0;
                    var tc2 = tweetAnnotationReason2.Any(x => x.DimensionId == dimension.Id && x.StartWordId == w.Id) ? 1 : 0;
                    FillMatrix(matrixReason, tc1, tc2);
                }
            }

            var calcReason = CalculateMetrics(matrixReason);
            ta.ReasonAgreement = calcReason.F1Measure;
        }





        private void FillMatrix(Matrix m, int actual, int predicted)
        {
            if (actual == 1 && predicted == 1)
                m.TruePositive++;
            else if (actual == 1 && predicted == 0)
                m.FalsePositive++;
            else if (actual == 0 && predicted == 1)
                m.FalseNegative++;
            else if (actual == 0 && predicted == 0)
                m.TrueNegative++;
        }

        private MatrixCalculations CalculateMetrics(Matrix m)
        {
            var precision = m.TruePositive + m.FalsePositive == 0 ? -1 : m.TruePositive * 1.0 / (m.TruePositive + m.FalsePositive);
            var recall = m.TruePositive + m.FalseNegative == 0 ? -1 : m.TruePositive * 1.0 / (m.TruePositive + m.FalseNegative);
            var f1Measure = precision + recall == 0 ? 0 : (2 * (precision * recall) / (precision + recall));

            return new MatrixCalculations()
            {
                Recall = recall,
                Precision = precision,
                F1Measure = f1Measure
            };
        }
        [HttpPost]
        public IActionResult Post([FromBody] AssignForm value)
        {
            var task = new AnnotationTask();

            task.CreatedByUserId = int.Parse(User.Identity.Name);
            task.Status = (int)AnnotationTaskUserStatusEnum.New;
            task.UserId = value.UserId;
            task.StartTweetId = -1;
            task.EndTweetId = -1;


            if (value.TweetIds == null || value.TweetIds.Length == 0)
            {
                return BadRequest(new { message = "No selected Tweets" });
            }

            task.CreatedByUserId = int.Parse(User.Identity.Name);

            P.AnnotationTasks.Save(task);

            var annotations = new List<AnnotationTaskUserTweet>();


            foreach (var t in value.TweetIds)
            {
                var annotation = new AnnotationTaskUserTweet()
                {
                    CreationDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    AnnotationTaskId = task.Id,
                    TweetId = t,
                    UserId = value.UserId,
                    Status = (int)AnnotationTaskUserStatusEnum.New
                };
                annotations.Add(annotation);
            }

            if (annotations.Any())
                P.AnnotationTaskUserTweets.Insert(annotations);

            return Ok(value);
        }
    }

    public class MatrixCalculations
    {
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Measure { get; set; }
    }
    public class Matrix
    {
        public Matrix()
        {
            TruePositive = 0;
            TrueNegative = 0;
            FalsePositive = 0;
            FalseNegative = 0;
        }
        public int TruePositive { get; set; }
        public int TrueNegative { get; set; }
        public int FalsePositive { get; set; }
        public int FalseNegative { get; set; }
    }

    public class AssignForm
    {
        public int UserId { get; set; }
        public int[] TweetIds { get; set; }
    }
    public class TweetAgreement
    {
        public int Id { get; set; }
        public int TweetId { get; set; }
        public double CategoryAgreement { get; set; }//F1 measure
        public double DimensionAgreement { get; set; }
        public double ReasonAgreement { get; set; }
    }
}
