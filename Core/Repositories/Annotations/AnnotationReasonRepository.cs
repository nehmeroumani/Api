using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories.Annotations
{
    public class AnnotationReasonRepository : BaseIntRepository<AnnotationReason>
    {
        public AnnotationReasonRepository(int userId)
        {
            base.Init("AnnotationReason_" + userId, "AnnotationId,CategoryId,DimensionId,StartWordId,EndWordId,StartWordPosition,EndWordPosition"); 
        }

        public List<AnnotationReason> GetAllView(int size, int page,int userId)
        {
            string query =
                "SELECT  ar.Id, ar.CreationDate, ar.LastModified, ar.IsDeleted, ar.AnnotationId, " +
                "ar.CategoryId, ar.DimensionId, ar.StartWordId, ar.EndWordId, ar.StartWordPosition," +
                " ar.EndWordPosition, atu.TweetId, atu.UserId FROM " +
                $"  {TableName} AS ar INNER JOIN Annotation_{userId} a ON ar.AnnotationId = a.Id" +
                " INNER JOIN AnnotationTaskUserTweet atu ON a.AnnotationTaskUserTweetId = atu.Id";


            return GetAll(size, page, "", "CreationDate Desc", query, true).ToList();
        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
        }
    }

    public class AnnotationReason : BaseIntModel
    {
        public int AnnotationId { get; set; }
        public int CategoryId { get; set; }
        public int DimensionId { get; set; }
        public int? StartWordId { get; set; }
        public int? EndWordId { get; set; }
        public int? StartWordPosition { get; set; }
        public int? EndWordPosition { get; set; }
        public List<AnnotationReasonWord> AnnotationReasonWords { get; set; }




        //For views
        public int TweetId { get; set; }
        public int UserId { get; set; }
    }

    public class AnnotationReasonWordRepository : BaseIntRepository<AnnotationReasonWord>
    {
        public AnnotationReasonWordRepository(int userId)
        {
            base.Init("AnnotationReasonWord_" + userId, "AnnotationReasonId,TweetWordId");
        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
        }
    }

    public class AnnotationReasonWord : BaseIntModel
    {
        public int AnnotationReasonId { get; set; }
        public int TweetWordId { get; set; }





    }
}
