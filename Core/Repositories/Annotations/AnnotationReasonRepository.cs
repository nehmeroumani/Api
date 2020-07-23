using System;
using System.Collections.Generic;

namespace Core.Repositories.Annotations
{
    public class AnnotationReasonRepository : BaseIntRepository<AnnotationReason>
    {
        public AnnotationReasonRepository()
        {
            base.Init("AnnotationReason", "AnnotationId,CategoryId,DimensionId,StartWordId,EndWordId,StartWordPosition,EndWordPosition"); 
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
    }

    public class AnnotationReasonWordRepository : BaseIntRepository<AnnotationReasonWord>
    {
        public AnnotationReasonWordRepository()
        {
            base.Init("AnnotationReasonWord", "AnnotationReasonId,TweetWordId");
        }
    }

    public class AnnotationReasonWord : BaseIntModel
    {
        public int AnnotationReasonId { get; set; }
        public int TweetWordId { get; set; }

    }
}
