using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

namespace EventVisualizer.Base
{

    public static class EventsFinder
    {
        public static List<EventCall> FindAllEvents()
        {
            List<EventCall> calls = new List<EventCall>();

            foreach (GameObject go in GetAllObjectsInScene())
            {
                foreach (Component caller in go.GetComponents<Component>())
                {
                    if (caller == null)
                    {
                        continue;
                    }
                    SerializedObject serializedObject = new UnityEditor.SerializedObject(caller);
                    calls.AddRange(ExtractEvents(caller, serializedObject));
                    calls.AddRange(ExtractDefaultEventTriggers(caller));
                }
            }
            return calls;
        }

        private static List<EventCall> ExtractDefaultEventTriggers(Component caller)
        {
            List<EventCall> calls = new List<EventCall>();
            EventTrigger eventTrigger = caller as EventTrigger;
            if (eventTrigger != null)
            {
                foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
                {
                    for (int i = 0; i < trigger.callback.GetPersistentEventCount(); i++)
                    {
                        calls.Add(new EventCall(caller,
                                  trigger.callback.GetPersistentTarget(i),
                                 trigger.eventID.ToString(),
                                 trigger.callback.GetPersistentMethodName(i)));
                    }

                }
            }

            return calls;
        }



        private static List<EventCall> ExtractEvents(Component caller, SerializedObject serializedObject)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.Next(true);

            List<EventCall> calls = new List<EventCall>();

            do
            {
                SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                if (persistentCalls != null)
                {
                    for (int i = 0; i < persistentCalls.arraySize; ++i)
                    {
                        SerializedProperty methodName = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_MethodName");
                        SerializedProperty target = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target");
                        Object receiver = EditorUtility.InstanceIDToObject(target.objectReferenceInstanceIDValue);

                        if (receiver != null)
                        {
                            calls.Add(new EventCall(caller,
                                receiver,
                                iterator.displayName,
                                methodName.stringValue));
                        }
                    }
                }
            } while (iterator.Next(false));

            return calls;
        }


        public static List<GameObject> GetAllObjectsInScene(bool rootOnly = false)
        {
            GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

            List<GameObject> pReturn = new List<GameObject>();

            foreach (GameObject pObject in pAllObjects)
            {
                if (rootOnly
                    && pObject.transform.parent != null)
                {
                    continue;
                }

                if (pObject.hideFlags == HideFlags.NotEditable
                    || pObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                if (Application.isEditor)
                {
                    string sAssetPath = AssetDatabase.GetAssetPath(pObject.transform.root.gameObject);
                    if (!string.IsNullOrEmpty(sAssetPath))
                    {
                        continue;
                    }
                }

                pReturn.Add(pObject);
            }

            return pReturn;
        }
    }
}