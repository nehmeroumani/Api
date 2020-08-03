using Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Core.Repositories;
using Core.Repositories.Twitter;


namespace Import
{
    class Program
    {
        public static IConfigurationSection settings;

        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            settings = configuration.GetSection("AppSettings");

            Pool.I.ConnectionString = settings["Connection"];


           // Pool.I.Users.CreateNewUserTables(new User() { Id = 2 });
            ImortTweets();

        }

        private static void ImortTweets()
        {
            var tweets = Pool.I.Tweets.GetAll().ToList();
            var words = Pool.I.Words.GetAll().ToList();
            var index = 0;
            char[] delimiterChars = { ' ', ',', '،', '.', ':', '\t' };
            foreach (var t in tweets)
            {
                Console.WriteLine(index++);
                var text = t.Text;

                //var text = RemoveDiacritics(t.Text);

                //var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
                //var split = text.CleanTweetUrLs().CleanUsername()
                //    .Replace("@", " ")
                //    .Replace(".", " ")
                //    .Replace("#", " ")
                //    .Replace("?", " ")
                //    .Replace(",", " ")
                //    .Replace("،", " ")
                //    .Replace(":", " ")
                //    .Replace("!", " ")
                //    .Replace("_", " ")
                //    .Replace("\n", "")
                //    .Replace("\r", "")
                //    .Replace("\t", "")
                //    .Replace("  ", " ")
                //    .RemoveEmojies()
                //    .RemoveNumbers().Trim()
                //    .Split().Select(x => x.Trim(punctuation));

                var split = text.Split(delimiterChars);

                var tweetWords = new List<TweetWord>();
                int i = 1;
                foreach (var s in split)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        Word word = words.SingleOrDefault(x => x.Name == s);

                        if (word == null)
                        {
                            word = new Word() { Name = s };
                            Pool.I.Words.Save(word);
                            words.Add(word);
                            Console.WriteLine(s);
                        }

                        tweetWords.Add(new TweetWord()
                        {
                            CreationDate = DateTime.Now,
                            LastModified = DateTime.Now,
                            TweetId = t.Id,
                            WordId = word.Id,
                            Position = i++
                        });
                    }
                }

                Pool.I.TweetWords.Insert(tweetWords);
            }
        }
      
        public static string RemoveDiacritics(string text)
        {

            if (text != null)
            {


                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }
            return "";
        }
    }
}
