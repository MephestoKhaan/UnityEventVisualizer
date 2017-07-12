using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EventsFinder : MonoBehaviour
{

    void Start()
    {

        Component[] allComponents = ScriptableObject.FindObjectsOfType<Component>();

        foreach (Component component in allComponents)
        {
            SerializedObject serializedObject = new UnityEditor.SerializedObject(component);
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.Next(true);

            while (iterator.Next(false))
            {
                if (iterator.type != null
                    && iterator.type != string.Empty)
                {
                    SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (persistentCalls != null)
                    {

                        for (int i = 0; i < persistentCalls.arraySize; ++i)
                        {
                            SerializedProperty methodName = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_MethodName");
                            SerializedProperty target = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target");

                            Debug.Log("InstanceID: " + target.objectReferenceInstanceIDValue + " calls: " + methodName.stringValue);
                            //callState.intValue = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
                        }
                    }

                }
            }

        }
    }


}
