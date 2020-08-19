using System;

namespace Core.Repositories
{
    public class WordRepository : BaseIntRepository<Word>
    {
        public WordRepository()
        {
            base.Init("Word", "Name,Language");
        }
    }

    public class Word : BaseIntModel
    {
        public string Name { get; set; }
        public string Language { get; set; }
    }

    public class TweetWordRepository : BaseIntRepository<TweetWord>
    {
        public TweetWordRepository()
        {
            base.Init("TweetWord", "TweetId,WordId,Position");
        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
        }
    }

    public class TweetWord : BaseIntModel
    {
        public int TweetId { get; set; }
        public int WordId { get; set; }
        public int Position { get; set; }

        //used instead of join
        public string WordName { get; set; }
    }
}
