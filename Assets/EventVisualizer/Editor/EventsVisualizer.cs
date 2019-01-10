using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.Events;
using System;
using UnityEditor.Callbacks;
using System.Linq;
using com.spacepuppyeditor;

namespace EventVisualizer.Base
{

    public static class EventsFinder
    {
        public static List<EventCall> FindAllEvents()
        {
            HashSet<EventCall> calls = new HashSet<EventCall>();
			
			var sw = System.Diagnostics.Stopwatch.StartNew();
			foreach (var type in ComponentsThatCanHaveUnityEvent) {
				foreach (Component caller in GameObject.FindObjectsOfType(type)) {
					ExtractDefaultEventTriggers(calls, caller);
					ExtractEvents(calls, caller);
				}
			}
			Debug.Log("UnityEventVisualizer FindAllEvents(). Milliseconds: " + sw.Elapsed.TotalMilliseconds);
			
			return calls.ToList();
        }
        
        private static void ExtractEvents(HashSet<EventCall> calls, Component caller)
        {
            SerializedProperty iterator = new SerializedObject(caller).GetIterator();
            iterator.Next(true);
			RecursivelyExtractEvents(calls, caller, iterator, 0);
        }

		private static bool RecursivelyExtractEvents(HashSet<EventCall> calls, Component caller, SerializedProperty iterator, int level) {
			bool hasData = true;

			do {
				SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
				bool isUnityEvent = persistentCalls != null;
				if (isUnityEvent && persistentCalls.arraySize > 0) {
					UnityEventBase unityEvent = EditorHelper.GetTargetObjectOfProperty(iterator) as UnityEventBase;
					AddEventCalls(calls, caller, unityEvent, iterator.displayName, iterator.propertyPath);
				}
				hasData = iterator.Next(!isUnityEvent);
				if (hasData) {
					if (iterator.depth < level) return hasData;
					else if (iterator.depth > level) hasData = RecursivelyExtractEvents(calls, caller, iterator, iterator.depth);
				}
			}
			while (hasData);
			return false;
		}

        private static void ExtractDefaultEventTriggers(HashSet<EventCall> calls, Component caller)
        {
            EventTrigger eventTrigger = caller as EventTrigger;
            if (eventTrigger != null)
            {
                foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
                {
					string name = trigger.eventID.ToString();
					AddEventCalls(calls, caller, trigger.callback, name, name);
                }
            }
        }

		private static void AddEventCalls(HashSet<EventCall> calls, Component caller, UnityEventBase unityEvent, string eventShortName, string eventFullName) {
			for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++) {
				string methodName = unityEvent.GetPersistentMethodName(i);
				UnityEngine.Object receiver = unityEvent.GetPersistentTarget(i);

				if (receiver != null && methodName != null && methodName != "") {
					calls.Add(new EventCall(caller, receiver, eventShortName, eventFullName, methodName, unityEvent));
				}
			}
		}





		

		public static bool NeedsGraphRefresh = false;
		
		private static HashSet<Type> ComponentsThatCanHaveUnityEvent = new HashSet<Type>();
		private static Dictionary<Type, bool> TmpSearchedTypes = new Dictionary<Type, bool>();

		[DidReloadScripts, InitializeOnLoadMethod]
		static void RefreshTypesThatCanHoldUnityEvents() {
			var sw = System.Diagnostics.Stopwatch.StartNew();

			var objects = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
				.SelectMany(a => a.GetTypes())
				.Where(t => typeof(Component).IsAssignableFrom(t));

			foreach (var obj in objects) {
				if (RecursivelySearchFields<UnityEventBase>(obj)) {
					ComponentsThatCanHaveUnityEvent.Add(obj);
				}
			}
			TmpSearchedTypes.Clear();

			NeedsGraphRefresh = true;
			Debug.Log("UnityEventVisualizer Updated Components that can have UnityEvents (" + ComponentsThatCanHaveUnityEvent.Count + "). Milliseconds: " + sw.Elapsed.TotalMilliseconds);
		}

		/// <summary>
		/// Search for types that have a field or property of type <typeparamref name="T"/> or can hold an object that can.
		/// </summary>
		/// <typeparam name="T">Needle</typeparam>
		/// <param name="type">Haystack</param>
		/// <returns>Can contain some object <typeparamref name="T"/></returns>
		static bool RecursivelySearchFields<T>(Type type) {
			bool wanted;
			if (TmpSearchedTypes.TryGetValue(type, out wanted)) return wanted;
			TmpSearchedTypes.Add(type, false);

			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var fType in type.GetFields(flags).Where(f => !f.FieldType.IsPrimitive).Select(f => f.FieldType).Concat(type.GetProperties(flags).Select(p => p.PropertyType))) {
				if (typeof(T).IsAssignableFrom(fType)) {
					return TmpSearchedTypes[type] |= true;
				}
				else if (typeof(UnityEngine.Object).IsAssignableFrom(fType)) {
					continue;
				}
				else if (!TmpSearchedTypes.TryGetValue(fType, out wanted)) {
					if (RecursivelySearchFields<T>(fType)) {
						return TmpSearchedTypes[type] |= true;
					}
				}
				else if (wanted) {
					return TmpSearchedTypes[type] |= true;
				}
			}

			if (type.IsArray) {
				if (RecursivelySearchFields<T>(type.GetElementType())) {
					return TmpSearchedTypes[type] |= true;
				}
			}

			return false;
		}
	}
}