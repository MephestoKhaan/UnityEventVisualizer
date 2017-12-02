using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Events;

namespace EventVisualizer.Base
{
    public static class EdgeTriggersTracker
    {
        public class EdgeTrigger
        {
            public Edge edge;
            public float triggeredTime;
        }

        private readonly static float TimeToLive = 1f;
        private static List<EdgeTrigger> triggers = new List<EdgeTrigger>();

        public static void RegisterTrigger(Edge edge)
        {
            triggers.Add(new EdgeTrigger() { edge = edge, triggeredTime = Time.unscaledTime });
        }

        public static List<float> GetTimings(Edge edge)
        {
            float now = Time.unscaledTime;
            List<EdgeTrigger> acceptedTriggers = triggers.FindAll(t => t.edge == edge);

            List<float> timings = new List<float>();//TODO cache
            foreach (EdgeTrigger t in acceptedTriggers)
            {
                float time = Mathf.Abs(t.triggeredTime - now) / TimeToLive;
                if (time <= 1f)
                {
                    timings.Add(time);
                }
                else
                {
                    triggers.Remove(t);
                }
            }
            return timings;
        }

        public static void CleanObsolete()
        {
            float now = Time.unscaledTime;
            triggers.RemoveAll(trigger => Mathf.Abs(now - trigger.triggeredTime) > TimeToLive);
        }

        public static bool HasData()
        {
            return triggers.Count > 0;
        }
    }
}