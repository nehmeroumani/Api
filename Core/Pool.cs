using System;
using Core.Cache;
using Core.Repositories;
using Core.Repositories.Annotations;
using Core.Repositories.Twitter;
using Core.Twitter;

namespace Core
{


    public class Pool
    {
        public string ConnectionString { get; set; }
        public static readonly Pool I = new Pool();

        //SELECT ' 'public ' + TABLE_NAME + 'Repository ' + TABLE_NAME +'s { get; set; }' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'

        public CategoryRepository Categorys { get; set; }
        public TweetAccountRepository TweetAccounts { get; set; }
        public UserRepository Users { get; set; }
        public TweetRepository Tweets { get; set; }
        public AnnotationReasonRepository AnnotationReasons { get; set; }
        public AnnotationRepository Annotations { get; set; }
        public WordRepository Words { get; set; }
        public AnnotationTaskRepository AnnotationTasks { get; set; }
        public TwitterTagRepository Tags { get; set; }
        public TweetTagMapRepository TweetMapTags { get; set; }
        public TwitterTagRepository TwitterTags { get; set; }
        public DimensionRepository Dimensions { get; set; }
        public TweetWordRepository TweetWords { get; set; }
        public LevelOfConfidenceRepository LevelOfConfidences { get; set; }
        //public AnnotationTaskUserRepository AnnotationTaskUsers { get; set; }
        public AnnotationReasonWordRepository AnnotationReasonWords { get; set; }
        public AnnotationTaskUserTweetRepository AnnotationTaskUserTweets { get; set; }

        private Pool()
        {
            Init();
        }

        //SELECT  TABLE_NAME + 's = new ' + TABLE_NAME + 'Repository();' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'
        public void Init()
        {
            AnnotationTaskUserTweets = new AnnotationTaskUserTweetRepository();
            AnnotationReasonWords = new AnnotationReasonWordRepository();
            TweetAccounts = new TweetAccountRepository();
            //AnnotationTaskUsers = new AnnotationTaskUserRepository();
            LevelOfConfidences = new LevelOfConfidenceRepository();
            Words = new WordRepository();
            TweetWords = new TweetWordRepository();
            AnnotationReasons = new AnnotationReasonRepository();
            Annotations = new AnnotationRepository();
            AnnotationTasks = new AnnotationTaskRepository();
            Dimensions = new DimensionRepository();
            TwitterTags = new TwitterTagRepository();
            TweetMapTags = new TweetTagMapRepository();
            Tags = new TwitterTagRepository();
            Tweets = new TweetRepository();
            Users = new UserRepository();
            Categorys = new CategoryRepository();
        }
    }
}
