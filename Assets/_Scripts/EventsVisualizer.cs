using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{

    public class EventsFinder : MonoBehaviour
    {
        public static List<EventCall> FindAllEvents()
        {
            List<EventCall> calls = new List<EventCall>();

            foreach (Component caller in ScriptableObject.FindObjectsOfType<Component>())
            {
                SerializedObject serializedObject = new UnityEditor.SerializedObject(caller);
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.Next(true);

                while (iterator.Next(false))
                {
                    SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (persistentCalls != null)
                    {
                        for (int i = 0; i < persistentCalls.arraySize; ++i)
                        {
                            SerializedProperty methodName = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_MethodName");
                            SerializedProperty target = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target");
                            Component receiver = EditorUtility.InstanceIDToObject(target.objectReferenceInstanceIDValue) as Component;

                            calls.Add(new EventCall(caller,
                                receiver,
                                iterator.displayName,
                                methodName.stringValue));
                        }
                    }
                }
            }
            return calls;
        }
    }
}