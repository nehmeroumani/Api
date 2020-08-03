using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories.Annotations
{
    public class AnnotationRepository : BaseIntRepository<Annotation>
    {
        public AnnotationRepository(int userId)
        {
            base.Init("Annotation_" + userId, "AnnotationTaskUserTweetId,CategoryId,DimensionId,AnnotationTaskId");
        }


        public List<Annotation> GetAllView(int size,int page)
        {
            string query =$"SELECT  a.Id, a.CreationDate, a.LastModified, a.IsDeleted, a.AnnotationTaskId, a.CategoryId," +
                          $" a.DimensionId,a.AnnotationTaskUserTweetId, dbo.AnnotationTaskUserTweet.TweetId, dbo.AnnotationTaskUserTweet.UserId FROM " +
                          $"  {TableName} AS a INNER JOIN AnnotationTaskUserTweet ON a.AnnotationTaskUserTweetId = dbo.AnnotationTaskUserTweet.Id";


            return GetAll(size, page, "", "CreationDate Desc", query, true).ToList();
        }
    }

    public class Annotation : BaseIntModel
    {
        public int AnnotationTaskUserTweetId { get; set; }
        public int AnnotationTaskId { get; set; }
        public int? CategoryId { get; set; }
        public int? DimensionId { get; set; }
        public List<AnnotationReason> AnnotationReasons { get; set; }





        //For views
        public int TweetId { get; set; }
        public int UserId { get; set; }

    }
}
