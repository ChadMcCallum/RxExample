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

            //watching a range of numbers
            var observable = Observable.Range(1, 10);
            //creating an observable using static factory method
            //passing in an Action for dealing with each obserable OnNext
            var observer = Observer.Create<int>(
                (i) =>
                {
                    Console.WriteLine(i);
                    Thread.Sleep(1000);
                }
                );

            var observerToo = Observer.Create<int>(
                (i) => { Console.WriteLine("I wanna execute!"); });

            //options for observable execution
            observable.ObserveOn(Scheduler.NewThread).SubscribeOn(Scheduler.CurrentThread).Subscribe(observer);
            observable.Subscribe(observerToo);

            var longObserver = Observer.Create<long>(Console.WriteLine);
            //creating an observable with an interval, will execute OnNext every 2 seconds
            var observableTimer = Observable.Interval(TimeSpan.FromSeconds(2));
            observableTimer.Subscribe(longObserver);

            WebRequest request = WebRequest.Create("http://www.rtigger.com");
            //creating an observable from an async method
            var requestObservable = Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse);

            //subscribing to an observable inline
            //also executing code once the async method returns
            requestObservable.Invoke().Subscribe(
                Observer.Create<WebResponse>(
                    (response) => Console.WriteLine("Downloaded " + response.ContentLength + " bytes")));

            //creating an observable using TwitterObservable
            IObservable<Tweet> observable2 = new TwitterObservable();
            //creating an observable by filtering the original obserable
            IObservable<Tweet> filtered = observable2.Where(t => !t.Favorited);
            //creating an obserable with filter via linq
            var filteredWithLinq = from t in observable2
                           where t.Favorited
                           select t.Text;
            var observer2 = new TwitterObserver();
            //adding a buffer to the obserable - this will queue all tweets received into 5 second increments and return one big list
            var buffered = filteredWithLinq.Buffer(TimeSpan.FromSeconds(5));

            buffered.Subscribe(Observer.Create<IList<string>>((list) => Console.WriteLine("Received " + list.Count + " tweets")));

            //Creating a subject - basically a wrapper for an obserable
            var subject = new Subject<string>();
            subject.Subscribe(Observer.Create<string>(Console.WriteLine));
            subject.Subscribe(Observer.Create<string>((str) => Console.WriteLine("I got a string!")));

            //Sending values to the subject, which in turn informs all of its observables
            subject.OnNext("data!");
            subject.OnNext("next");
            subject.OnNext("next again");


            Console.ReadLine();
        }
    }

    class TwitterObserver : IObserver<Tweet>
    {
        //any time the observable receives a tweet, deal with it
        public void OnNext(Tweet value)
        {
            if(value.User != null)
            Console.WriteLine(string.Format("{0} tweets: {1}", value.User.Name, value.Text));
        }

        //any time the obserable throws an error, deal with it
        public void OnError(Exception error)
        {
            Console.WriteLine(string.Format("THERE WAS AN ERROR!!! {0}", error.Message));
        }

        //once the observable tells us it's done, deal with it
        public void OnCompleted()
        {
            Console.WriteLine("We're done listening to tweets");
        }
    }
}
