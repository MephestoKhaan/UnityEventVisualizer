using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{
    public static class EventsFinder
    {
        public static List<EventCall> FindAllEvents()
        {
            List<EventCall> calls = new List<EventCall>();
            
            foreach(GameObject go in GetAllObjectsInScene())
            {
                foreach (Component caller in go.GetComponents<Component>())
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
    }
}