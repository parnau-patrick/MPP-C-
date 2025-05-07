using System;
using ConcursModel.domain.observer;

namespace ConcursClient.service
{
    public class ClientObserverJsonProxy : IObserver
    {
        private IObserver client;

        public ClientObserverJsonProxy(IObserver client)
        {
            this.client = client;
        }

        public void Update()
        {
            client.Update();
        }
    }
}