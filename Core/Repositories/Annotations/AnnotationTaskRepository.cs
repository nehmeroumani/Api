using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories.Annotations
{
    public class AnnotationTaskRepository : BaseIntRepository<AnnotationTask>
    {
        public AnnotationTaskRepository()
        {
            base.Init("AnnotationTask", "StartTweetId,EndTweetId,CreatedByUserId,UserId,Status,StartTime,FinishTime,TaskDuration");
        }

        
    }

    public class AnnotationTask : BaseIntModel
    {
        //public string Name { get; set; }
        public int StartTweetId { get; set; }
        public int EndTweetId { get; set; }
        public int CreatedByUserId { get; set; }
        //public bool IsFinished { get; set; }
        public int UserId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public int TaskDuration { get; set; }
        public int TotalTweets { get; set; }
        public int DoneTweets { get; set; }
        public int Status { get; set; }


        public int TotalAnnotations { get; set; }
        // public int[] UserIds { get; set; }
    }

    //public class AnnotationTaskUserRepository : BaseIntRepository<AnnotationTaskUser>
    //{
    //    public AnnotationTaskUserRepository()
    //    {
    //        base.Init("AnnotationTaskUser", "UserId,AnnotationTaskId");
    //    }
    //}

    //public class AnnotationTaskUser : BaseIntModel
    //{
    //    public int UserId { get; set; }
    //    public int AnnotationTaskId { get; set; }
    //}


    public class AnnotationTaskUserTweetRepository : BaseIntRepository<AnnotationTaskUserTweet>
    {
        public AnnotationTaskUserTweetRepository()
        {
            base.Init("AnnotationTaskUserTweet", "TaskDuration,UserId,AnnotationTaskId,TweetId,Status,StartTime,FinishTime,IsIrrelevant,LevelOfConfidenceId");
        }

        public AnnotationTaskUserTweet GetNext(string userId)
        {
            return GetWhere($"UserId={userId} and (Status={(int)AnnotationTaskUserStatusEnum.InProgress} OR Status={(int)AnnotationTaskUserStatusEnum.New}) ").FirstOrDefault();
        }

      
        public List<AnnotationTaskUserTweet> GetStatisticsStatus(RequestData rd, out int total)
        {
            string Where = "WHERE atut.IsDeleted=0 ";

            var userFilter = rd.Filter.FirstOrDefault(x => x.Key.ToLower() == "userid");
         
            if (userFilter != null)
            {
                Where += $" AND UserId = '{userFilter.Value}' ";
            }

            var data = Query<AnnotationTaskUserTweet>("SELECT Status,avg(l.Value) as AvgLevelOfConfidence,avg(TaskDuration) as AvgTaskDuration,count(*) as TotalTasks , " +
                                                      "count(*) * 100.0 / (select count(*) from [AnnotationTaskUserTweet] WHERE IsDeleted=0 ) as percentage" +
                " ,sum(TaskDuration) as TotalTaskDuration   FROM [AnnotationTaskUserTweet] atut left join LevelOfConfidence l on l.Id = atut.LevelOfConfidenceId  " + Where + " GROUP BY Status ").ToList();
            

            total = data.Count;
            return data;
        }
    }   
    public class AnnotationTaskUserTweet : BaseIntModel
    {
        public bool? IsIrrelevant { get; set; }
        public int? LevelOfConfidenceId { get; set; }
       
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public int TaskDuration { get; set; }
        public int Status { get; set; }

        public AnnotationTaskUserStatusEnum AnnotationStatusEnum => (AnnotationTaskUserStatusEnum)Status;
        public int TweetId { get; set; }
        public int UserId { get; set; }
        public int AnnotationTaskId { get; set; }
       


        public string ConfidenceName { get; set; }
        public string UserName { get; set; }
        public string TweetText { get; set; }

        public int TotalTasks { get; set; }
        public int TotalTaskDuration { get; set; }
        public double Percentage { get; set; }
        public int AvgTaskDuration { get; set; }
        public int AvgLevelOfConfidence { get; set; }
        
        public List<AnnotationTaskUserTweet> StatusAgregates { get; set; }

        public List<Annotation> Annotations { get; set; }
        public int? NextAnnotationId { get; set; }
        
        public int TotalAnnotations { get; set; }

    }
    public enum AnnotationTaskUserStatusEnum
    {
        New = 10, InProgress = 20, Done = 30
    }
}
