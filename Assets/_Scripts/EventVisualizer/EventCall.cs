using UnityEngine;

namespace EventVisualizer.Base
{
    public class EventCall
    {
        public Object Sender { get; private set; }
        public Object Receiver { get; private set; }
        public string EventName { get; private set; }
        public string Method { get; private set; }

        public EventCall(Object sender, Object receiver, string eventName, string method)
        {
            Sender = sender;
            Receiver = receiver;
            EventName = eventName;
            Method = method;
        }
    }
}