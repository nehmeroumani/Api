using System;
using Core.Repositories;

namespace Core.Twitter
{
    public class TweetTagMapRepository : BaseRepository<TweetTagMap>
    {
        public TweetTagMapRepository()
        {
            base.Init("TweetTagMap", "TweetId, TagId");
        }

    }


    public class TweetTagMap : BaseModel
    {
        public string TweetId { get; set; }
        public string TagId { get; set; }      
    }



}
