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

        public List<Annotation> GetAllTweetAnnotationsByUser(int tweeId, int userId)
        {
            string Where = $"WHERE a.IsDeleted=0 AND atut.Status=30 AND atut.UserId = '{userId}' AND atut.TweetId={tweeId}";
            string query = $"SELECT a.Id, a.CreationDate, a.LastModified, a.IsDeleted, a.AnnotationTaskId, a.CategoryId," +
                         $" a.DimensionId, a.AnnotationTaskUserTweetId FROM " +
                         $"{TableName} AS a INNER JOIN AnnotationTaskUserTweet atut ON a.AnnotationTaskUserTweetId = atut.Id "+Where;

            var data = Query<Annotation>(query).ToList();
            return data;
        }

        public List<AnnotationStatistics> GetStatistics(int userId)
        {
            string Where = $"WHERE a.IsDeleted=0 AND atut.Status=30 AND atut.UserId ={userId}";
            string query = $"SELECT COUNT(*) AS Total, a.CategoryId, a.DimensionId FROM " +
                         $"{TableName} AS a INNER JOIN AnnotationTaskUserTweet atut ON a.AnnotationTaskUserTweetId = atut.Id " + Where +
                         " GROUP BY a.CategoryId, a.DimensionId";
            var data = Query<AnnotationStatistics>(query).ToList();
            if (data.Count > 0)
            {
                data.ForEach((AnnotationStatistics stats) =>
                {
                    stats.UserId = userId;
                });
            }
            return data;

        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
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

    public class AnnotationStatistics
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int DimensionId { get; set; }
        public int Total { get; set; }
    }
}
