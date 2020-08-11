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
using NReco.Csv;
using Core.Repositories.Twitter;


namespace Import
{
    class Program
    {
        public static IConfigurationSection settings;
        private static List<Word> savedWords;
        private static List<Tweet> savedTweets;

        static void Main(string[] args)
        {
            Console.WriteLine("Tweets importer");
            Console.WriteLine("It's a command line tool that affords importing and tokenizing tweets with less hussle.");
            Console.WriteLine("The required CSV file must be prepared based on the following structure:");
            Console.WriteLine("Tweet ID | Text | Account Display Name | User ID | Date");
            Console.WriteLine("Please type the path of the tweets file you want to import:");
            string tweetsFilePath = Console.ReadLine();

            Init();
            ReadSavedData();
            ImportTweets(tweetsFilePath.Trim());
        }

        private static void Init()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            settings = configuration.GetSection("AppSettings");

            Pool.I.ConnectionString = settings["Connection"];
        }

        private static void ReadSavedData()
        {
            Console.WriteLine("Reading saved words from database...");
            savedWords = Pool.I.Words.GetAll().ToList();
            Console.WriteLine("Reading saved tweets from database...");
            savedTweets = Pool.I.Tweets.GetAll().ToList();
        }

        private static void ImportTweets(string tweetsFilePath)
        {
            Console.Write("Reading tweets from csv file...");
            var importedTweets = ReadTweetsFromCsvFile(tweetsFilePath);
            Console.WriteLine($" {importedTweets.Count()} tweets");

            Console.Write("Removing duplicates...");
            importedTweets = RemoveDuplicates(importedTweets);
            Console.WriteLine($" {importedTweets.Count()} tweets");

            if (importedTweets.Count() > 0) {

                var newTweets = new List<Tweet>();
                //Console.Write("Selecting new tweets only...");
                //newTweets = SelectNewTweetsFrom(importedTweets);
                //Console.WriteLine($" {newTweets.Count()} new tweets");

                List<Tweet> tweets;
                if (newTweets.Count() > 0)
                {
                    Console.WriteLine("Inserting new tweets into the database...");
                    Pool.I.Tweets.Insert(newTweets);

                    Console.Write("Fetching new inserted tweets from the database...");
                    tweets = Pool.I.Tweets.GetWhere("TweetId IN (" + string.Join(",", newTweets.Select((t)=>$"{t.TweetId}").ToList())+ ")");
                    Console.WriteLine($" {tweets.Count()} new inserted tweets");
                }
                else
                {
                    Console.Write("Fetching tweets from the database...");
                    tweets = Pool.I.Tweets.GetWhere("TweetId IN (" + string.Join(",", importedTweets.Select((t) => $"{t.TweetId}").ToList()) + ")");
                    Console.WriteLine($" {tweets.Count()} tweets");
                }

                Console.WriteLine("Tokenizing new tweets...");
                var tweetTokens = TokenizeTweets(tweets);
                Console.WriteLine("Saving tokens of new tweets into database...");
                Pool.I.TweetWords.Insert(tweetTokens);
            }
        }

        private static List<Tweet> ReadTweetsFromCsvFile(string tweetsFilePath)
        {
            var tweets = new List<Tweet>();
            if (!string.IsNullOrEmpty(tweetsFilePath))
            {
                //CSV file template
                //Tweet ID | Text | Account Display Name | User ID | Date
                using (var reader = new StreamReader(tweetsFilePath))
                {
                    var rowNum = 1;
                    var csvReader = new CsvReader(reader, ",");
                    while (csvReader.Read())
                    {
                        if (rowNum > 1)
                        {
                            var tweet = new Tweet()
                            {
                                CreationDate = DateTime.Now,
                                LastModified = DateTime.Now,
                            };
                            for (int colIndex = 0; colIndex < csvReader.FieldsCount; colIndex++)
                            {

                                string cellVal = csvReader[colIndex];

                                switch (colIndex)
                                {
                                    case 0:
                                        tweet.TweetId = long.Parse(cellVal.Trim('#'));
                                        break;
                                    case 1:
                                        tweet.Text = cellVal;
                                        break;
                                    case 2:
                                        tweet.AccountDisplayName = cellVal;
                                        break;
                                    case 3:
                                        tweet.UserID = long.Parse(cellVal.Trim('#'));
                                        break;
                                    case 4:
                                        tweet.TweetCreationDate = Convert.ToDateTime(cellVal);
                                        break;
                                }
                            }
                            tweets.Add(tweet);
                        }
                        rowNum++;
                    }
                }
            }
            return tweets;
        }

        private static List<Tweet> RemoveDuplicates(List<Tweet> tweets)
        {
            if (tweets != null && tweets.Count > 0)
            {
                List<Tweet> uniqueTweets = new List<Tweet>();
                foreach(Tweet tweet in tweets)
                {
                    if (uniqueTweets.SingleOrDefault((t) => t.TweetId == tweet.TweetId) == null)
                    {
                        uniqueTweets.Add(tweet);
                    }
                }
                return uniqueTweets;
            }
            return tweets;
        }

        private static List<Tweet> SelectNewTweetsFrom(List<Tweet> tweets)
        {
            if (tweets != null && tweets.Count > 0)
            {
                tweets = tweets.Where((t) => savedTweets.SingleOrDefault((tt) => t.TweetId == tt.TweetId) == null).ToList();
            }
            return tweets;
        }

        private static List<TweetWord> TokenizeTweets(List<Tweet> tweets)
        {
            var tweetWords = new List<TweetWord>();
            foreach (var tweet in tweets)
            {
                tweetWords.AddRange(TokenizeTweet(tweet));
            }
            return tweetWords;
        }

        private static List<TweetWord> TokenizeTweet(Tweet tweet)
        {
            var tweetWords = new List<TweetWord>();
            if (tweet != null)
            {
                var textTokens = TokenizeTweetText(tweet.Text);
                int i = 1;
                foreach (var tt in textTokens)
                {
                    if (!string.IsNullOrEmpty(tt))
                    {
                        var word = FindWordOrCreateIt(tt);

                        tweetWords.Add(new TweetWord()
                        {
                            CreationDate = DateTime.Now,
                            LastModified = DateTime.Now,
                            TweetId = tweet.Id,
                            WordId = word.Id,
                            Position = i++
                        });
                    }
                }
            }
            return tweetWords;
        }

        private static List<string> TokenizeTweetText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var textWithoutDiacritics = RemoveDiacritics(text);
                var punctuation = textWithoutDiacritics.Where(Char.IsPunctuation).Distinct().ToArray();
                return textWithoutDiacritics.CleanTweetUrLs().CleanUsername()
                    .Replace("@", " ")
                    .Replace(".", " ")
                    .Replace("#", " ")
                    .Replace("?", " ")
                    .Replace(",", " ")
                    .Replace("،", " ")
                    .Replace(":", " ")
                    .Replace("!", " ")
                    .Replace("_", " ")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("\t", "")
                    .Replace("  ", " ")
                    .RemoveEmojies()
                    .RemoveNumbers().Trim()
                    .Split(" ").Select(x => x.Trim(punctuation)).ToList();
            }
            return new List<string>();
        }

        private static string RemoveDiacritics(string text)
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

        private static Word FindWordOrCreateIt(string s)
        {
            Word word = savedWords.SingleOrDefault(w => w.Name == s);

            if (word == null)
            {
                word = new Word() { Name = s };
                Pool.I.Words.Save(word);
                savedWords.Add(word);
            }

            return word;
        }
    }
}
