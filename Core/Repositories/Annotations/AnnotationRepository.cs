using System;
using System.Collections.Generic;

namespace Core.Repositories.Annotations
{
    public class AnnotationRepository : BaseIntRepository<Annotation>
    {
        public AnnotationRepository()
        {
            base.Init("Annotation", "AnnotationTaskUserTweetId,CategoryId,DimensionId,AnnotationTaskId");
        }
    }

    public class Annotation : BaseIntModel
    {
        public int AnnotationTaskUserTweetId { get; set; }
       
        public int? CategoryId { get; set; }
        public int? DimensionId { get; set; }
        public List<AnnotationReason> AnnotationReasons { get; set; }

        public int AnnotationTaskId { get; set; }


        //For views
        public int TweetId { get; set; }
        public int UserId { get; set; }

    }
}
