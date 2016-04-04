using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Extend;
using Newtonsoft.Json;
using StackOverflowApi.Models;

namespace StackOverflowApi
{
    public class Program
    {
        static int userID = 0;
        static List<string> comments = new List<string>();
        static List<string> answers = new List<string>();

        private static readonly DateTime UnixEpoch =
new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter User ID (EX: 1048571)");
                Console.WriteLine("==================");
                userID = Convert.ToInt32(Console.ReadLine());
                var task1 = Task.Run(() => SetAnswers());
                var task2 = Task.Run(() => SetComments());
                Task.WaitAll(task1, task2);
                Task.Run(() => PrintReport(answers, comments));
                Console.ReadLine();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        static async Task SetAnswers()
        {
            string endpoint = @"https://api.stackexchange.com/2.2/users/{0}/answers?{1}";

            DateTime dayOne, dayTwo;
            long dayOneUnix, dayTwoUnix;

            dayOne = DateTimeEx.StartOfWeek(DateTime.Now.AddDays(-7));
            dayTwo = DateTimeEx.EndOfWeek(DateTime.Now.AddDays(-7));

            dayOneUnix = GetCurrentUnixTimestampSeconds(dayOne);
            dayTwoUnix = GetCurrentUnixTimestampSeconds(dayTwo);

            string parameters;
            parameters = "order=desc&sort=activity&site=stackoverflow&fromdate={0}&todate={1}";
            parameters = string.Format(parameters, dayOneUnix, dayTwoUnix);

            endpoint = string.Format(endpoint, userID, parameters);

            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            try
            {
                var response = await client.GetAsync(endpoint);
                string json;
                if (response.IsSuccessStatusCode)
                {
                    using (var s = await response.Content.ReadAsStreamAsync())
                    {
                        using (var decompressed = new GZipStream(s, CompressionMode.Decompress))
                        {
                            using (var rdr = new StreamReader(decompressed))
                            {
                                json = await rdr.ReadToEndAsync();
                            }
                        }
                    }
                    var answerObject = JsonConvert.DeserializeObject<RootAnswer>(json);
                    answers = await GetListOfStackOverflowAnswers(answerObject);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                throw;
            }

        }

        static async Task SetComments()
        {
            string endpoint = @"https://api.stackexchange.com/2.2/users/{0}/comments?{1}";
            ;

            DateTime dayOne, dayTwo;
            long dayOneUnix, dayTwoUnix;

            dayOne = DateTimeEx.StartOfWeek(DateTime.Now.AddDays(-7));
            dayTwo = DateTimeEx.EndOfWeek(DateTime.Now.AddDays(-7));

            dayOneUnix = GetCurrentUnixTimestampSeconds(dayOne);
            dayTwoUnix = GetCurrentUnixTimestampSeconds(dayTwo);

            string parameters;
            parameters = "fromdate={0}&todate={1}&order=desc&sort=creation&site=stackoverflow";
            parameters = string.Format(parameters, dayOneUnix, dayTwoUnix);
            endpoint = string.Format(endpoint, userID, parameters);

            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            try
            {
                var response = await client.GetAsync(endpoint);
                string json;
                if (response.IsSuccessStatusCode)
                {
                    using (var s = await response.Content.ReadAsStreamAsync())
                    {
                        using (var decompressed = new GZipStream(s, CompressionMode.Decompress))
                        {
                            using (var rdr = new StreamReader(decompressed))
                            {
                                json = await rdr.ReadToEndAsync();
                            }
                        }
                    }
                    var commentObject = JsonConvert.DeserializeObject<RootComment>(json);
                    comments = await GetListOfStackOverflowComments(commentObject);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                throw;
            }
        }

        public static async Task<List<string>> GetListOfStackOverflowAnswers(RootAnswer root)
        {
            List<string> ReportLinks = new List<string>();
            foreach (var a in root.answers)
            {
                ReportLinks.Add(GetLinkForAnswer(a.answer_id));
            }
            return ReportLinks;
        }

        public static async Task<List<string>> GetListOfStackOverflowComments(RootComment root)
        {
            List<string> ReportLinks = new List<string>();
            foreach (var c in root.comments)
            {
                ReportLinks.Add(GetLinkForComment(c.post_id, c.comment_id));
            }
            return ReportLinks;
        }

        public static async Task PrintReport(List<string> answers, List<string> comments)
        {
            try
            {
                string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                docPath = Path.Combine(docPath, $@"A&O {userID} {DateTime.Now:yyyy-MM-dd}.txt");
                using (
                    StreamWriter outputFile = new StreamWriter(docPath)
                    )
                {
                    await outputFile.WriteLineAsync("Answers");
                    await outputFile.WriteLineAsync("==================");
                    foreach (var a in answers)
                    {
                        await outputFile.WriteLineAsync(a);
                    }
                    await outputFile.WriteLineAsync(Environment.NewLine);

                    await outputFile.WriteLineAsync("Comments");
                    await outputFile.WriteLineAsync("==================");
                    foreach (var c in comments)
                    {
                        await outputFile.WriteLineAsync(c);
                    }
                    await outputFile.WriteLineAsync(Environment.NewLine);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static long GetCurrentUnixTimestampSeconds(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        public static string GetLinkForAnswer(int answerId)
        {
            //http://stackoverflow.com/a/36361166
            var url = "http://stackoverflow.com/a/{0}";
            url = string.Format(url, answerId);
            return url;
        }

        public static string GetLinkForComment(int postId, int commentId)
        {
            //http://stackoverflow.com/q/36379031#comment60401451_36379031
            var url = "http://stackoverflow.com/q/{0}#comment{1}_{0}";
            url = string.Format(url, postId, commentId);
            return url;
        }
    }
}
