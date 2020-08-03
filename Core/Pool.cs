using System;
using System.Collections.Generic;
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

        public WordRepository Words { get; set; }
        public AnnotationTaskRepository AnnotationTasks { get; set; }
        public TwitterTagRepository Tags { get; set; }
        public TweetTagMapRepository TweetMapTags { get; set; }
        public TwitterTagRepository TwitterTags { get; set; }
        public DimensionRepository Dimensions { get; set; }
        public TweetWordRepository TweetWords { get; set; }
        public LevelOfConfidenceRepository LevelOfConfidences { get; set; }
        //public AnnotationTaskUserRepository AnnotationTaskUsers { get; set; }
        public AnnotationTaskUserTweetRepository AnnotationTaskUserTweets { get; set; }


        public Dictionary<int, AnnotationRepository> Annotations { get; set; }
        public Dictionary<int, AnnotationReasonRepository> AnnotationReasons { get; set; }
        public Dictionary<int, AnnotationReasonWordRepository> AnnotationReasonWords { get; set; }


        private Pool()
        {
            Init();
        }

        //SELECT  TABLE_NAME + 's = new ' + TABLE_NAME + 'Repository();' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'
        public void Init()
        {
            Users = new UserRepository();
            AnnotationTaskUserTweets = new AnnotationTaskUserTweetRepository();
            TweetAccounts = new TweetAccountRepository();
            //AnnotationTaskUsers = new AnnotationTaskUserRepository();
            LevelOfConfidences = new LevelOfConfidenceRepository();
            Words = new WordRepository();
            TweetWords = new TweetWordRepository();
            AnnotationTasks = new AnnotationTaskRepository();
            Dimensions = new DimensionRepository();
            TwitterTags = new TwitterTagRepository();
            TweetMapTags = new TweetTagMapRepository();
            Tags = new TwitterTagRepository();
            Tweets = new TweetRepository();
            Categorys = new CategoryRepository();
        }

        public void InitAnnotationRepositorys()
        {
            var allUsers = Users.GetAll();

            Annotations = new Dictionary<int, AnnotationRepository>();
            AnnotationReasons = new Dictionary<int, AnnotationReasonRepository>();
            AnnotationReasonWords = new Dictionary<int, AnnotationReasonWordRepository>();

            foreach (var user in allUsers)
            {
                Annotations[user.Id] = new AnnotationRepository(user.Id);
                AnnotationReasons[user.Id] = new AnnotationReasonRepository(user.Id);
                AnnotationReasonWords[user.Id] = new AnnotationReasonWordRepository(user.Id);
            }
        }
    }
}
