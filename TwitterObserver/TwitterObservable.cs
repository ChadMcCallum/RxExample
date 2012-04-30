using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TwitterObserver
{
    public class TwitterObservable : IObservable<Tweet>
    {
        private List<IObserver<Tweet>> observers = new List<IObserver<Tweet>>();

        public TwitterObservable()
        {
            TwitterFirehose.StartStream(OnTweet);
        }

        //every time we recieve a tweet (from our Firehose class)
        //notify all our observers
        private void OnTweet(Task<Tweet> task)
        {
            foreach(IObserver<Tweet> observer in observers)
            {
                observer.OnNext(task.Result);
            }
        }

        //here's the actual IObservable method we have to implement
        //just keeping a list of observers so we can nofity them later
        public IDisposable Subscribe(IObserver<Tweet> observer)
        {
            observers.Add(observer);
            var unsubscriber = new Unsubscriber(observer, observers);
            return unsubscriber;
        }
    }

    //placeholder class for making unsubscribing from an observable
    //easier
    public class Unsubscriber : IDisposable
    {
        private IObserver<Tweet> _observer;
        private List<IObserver<Tweet>> _observers;

        public Unsubscriber(IObserver<Tweet> observer, List<IObserver<Tweet>> observers)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}
