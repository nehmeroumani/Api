using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Twitter;

namespace Core.Repositories.Twitter
{
    public class TweetRepository : BaseIntRepository<Tweet>
    {
        public TweetRepository()
        {
            base.Init("Tweet", "TweetCreationDate,Text,AccountDisplayName,TweetId");
        }

        public IEnumerable<Tweet> GetStatistics(RequestData rd, out int total)
        {
            string Where = "WHERE t.IsDeleted=0 AND t.ThemeDetected=1 ";
            var themeFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "themeid");


            var dictionaryFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "dictionaryid");
            var accountFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "twitterappid");
            var startDate = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
            var endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var startDateFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "startdate");
            var endDateFilter = rd.Filter.SingleOrDefault(x => x.Key.ToLower() == "enddate");
            if (startDateFilter != null)
                startDate = startDateFilter.Value;
            if (endDateFilter != null)
                endDate = endDateFilter.Value;

            Where += $" AND t.TweetCreationDate>='{startDate}'";
            Where += $" AND t.TweetCreationDate<='{endDate}'";


            var table = "dbo.Tweet t";

            table += $" INNER JOIN dbo.DictionaryLabel AS dl ON t.Id = dl.ItemId  " +
                $"INNER JOIN Dictionary d on d.Id=dl.dictionaryid INNER JOIN ThemeCategory tc on tc.Id = d.ThemeCategoryId ";

            if (themeFilter != null)
            {
                Where += $" AND tc.ThemeId = '{themeFilter.Value}' ";
            }

            if (dictionaryFilter != null)
            {
                Where += $" AND dl.dictionaryid='{dictionaryFilter.Value}'";

            }

            if (accountFilter != null)
                Where += $" AND t.twitterappid='{accountFilter.Value}'";

            total = ExecuteScaler("Select count(distinct t.Id) from   " + table + " " + Where);
            var dates = Query<Tweet>("SELECT CONVERT(VARCHAR(10), TweetCreationDate, 111) AS Date,COUNT(distinct t.Id) as Count FROM  " + table + " " + Where +
                " GROUP BY CONVERT(VARCHAR(10), TweetCreationDate, 111) ORDER BY DATE ASC");

            int i = 0;
            //foreach (DateTime day in EachDay(DateTime.Parse(startDate), DateTime.Parse(endDate)))
            DateTime? oldDate = null;

            var lst = new List<Tweet>();
            foreach (var t in dates)
            {

                if (!oldDate.HasValue)
                {
                    oldDate = DateTime.Parse(t.Date);
                }

                var diff = DateTime.Parse(t.Date).Subtract(oldDate.Value).Days - 1;

                for (int j = 0; j < diff; j++)
                {
                    lst.Add(new Tweet()
                    {
                        Count = 0,
                        Date = oldDate.Value.AddDays((j + 1)).ToString("yyyy/MM/dd")
                    });
                }
                lst.Add(t);
                oldDate = DateTime.Parse(t.Date);
                i++;
            }
            return lst;
        }

        public List<Tweet> GetRange(int startTweetId, int endTweetId)
        {
            return GetWhere($" Id >= {startTweetId} and Id<={endTweetId}");
        }

        public List<Tweet> GetAllView(RequestData rd, out int total)
        {
            var query = @"SELECT        Id, CreationDate, LastModified, IsDeleted, TweetCreationDate, Text, AccountDisplayName,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.AnnotationTaskUserTweet
                               WHERE        (IsDeleted = 0) AND (t.Id = TweetId)) AS AssignedInTasks
                            FROM            dbo.Tweet AS t";

            return GetAll(rd, out total, query, true).ToList();
        }
    }
    public static class TweetParser
    {
        public static string Link(this string s, string url)
        {
            return string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", url, s);
        }

        public static string CleanTweetUrLs(this string s)
        {
            //return Regex.Replace(s, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/\S\w[\w- ;,./?%&=]\S*)?", new MatchEvaluator(URL));
            return Regex.Replace(s, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/\S\w[\w- ;,./?%&=]\S*)?", "");
        }
        public static string RemoveEmojies(this string raw)
        {
            return Regex.Replace(raw, @"\p{Cs}", ""); //Regex.Replace(raw, @"(@[A-Za-z0-9]+)|([^0-9A-Za-z \t])|(\w+:\/\/\S+)", " ").ToString();
        }
        public static string RemoveNumbers(this string raw)
        {
            return Regex.Replace(raw, @"[\d-]", string.Empty);
        }

        public static string CleanUsername(this string s)
        {
            //return Regex.Replace(s, "(@)((?:[A-Za-z0-9-_]*))", new MatchEvaluator(Username));
            return Regex.Replace(s, "(@)((?:[A-Za-z0-9-_]*))", "");
        }
        public static string ParseHashtag(this string s)
        {
            return Regex.Replace(s, "(#)((?:[A-Za-z0-9-_]*))", new MatchEvaluator(Hashtag));
        }
        private static string Hashtag(Match m)
        {
            string x = m.ToString();
            string tag = x.Replace("#", "%23");
            return x.Link("http://search.twitter.com/search?q=" + tag);
        }
        private static string Username(Match m)
        {
            string x = m.ToString();
            string username = x.Replace("@", "");
            return x.Link("http://twitter.com/" + username);
        }
        private static string URL(Match m)
        {
            string x = m.ToString();
            return x.Link(x);
        }
    }
    public class TweetStatistics
    {

    }

    public class Tweet : BaseIntModel
    {


        public string Text { get; set; }

        public long TweetId { get; set; }

        public long UserID { get; set; }

        public bool Assigned { get; set; }

        public bool AssignedCount { get; set; }

        public DateTime TweetCreationDate { get; set; }

        public string TwitterAppId { get; set; }

        public string AccountDisplayName { get; set; }


        public TweetAccount TweetAccount { get; set; }

        public int[] WordIds { get; set; }

        public List<TweetWord> TweetWords { get; set; }

        public string Date { get; set; }

        public int Count { get; set; }

        public int AssignedInTasks { get; set; }

    }



}
