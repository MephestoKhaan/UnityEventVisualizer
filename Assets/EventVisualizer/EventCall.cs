using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace EventVisualizer.Base
{
    [System.Serializable]
    public class EventCall
    {
        public Object Sender { get; private set; }
        public Object Receiver { get; private set; }
        public string EventName { get; private set; }
        public string Method { get; private set; }
        public string ReceiverComponentName { get; private set; }

        public string MethodFullPath
        {
            get
            {
                return ReceiverComponentName + Method;
            }
        }

        public System.Action OnTriggered;

        private static Regex parenteshesPattern = new Regex(@"\((.*)\)");

        public EventCall(Object sender, Object receiver, string eventName, string methodName, UnityEventBase unityEvent)
        {
            Sender = sender as Component ? (sender as Component).gameObject : sender;
            Receiver = receiver as Component ? (receiver as Component).gameObject : receiver;
            EventName = eventName;
            Method = methodName;

            UpdateReceiverComponentName(receiver);
            AttachTrigger(unityEvent);
        }
        private void AttachTrigger(UnityEventBase unityEvent)
        {
            if (unityEvent == null)
            {
                return;
            }
            MethodInfo eventRegisterMethod = unityEvent.GetType().GetMethod("AddListener");
            if (eventRegisterMethod != null)
            {
                ParameterInfo parameter = eventRegisterMethod.GetParameters()[0];
                
                var parameterType = parameter.ParameterType;
                if (parameterType.Name == "UnityAction")
                {
                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    { new UnityAction(() =>
                        { if (OnTriggered != null)
                                OnTriggered.Invoke();
                        })
                    });
                }
                /*if (parameterType.Name == "UnityAction`1")
                {
                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    { new UnityAction<UnityEngine.EventSystems.BaseEventData>((a) =>
                        { if (OnTriggered != null)
                                OnTriggered.Invoke();
                        })
                    });
                }*/
                if (parameterType.Name == "UnityAction`1")
                {
                    //object param = System.Activator.CreateInstance(typeof(UnityAction<>).MakeGenericType(new System.Type[] { }));
                    System.Type type = typeof(UnityAction<>).MakeGenericType(new System.Type[] { typeof(UnityEngine.EventSystems.BaseEventData) });
                    //System.Delegate del = System.Delegate.CreateDelegate(parameterType, this, "TriggerOneArg);

                    MethodInfo vesselMethod = this.GetType().GetMethod("TriggerOneArg");
                    System.Delegate triggerAction = System.Delegate.CreateDelegate(parameterType, vesselMethod);
                    eventRegisterMethod.Invoke(unityEvent, new object[]
                    {
                        System.Activator.CreateInstance(parameterType, triggerAction)
                    });
                }

                /*
                if (parameterType.Name == "UnityAction`2")
                {
                    eventRegisterMethod.Invoke(unityEvent, new object[] { new UnityAction<object, object>((a, b) => Debug.Log("AR2")) });
                }
                if (parameterType.Name == "UnityAction`3")
                {
                    eventRegisterMethod.Invoke(unityEvent, new object[] { new UnityAction<object, object, object>((a, b, c) => Debug.Log("AR3")) });
                }
                if (parameterType.Name == "UnityAction`4")
                {
                    eventRegisterMethod.Invoke(unityEvent, new object[] { new UnityAction<object, object, object, object>((a, b, c, d) => Debug.Log("AR4")) });
                }
                */
            }
        }

        public void TriggerZeroArgs()
        {

        }

        public static void TriggerOneArg(UnityEngine.EventSystems.BaseEventData arg)
        {
            Debug.Log("whatever");
        }

        private void UpdateReceiverComponentName(Object component)
        {
            if (Receiver != null)
            {
                MatchCollection matches = parenteshesPattern.Matches(component.ToString());
                if (matches != null && matches.Count > 0)
                {
                    ReceiverComponentName = matches[matches.Count - 1].Value;
                    if (ReceiverComponentName.Length > 1)
                    {
                        ReceiverComponentName = ReceiverComponentName.Remove(0, 1);
                    }
                    ReceiverComponentName = ReceiverComponentName.Replace(")", ".");
                }
            }
        }
    }
}