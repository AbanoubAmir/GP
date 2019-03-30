using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace OpinionMining.Classes
{
    public class PrototypeFunctions
    {
        private readonly string CK, CS, AT, ATS, UN, PAS, URL;
        private NaturalLanguageUnderstandingExample nlp;
        public List<Tuple<string, float?>> scores;
        public PrototypeFunctions()
        {
            CK = WebConfigurationManager.AppSettings["consumer_key"];
            CS = WebConfigurationManager.AppSettings["consumer_secret"];
            AT = WebConfigurationManager.AppSettings["access_token"];
            ATS = WebConfigurationManager.AppSettings["access_token_secret"];
            UN = WebConfigurationManager.AppSettings["username"];
            PAS = WebConfigurationManager.AppSettings["password"];
            URL = WebConfigurationManager.AppSettings["url"];
            ITwitterCredentials creds = new TwitterCredentials(CK, CS, AT, ATS);
            Auth.SetCredentials(creds);
            nlp = new NaturalLanguageUnderstandingExample(URL, UN, PAS);
            ExceptionHandler.SwallowWebExceptions = false;
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

        }
        public void GetNews(string Query)
        {
            scores = new List<Tuple<string, float?>>();
            var newsApiClient = new NewsApiClient("a96b258ae04044d79886855bb03003a1");
            var articlesResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
            {
                Q = Query,
                Language = Languages.AR,
                //From = new DateTime(2019, 2, 9)
            });
            if (articlesResponse.Status == Statuses.Ok)
            {
                foreach (var article in articlesResponse.Articles)
                {
                    float? x = nlp.Analyze(article.Url);
                    string y = article.Url;
                    scores.Add(new Tuple<string, float?>(y, x));
                    
                }
            }
            //return scores;
        }
        public List<Tuple<string,float ?>> FetchTweets(string Query)
        {
            scores = new List<Tuple<string, float?>>();
            try
            {
                var searchParameter = new SearchTweetsParameters(Query)
                {
                    Lang = LanguageFilter.English,
                    SearchType = SearchResultType.Popular,
                    MaximumNumberOfResults = 20,
                    //Filters = TweetSearchFilters.
                };
                var tweets = Search.SearchTweets(searchParameter);
                foreach (var tweet in tweets)
                {
                    float? x = nlp.Analyze(tweet.FullText);
                    string y = tweet.GenerateOEmbedTweet().HTML;
                    scores.Add(new Tuple<string, float?>(y, x));
                    //Console.WriteLine(Environment.NewLine + "Sentiment Score: " + x.ToString() + " " + label + Environment.NewLine + "(" + tweet.FullText + ")");
                }
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                scores.Add(new Tuple<string, float?>("Request parameters are invalid: '{0}'" + ex.Message,null));
                
            }
            catch (TwitterException ex)
            {
                // Twitter API Request has been failed; Bad request, network failure or unauthorized request
                scores.Add(new Tuple<string, float?>("Something went wrong when we tried to execute the http request : '{0}'"+ ex.TwitterDescription, null));

            }
            return scores;
        }
        private static string Sanitize(string raw)
        {
            return Regex.Replace(raw, @"(@[A-Za-z0-9]+)|([^0-9A-Za-z \t])|(\w+:\/\/\S+)", " ").ToString();
        }
        
    }
    
    public class NaturalLanguageUnderstandingExample
    {
        private NaturalLanguageUnderstandingService _naturalLanguageUnderstandingService;

        #region Constructor
        public NaturalLanguageUnderstandingExample(string url, string username, string password)
        {
            _naturalLanguageUnderstandingService = new NaturalLanguageUnderstandingService(username, password, "2018-03-19");
            _naturalLanguageUnderstandingService.SetEndpoint(url);
        }
        #endregion

        public float? Analyze(string _nluText)
        {
            Parameters parameters = new Parameters()
            {
                //Url = _nluText,
                Language= "en",
                Text=_nluText,
                Features = new Features()
                {
                    Sentiment = new SentimentOptions()
                    {
                        Document = true
                    }

                }
            };
            var result = _naturalLanguageUnderstandingService.Analyze(parameters);
            return result.Sentiment.Document.Score;
        }
    }
}