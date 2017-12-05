using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.Events;

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
                    calls.AddRange(ExtractEvents(caller));
                    calls.AddRange(ExtractDefaultEventTriggers(caller));
                }
            }
            return calls;
        }
        
        private static List<EventCall> ExtractEvents(Component caller)
        {
            SerializedProperty iterator = new SerializedObject(caller).GetIterator();
            iterator.Next(true);

            List<EventCall> calls = new List<EventCall>();

            do
            {
                SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                if (persistentCalls != null)
                {
                    UnityEventBase unityEvent = FindEvent(caller, iterator);
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
                                methodName.stringValue,
                                unityEvent));
                        }
                    }
                }
            } while (iterator.Next(false));

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
                                 trigger.callback.GetPersistentMethodName(i),
                                 trigger.callback));
                    }

                }
            }

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


        private static UnityEventBase FindEvent(Component caller, SerializedProperty iterator)
        {
            PropertyInfo eventPropertyInfo = caller.GetType().GetProperty(iterator.propertyPath);
            if (eventPropertyInfo == null)
            {
                string fieldToPropertyName = iterator.propertyPath.Replace("m_", "");
                fieldToPropertyName = fieldToPropertyName[0].ToString().ToLower() + fieldToPropertyName.Substring(1);

                eventPropertyInfo = caller.GetType().GetProperty(fieldToPropertyName);
            }
            if (eventPropertyInfo != null)
            {
                return eventPropertyInfo.GetValue(caller, null) as UnityEventBase;
            }

            FieldInfo eventFieldInfo = caller.GetType().GetField(iterator.propertyPath);
            if (eventFieldInfo == null)
            {
                string fieldToFieldName = iterator.propertyPath.Replace("m_", "");
                fieldToFieldName = fieldToFieldName[0].ToString().ToLower() + fieldToFieldName.Substring(1);

                eventFieldInfo = caller.GetType().GetField(fieldToFieldName);
            }
            if (eventFieldInfo != null)
            {
                return  eventFieldInfo.GetValue(caller) as UnityEventBase;
            }
            return null;
        }
    }
}