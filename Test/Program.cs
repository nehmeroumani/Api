using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            var categories = new[] { 1, 2, 3, 4, 5 };
            var tweet = 1;

            var user1 = new User(1)
            {
                Annotations = new List<Annotation>()
                {
                    new Annotation(tweetId: 1, userId: 1, categoryId: 1),
                    new Annotation(tweetId: 1, userId: 1, categoryId: 3),
                }
            };

            var user2 = new User(2)
            {
                Annotations = new List<Annotation>()
                {
                    new Annotation(tweetId: 1, userId: 2, categoryId: 1),
                    new Annotation(tweetId: 1, userId: 2, categoryId: 4),
                }
            };


                int positivePositive = 0, positiveNegative = 0, negativePositive = 0, negativeNegative = 0;

            //var tc = new TweetCategory();
            //tc.CategoryResults1 = new Dictionary<int, int>();
            //tc.CategoryResults2 = new Dictionary<int, int>();
            //tc.CategoryResults1[t] = a1 != null ? 1 : 0;
            foreach (var t in categories)
            {
                var a1 = user1.Annotations.SingleOrDefault(x => x.TweetId == tweet && x.CategoryId == t);
                var tc1 = a1 != null ? 1 : 0;
                var a2 = user2.Annotations.SingleOrDefault(x => x.TweetId == tweet && x.CategoryId == t);
                var tc2 = a2 != null ? 1 : 0;

                if (tc1 == 1 && tc2 == 1)
                    positivePositive++;
                else if (tc1 == 1 && tc2 == 0)
                    positiveNegative++;
                else if (tc1 == 0 && tc2 == 1)
                    negativePositive++;
                else if (tc1 == 0 && tc2 == 0)
                    negativeNegative++;
            }


            Console.WriteLine("positive_positive: " + positivePositive);
            Console.WriteLine("positiveNegative: " + positiveNegative);
            Console.WriteLine("negativePositive: " + negativePositive);
            Console.WriteLine("negativeNegative: " + negativeNegative);
        }
    }
    //class TweetCategory
    //{
    //    public int TweetId { get; set; }
    //    public Dictionary<int, int> CategoryResults1 { get; set; }
    //    public Dictionary<int, int> CategoryResults2 { get; set; }
    //}
    class User
    {
        public int Id { get; set; }
        public User(int id)
        {
            this.Id = id;
        }
        public List<Annotation> Annotations { get; set; }
    }
    class Annotation
    {
        public Annotation(int tweetId, int userId, int categoryId)
        {
            this.TweetId = tweetId;
            this.UserId = userId;
            this.CategoryId = categoryId;
        }
        public int TweetId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int DimensionId { get; set; }
    }
}
