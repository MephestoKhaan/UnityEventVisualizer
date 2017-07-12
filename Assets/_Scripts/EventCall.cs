using UnityEngine;

namespace EventVisualizer.Base
{

    public class EventCall
    {
        public Component Sender { get; private set; }
        public Component Receiver { get; private set; }
        public string Method { get; private set; }

        public EventCall(Component sender, Component receiver, string method)
        {
            Sender = sender;
            Receiver = receiver;
            Method = method;
        }
    }

}