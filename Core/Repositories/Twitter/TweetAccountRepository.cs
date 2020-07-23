using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories.Twitter
{
    public class TweetAccountRepository : BaseRepository<TweetAccount>
    {
        public TweetAccountRepository()
        {
            base.Init("TweetAccount", "UserName,Photo,DisplayName,Url");
        }
        public IEnumerable<TweetAccount> GetStatistics(RequestData rd)
        {
            string Where = "";
            string JoinDictionary = "";
            var startDate = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
            var endDate = DateTime.Now.ToString("yyyy-MM-dd");
            var startDateFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "startdate");
            var endDateFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "enddate");
            if (startDateFilter != null)
                startDate = startDateFilter.Value;
            if (endDateFilter != null)
                endDate = endDateFilter.Value;

            var themeFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "themeid");
            if (themeFilter != null)
            {
                JoinDictionary = $" INNER JOIN Tweet tt on tt.TwitterAppId = app.Id INNER JOIN DictionaryLabel dl on dl.ItemId = tt.Id inner join Dictionary dd on dl.DictionaryId = dd.Id inner join ThemeCategory th on dd.ThemeCategoryId = th.Id  WHERE  th.ThemeId = '{themeFilter.Value}' ";
            }

            var twitterapp = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "twitterappid");
            if (twitterapp != null)
            {
                if (string.IsNullOrEmpty(JoinDictionary))
                    Where = $"Where id='{twitterapp.Value}' ";
                else
                {
                    JoinDictionary += $" AND id='{twitterapp.Value}' ";
                }
            }
            var query = @"SELECT     distinct   app.*,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.DictionaryLabel AS dl INNER JOIN
                                                         dbo.Tweet AS t ON t.Id = dl.ItemId
                               WHERE        (t.TwitterAppId = app.Id)) AS LabelsCount,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.Tweet AS t
                               WHERE        (TwitterAppId = app.Id) AND t.ThemeDetected=1) AS TweetsThemeCount
                            FROM            dbo.TwitterApp AS app " + JoinDictionary + " Order by LabelsCount desc";


            return Query<TweetAccount>(query);
        }
    }

    public class TweetAccount : BaseModel
    {
        public string UserName { get; set; }
        public string Photo { get; set; }
        public string Url { get; set; }
        public string DisplayName { get; set; }

        public int TotalTweets { get; set; }
    }

}
