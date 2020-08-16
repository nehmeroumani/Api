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


        public List<AnnotationTask> GetAllView(RequestData rd, out int total)
        {
            var query = @"SELECT        Id, CreationDate, LastModified, IsDeleted, Name, StartTweetId, EndTweetId, CreatedByUserId,
                                IsFinished, UserId, Status, StartTime, FinishTime, TaskDuration,                            
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.AnnotationTaskUserTweet
                               WHERE        (AnnotationTaskId = a.Id) AND (IsDeleted = 0)) AS TotalTweets,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.AnnotationTaskUserTweet AS AnnotationTaskUserTweet_1
                               WHERE        (AnnotationTaskId = a.Id) AND (IsDeleted = 0) AND (Status = 30)) AS DoneTweets
                                FROM            dbo.AnnotationTask AS a";

            return GetAll(rd, out total, query, true).ToList();
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

        public int GetAvgLevelOfConfidence(int userId)
        {
            string Where = $"WHERE atut.IsDeleted=0 AND atut.Status=30 AND UserId = '{userId}' ";
            
            var data = Query<AnnotationTaskUserTweet>("SELECT avg(l.Value) as AvgLevelOfConfidence " +
                "FROM [AnnotationTaskUserTweet] atut left join LevelOfConfidence l on l.Id = atut.LevelOfConfidenceId  " + Where + " GROUP BY Status ").ToList();

            if (data.Count> 0)
            {
                return data[0].AvgLevelOfConfidence;
            }
            return -1;
        }


        public string GetViewQuery()
        {
            return
                @"SELECT        atu.Id, atu.CreationDate, atu.LastModified, atu.IsDeleted, atu.TweetId, atu.UserId, atu.AnnotationTaskId, atu.Status, dbo.[User].Name AS UserName, dbo.Tweet.Text AS TweetText, atu.StartTime, atu.FinishTime, 
                         atu.LevelOfConfidenceId, atu.TaskDuration, atu.IsIrrelevant, dbo.LevelOfConfidence.Name AS ConfidenceName,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.Annotation
                               WHERE        (AnnotationTaskUserTweetId = atu.Id) AND (IsDeleted = 0)) AS TotalAnnotations
                            FROM            dbo.AnnotationTaskUserTweet AS atu INNER JOIN
                         dbo.[User] ON atu.UserId = dbo.[User].Id INNER JOIN
                         dbo.Tweet ON atu.TweetId = dbo.Tweet.Id LEFT OUTER JOIN
                         dbo.LevelOfConfidence ON atu.LevelOfConfidenceId = dbo.LevelOfConfidence.Id";
        }

        public List<AnnotationTaskUserTweet> GetAllView(RequestData rd, out int total)
        {
            return GetAll(rd, out total, GetViewQuery(), true).ToList();
        }
        public List<AnnotationTaskUserTweet> GetAllView(string where,int size,int page)
        {
            return GetAll(size,page,where,"CreationDate Desc", GetViewQuery(),true).ToList();
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
