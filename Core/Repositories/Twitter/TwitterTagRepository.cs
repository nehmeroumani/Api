using System.Collections.Generic;

namespace Core.Repositories.Twitter
{
    public class TwitterTagRepository : BaseRepository<TwitterTag>
    {
        public TwitterTagRepository()
        {
            base.Init("TwitterTag", "Name");
        }

        public IEnumerable<TwitterTag> GetTopTags()
        {
            return Query<TwitterTag>("SELECT t.Text ,Count(*) as Count  FROM TwitterTag as t " +
                                     "inner join TweetTagMap tg " +
                                     "on t.Id = tg.TagId " +
                                     "group by t.Text " +
                                     "order by Count desc " +
                                     "limit 20");
        }
    }


    public class TwitterTag : BaseModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Tweets { get; set; }

        public int Evaluation { get; set; }
    }

}
