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

            foreach (Component component in ScriptableObject.FindObjectsOfType<Component>())
            {
                SerializedObject serializedObject = new UnityEditor.SerializedObject(component);
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
                            calls.Add(new EventCall(component,
                                receiver,
                                methodName.stringValue));
                        }
                    }
                }
            }
            return calls;
        }
    }
}