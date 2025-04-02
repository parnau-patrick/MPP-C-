using System;

namespace Laborator3.observer
{
    public interface IObserver
    {
        void Update();
    }

    public interface IObservable
    {
        void AddObserver(IObserver observer);
        void RemoveObserver(IObserver observer);
        void NotifyObservers();
    }
}