using System.Drawing;
using System;
using MindMurmur.Domain.Messages;

namespace MindMurmur.Lights
{
    public class MeditationMonitor : IObserver<MeditationState>
    {
        DMX dmx = null;
        private IDisposable unsubscriber;
        private bool first = true;
        private MeditationState last;
        public MeditationMonitor(DMX dmx)
        {
            this.dmx = dmx;
        }

        public virtual void Subscribe(IObservable<MeditationState> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional colors will not be processed.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(MeditationState value)
        {
            Console.WriteLine("Current color is {0}", value);
            if (first)
            {
                last = value;
                first = false;
            }
            else
            {
                Console.WriteLine("Current color is {0}, a change from {1}", value, last);
                last = value;
            }
        }
    }
}
