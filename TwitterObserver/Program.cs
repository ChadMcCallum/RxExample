using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;


namespace TwitterObserver
{
    class Program
    {
        static void Main()
        {
            //var disposable = observable.Subscribe(observer);

            //Console.ReadLine();

            //var observable = Observable.Range(1, 10);
            //var observer = Observer.Create<int>(
            //    (i) =>
            //        {
            //            Console.WriteLine(i);
            //            Thread.Sleep(1000);
            //        }
            //    );

            //var observerToo = Observer.Create<int>(
            //    (i) => { Console.WriteLine("I wanna execute!"); });

            //observable.ObserveOn(Scheduler.NewThread).SubscribeOn(Scheduler.CurrentThread).Subscribe(observer);
            //observable.Subscribe(observerToo);

            //var longObserver = Observer.Create<long>(Console.WriteLine);
            //var observableTimer = Observable.Interval(TimeSpan.FromSeconds(2));
            //observableTimer.Subscribe(longObserver);

            //WebRequest request = WebRequest.Create("http://www.rtigger.com");
            //var requestObservable = Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse);

            //requestObservable.Invoke().Subscribe(
            //    Observer.Create<WebResponse>(
            //        (response) => Console.WriteLine("Downloaded " + response.ContentLength + " bytes")));

            //IObservable<Tweet> observable = new TwitterObservable();
            ////IObservable<Tweet> filtered = observable.Where(t => !t.Favorited);
            //var filtered = from t in observable
            //               where t.Favorited
            //               select t.Text;
            //var observer = new TwitterObserver();

            //var buffered = filtered.Buffer(TimeSpan.FromSeconds(5));

            //buffered.Subscribe(Observer.Create<IList<string>>((list) => Console.WriteLine("Received " + list.Count + " tweets")));

            Subject<string> subject = new Subject<string>();
            subject.Subscribe(Observer.Create<string>(Console.WriteLine));
            subject.Subscribe(Observer.Create<string>((str) => Console.WriteLine("I got a string!")));

            subject.OnNext("data!");
            subject.OnNext("next");
            subject.OnNext("next again");


            Console.ReadLine();
        }
    }

    class TwitterObserver : IObserver<Tweet>
    {
        public void OnNext(Tweet value)
        {
            if(value.User != null)
            Console.WriteLine(string.Format("{0} tweets: {1}", value.User.Name, value.Text));
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(string.Format("THERE WAS AN ERROR!!! {0}", error.Message));
        }

        public void OnCompleted()
        {
            Console.WriteLine("We're done listening to tweets");
        }
    }
}
