using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwitterObserver
{
    static class TwitterFirehose
    {


        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
        }

        public static CancellationTokenSource StartStream(Action<Task<Tweet>> onTweet)
        {
            //ignore this, I know it's ugly.
            var source = new CancellationTokenSource();
            TaskScheduler.UnobservedTaskException += (sender, excArgs) => excArgs.SetObserved();
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);
            Task.Factory.StartNew(() =>
                                  {
                                      var request =
                                          WebRequest.Create(
                                              "http://stream.twitter.com/1/statuses/sample.json?delimited=length") as
                                          HttpWebRequest;
                                      if (request != null)
                                      {
                                          var credentials = new NetworkCredential { UserName = "CAOTW", Password = "shoehorn" };
                                          request.Credentials = credentials;
                                          var response = request.GetResponse() as HttpWebResponse;
                                          if (response != null)
                                          {
                                              var responseStream = response.GetResponseStream();
                                              if (responseStream != null)
                                              {
                                                  var streamReader = new StreamReader(responseStream);
                                                  int statusLength = 0;
                                                  int currentIndex = 0;
                                                  while (!source.IsCancellationRequested)
                                                  {
                                                      if (statusLength == 0)
                                                      {
                                                          string temp = streamReader.ReadLine();
                                                          if (string.IsNullOrEmpty(temp) ||
                                                              !int.TryParse(temp, out statusLength))
                                                          {
                                                              statusLength = 0;
                                                          }
                                                      }
                                                      var buffer = new char[statusLength];
                                                      int charsRead = streamReader.ReadBlock(buffer, currentIndex,
                                                                                             statusLength);
                                                      currentIndex += charsRead;
                                                      statusLength -= charsRead;
                                                      if (statusLength <= 0)
                                                      {
                                                          var task = new Task<Tweet>(ParseTweet, new String(buffer),
                                                                                     CancellationToken.None);
                                                          task.ContinueWith(onTweet, CancellationToken.None,
                                                                            TaskContinuationOptions.NotOnFaulted,
                                                                            TaskScheduler.Default);
                                                          task.Start();
                                                          statusLength = 0;
                                                          currentIndex = 0;
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  });
            return source;
        }


        private static Tweet ParseTweet(object input)
        {
            var str = input as string;
            if (!string.IsNullOrEmpty(str))
            {
                return JsonConvert.DeserializeObject<Tweet>(str);
            }

            throw new JsonReaderException("Unable to parse tweet from string");
        }

    }

    public class Tweet
    {
        public bool Favorited { get; set; }
        public string Created_At { get; set; }
        public string Text { get; set; }
        public User User { get; set; }
    }

    public class User
    {
        public string Name { get; set; }
    }
}
