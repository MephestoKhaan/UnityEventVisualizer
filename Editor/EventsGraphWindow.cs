using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{
	public class EventsGraphWindow : EditorWindow
	{
		[SerializeField]
		private EventsGraph _graph;
		[SerializeField]
		private EventsGraphGUI _graphGUI;

		private const float kZoomMin = 0.1f;
		private const float kZoomMax = 1.0f;

		private Rect _zoomArea = new Rect(0.0f, 75.0f, 600.0f, 300.0f - 100.0f);
		private float _zoom = 1f;
		private Vector2 _zoomCoordsOrigin = Vector2.zero;

		private const float kBarHeight = 17;

		[MenuItem("Window/Events Graph editor")]
		static void ShowEditor()
		{
			EventsGraphWindow editor = EditorWindow.GetWindow<EventsGraphWindow>();
			editor.hideFlags = HideFlags.HideAndDontSave;
			editor.Initialize();
		}


		private bool initialized = false;
		public void Initialize()
		{
			if (initialized) return;
			initialized = true;
			_graph = EventsGraph.Create();
			//_graph.RebuildGraph();

			_graphGUI = _graph.GetEditor();
			_graphGUI.CenterGraph();

			EditorUtility.SetDirty(_graphGUI);
			EditorUtility.SetDirty(_graph);
		}

		private static readonly string[] toolbarStrings = new string[] {"Rebuild on selected Hierarchy", "Rebuild JUST selected", "Update connections" };

		void OnGUI()
		{
			Initialize();

			var width = position.width;
			var height = position.height;
			_zoomArea = new Rect(0, 0, width, height);
			HandleEvents();

			if (_graphGUI != null)
			{
				Rect r = EditorZoomArea.Begin(_zoom, _zoomArea);
				// Main graph area
				_graphGUI.BeginGraphGUI(this, r);
				_graphGUI.OnGraphGUI();
				_graphGUI.EndGraphGUI();

				// Clear selection on background click
				var e = Event.current;
				if (e.type == EventType.MouseDown && e.clickCount == 1)
					_graphGUI.ClearSelection();
				
				EditorZoomArea.End();
			}


			// Status bar
			GUILayout.BeginArea(new Rect(0, 0, width, kBarHeight+5));
			int result = GUILayout.Toolbar(-1, toolbarStrings);
			if(result == 0) RebuildGraphOnSelected(true);
			else if (result == 1) RebuildGraphOnSelected(false);
			else if(result == 2) RefreshGraphConnections();
			GUILayout.EndArea();

			const float maxWidth = 200;
			GUILayout.BeginArea(new Rect(0, kBarHeight + 5, maxWidth, 20 * 5), GUI.skin.box);
			showOnlyWhenSelected.Set(EditorGUILayout.Toggle("Show only when selected", showOnlyWhenSelected.Get(), GUILayout.MaxWidth(maxWidth)));
			showLabels.Set(EditorGUILayout.Toggle("Labels", showLabels.Get(), GUILayout.MaxWidth(maxWidth)));
			showComponentName.Set(EditorGUILayout.Toggle("Function Full Path", showComponentName.Get(), GUILayout.MaxWidth(maxWidth)));
			eventFullName.Set(EditorGUILayout.Toggle("Event Full Name", eventFullName.Get(), GUILayout.MaxWidth(maxWidth)));
			showTimesExecuted.Set(EditorGUILayout.Toggle("Times Executed", showTimesExecuted.Get(), GUILayout.MaxWidth(maxWidth)));
			GUILayout.EndArea();
		}

		public SavedPrefBool showOnlyWhenSelected = new SavedPrefBool("EventVisualizer_showOnlyWhenSelected", true);
		public SavedPrefBool showLabels = new SavedPrefBool("EventVisualizer_showLabels", true);
		public SavedPrefBool showComponentName = new SavedPrefBool("EventVisualizer_showComponentName", true);
		public SavedPrefBool showTimesExecuted = new SavedPrefBool("EventVisualizer_showTimesExecuted", true);
		public SavedPrefBool eventFullName = new SavedPrefBool("EventVisualizer_eventFullName", true);
		public float separation = 3;
		public GUISkin guiSkin;


		public class SavedPrefBool {
			public readonly string name;
			protected readonly bool defaultValue;
			protected bool value;
			private bool ready = false;

			public SavedPrefBool(string name, bool defaultValue) {
				this.name = name;
				this.defaultValue = defaultValue;
			}

			public bool Get() {
				if (!ready) {
					ready = true;
					value = EditorPrefs.GetBool(name, defaultValue);
				}
				return value;
			}
			public void Set(bool value) {
				this.value = value;
				EditorPrefs.SetBool(name, value);
			}
		}

		private void Update()
		{
			if (EdgeTriggersTracker.HasData())
			{
				Repaint();
			}
		}

		public void OverrideSelection(int overrideIndex)
		{
			_graphGUI.SelectionOverride = overrideIndex;
		}

		public Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
		{
			return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
		}


		private void HandleEvents()
		{
			if (Event.current.type == EventType.ScrollWheel)
			{
				Vector2 screenCoordsMousePos = Event.current.mousePosition;
				Vector2 delta = Event.current.delta;
				Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
				float zoomDelta = -delta.y / 150.0f;
				float oldZoom = _zoom;
				_zoom += zoomDelta;
				_zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
				_zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);

				Event.current.Use();
			}
		}


		public void RebuildGraphOnSelected(bool searchHierarchy)
		{
			RebuildGraph(Selection.gameObjects, searchHierarchy);
		}

		public void RebuildGraph(GameObject[] roots, bool searchHierarchy)
		{
			if (_graph != null)
			{
				_graph.RebuildGraph(Selection.gameObjects, searchHierarchy);
			}
		}

		public void RefreshGraphConnections()
		{
			Debug.Log("Refreshing UnityEventVisualizer Graph Connections");
			if (_graph != null)
			{
				_graph.RefreshGraphConnections();
			}
		}


		void OnFocus() {
			RemoveCallbacks();
			AddCallbacks();
		}

		void OnDestroy() {
			RemoveCallbacks();
		}

		void AddCallbacks() {
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}
		void RemoveCallbacks() {
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		void OnPlayModeStateChanged(PlayModeStateChange mode) {
			switch (mode) {
				case PlayModeStateChange.EnteredEditMode:
					break;
				case PlayModeStateChange.EnteredPlayMode:
					RefreshGraphConnections();
					break;
			}
		}

		private struct Bezier {
			public enum Tangent { None, Auto, Positive, Negative, PositiveUnscaled, NegativeUnscaled }
			public Vector2 start;
			public Vector2 end;
			public Tangent startTangent, endTangent;
		}

		private struct EventBox {
			public EventCall ev;
			public GUIContent content;
			public Rect rect;
		}

		private Dictionary<EventCall, Bezier> beziersToDraw = new Dictionary<EventCall, Bezier>();
		private List<EventBox> boxesToDraw = new List<EventBox>();
		void OnSceneGUI(SceneView sceneView) {
			Handles.BeginGUI();
			GUI.skin = guiSkin;
			
			foreach (var elem in NodeData.Nodes) {
				GameObject entity = elem.Entity as GameObject;
				if (null != entity) {
					bool isEntitySelected = CheckSelection(entity.gameObject);
					bool behindScreen;
					var senderPos2D = WorldToGUIPoint(entity.transform.position, SceneView.currentDrawingSceneView.camera.transform.position, out behindScreen);

					if (isEntitySelected) {
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject receiver = ev.receiver as GameObject;
							GameObject sender = ev.sender as GameObject;

							if (null != sender && receiver != ev.sender) {
								if (behindScreen) senderPos2D = WorldToGUIPoint(entity.transform.position, sender.transform.position, out behindScreen);
								var rect = DrawEventBox(sb, "➜● ", boxesToDraw, ev, ref senderPos2D, behindScreen);
								sb.Length = 0;

								Bezier b;
								beziersToDraw.TryGetValue(ev, out b);
								b.endTangent = Bezier.Tangent.Negative;
								b.end = new Vector2(rect.x, rect.y + rect.height * 0.5f);
								beziersToDraw[ev] = b;
							}
						}
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject receiver = ev.receiver as GameObject;
							if (receiver == ev.sender) {
								if (behindScreen) continue; // Don't draw
								var rect = DrawEventBox(sb, " ●  ", boxesToDraw, ev, ref senderPos2D, behindScreen);
								sb.Length = 0;

								Bezier b;
								beziersToDraw.TryGetValue(ev, out b);
								b.endTangent = Bezier.Tangent.NegativeUnscaled;
								b.startTangent = Bezier.Tangent.NegativeUnscaled;
								b.start = new Vector2(rect.x, rect.y + rect.height * 0.25f);
								b.end = new Vector2(rect.x, rect.y + rect.height * 0.75f);
								beziersToDraw[ev] = b;
							}
						}
						for (int i = 0; i < elem.Outputs.Count; i++) {
							var ev = elem.Outputs[i];
							GameObject receiver = ev.receiver as GameObject;
							if (null != receiver && receiver != ev.sender) {
								if (behindScreen) senderPos2D = WorldToGUIPoint(entity.transform.position, receiver.transform.position, out behindScreen);
								var rect = DrawEventBox(sb, "●➜ ", boxesToDraw, ev, ref senderPos2D, behindScreen);
								sb.Length = 0;

								Bezier b;
								beziersToDraw.TryGetValue(ev, out b);
								b.startTangent = behindScreen ? Bezier.Tangent.None : Bezier.Tangent.Positive;
								b.start = new Vector2(rect.x + rect.width, rect.y + rect.height * 0.5f);
								beziersToDraw[ev] = b;
							}
						}
					}
					else {
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject sender = ev.sender as GameObject;
							if (null != sender) {
								bool isSenderSelected = CheckSelection(sender.gameObject);

								if (isSenderSelected || ev.lastTimeExecuted + EdgeTriggersTracker.TimeToLive >= EditorApplication.timeSinceStartup) {
									if (behindScreen) senderPos2D = WorldToGUIPoint(entity.transform.position, sender.transform.position, out behindScreen);
									var localEnd2D = senderPos2D + new Vector2(0, separation * ev.nodeSender.Inputs.IndexOf(ev));
									
									Bezier b;
									beziersToDraw.TryGetValue(ev, out b);
									b.endTangent = behindScreen ? Bezier.Tangent.None : Bezier.Tangent.Auto;
									b.end = localEnd2D;
									beziersToDraw[ev] = b;
								}
							}
						}
						
						for (int i = 0; i < elem.Outputs.Count; i++) {
							var ev = elem.Outputs[i];
							GameObject receiver = ev.receiver as GameObject;
							if (null != receiver) {
								bool isReceiverSelected = CheckSelection(receiver.gameObject);

								if (isReceiverSelected || ev.lastTimeExecuted + EdgeTriggersTracker.TimeToLive >= EditorApplication.timeSinceStartup) {
									if (behindScreen) senderPos2D = WorldToGUIPoint(entity.transform.position, receiver.transform.position, out behindScreen);
									var localStart2D = senderPos2D + new Vector2(0, separation * i);
									
									Bezier b;
									beziersToDraw.TryGetValue(ev, out b);
									b.startTangent = Bezier.Tangent.Auto;
									b.start = localStart2D;
									beziersToDraw[ev] = b;
								}
							}
						}

					}
				}
			}

			foreach (var elem in beziersToDraw) {
				DrawConnection(elem.Key, elem.Value);
			}
			beziersToDraw.Clear();

			foreach (var box in boxesToDraw) {
				DrawEventBox(box);
			}
			boxesToDraw.Clear();

			Handles.EndGUI();
		}

		private bool CheckSelection(GameObject go) {
			return !showOnlyWhenSelected.Get() || Selection.Contains(go);
		}

		private Rect DrawEventBox(StringBuilder sb, string type, List<EventBox> boxesToDraw, EventCall ev, ref Vector2 boxPos, bool outsideScreen) {
			Rect rect = new Rect(boxPos, new Vector2(0, separation));

			if (!outsideScreen && showLabels.Get()) {
				sb.Append(type);
				if (showTimesExecuted.Get()) sb.Append("(").Append(ev.timesExecuted).Append(") ");
				sb.Append(eventFullName.Get() ? ev.eventFullName : ev.eventShortName).Append("  ▶  ");
				if (showComponentName.Get()) sb.Append(ev.ReceiverComponentNameSimple).Append(".");
				sb.Append(ev.method);

				GUIContent content = new GUIContent(sb.ToString());

				rect.size = GUI.skin.box.CalcSize(content);

				boxesToDraw.Add(new EventBox() {
					content = content,
					ev = ev,
					rect = rect
				});
			}

			boxPos.y += rect.height;
			return rect;
		}


		private static void DrawEventBox(EventBox box) {
			var originalContentColor = GUI.contentColor;
			var originalBackgroundColor = GUI.backgroundColor;

			GUI.backgroundColor = box.ev.color;
			GUI.contentColor = Brightness(box.ev.color) < 0.5f ? Color.white : Color.black;
			
			if (GUI.Button(box.rect, box.content)) {
				Selection.activeObject = box.ev.sender;
			}

			GUI.contentColor = originalContentColor;
			GUI.backgroundColor = originalBackgroundColor;
		}

		private static float Brightness(Color color) {
			return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
		}
		
		private static void DrawConnection(EventCall ev, Bezier b) {
			const float tangentSize = 50;

			float diff = b.end.x - b.start.x;
			diff = Mathf.Sign(diff) * Mathf.Min(Mathf.Abs(diff), tangentSize);

			var p1 = b.start;
			var p2 = b.end;
			var p3 = p1;
			var p4 = p2;

			if (b.startTangent == Bezier.Tangent.Auto) p3 += new Vector2(diff, 0);
			else if (b.startTangent == Bezier.Tangent.Negative) p3 -= new Vector2(Math.Abs(diff), 0);
			else if (b.startTangent == Bezier.Tangent.Positive) p3 += new Vector2(Math.Abs(diff), 0);
			else if (b.startTangent == Bezier.Tangent.NegativeUnscaled) p3 -= new Vector2(tangentSize, 0);
			else if (b.startTangent == Bezier.Tangent.PositiveUnscaled) p3 += new Vector2(tangentSize, 0);

			if (b.endTangent == Bezier.Tangent.Auto) p4 -= new Vector2(diff, 0);
			else if (b.endTangent == Bezier.Tangent.Negative) p4 -= new Vector2(Math.Abs(diff), 0);
			else if (b.endTangent == Bezier.Tangent.Positive) p4 += new Vector2(Math.Abs(diff), 0);
			else if (b.endTangent == Bezier.Tangent.NegativeUnscaled) p4 -= new Vector2(tangentSize, 0);
			else if (b.endTangent == Bezier.Tangent.PositiveUnscaled) p4 += new Vector2(tangentSize, 0);
			
			Color c = ev.color;
			Color prevColor = Handles.color;
			Handles.color = c;
			Handles.DrawBezier(p1, p2, p3, p4, c, (Texture2D) UnityEditor.Graphs.Styles.selectedConnectionTexture.image, EdgeGUI.kEdgeWidth);
			foreach (var trigger in EdgeTriggersTracker.GetTimings(ev)) {
				Vector3 pos = EdgeGUI.CalculateBezierPoint(trigger, p1, p3, p4, p2);
				Handles.DrawSolidArc(pos, Vector3.back, pos + Vector3.up, 360, EdgeGUI.kEdgeWidth);
			}
			Handles.color = prevColor;
		}

		/// <param name="p2">if p is behind the screen, p2 is used to trace a line to p and raycast it to the near clipping camera of the scene camera</param>
		private static Vector2 WorldToGUIPoint(Vector3 p, Vector3 p2, out bool behindScreen) {
			Camera cam = SceneView.currentDrawingSceneView.camera;
			Vector3 viewPos = cam.WorldToViewportPoint(p);
			behindScreen = viewPos.z < 0;

			if (behindScreen && cam.WorldToViewportPoint(p2).z < 0) return Vector2.zero;

			if (behindScreen) {
				if (p2 == cam.transform.position) return Vector2.zero;
				
				var Plane = new Plane(cam.transform.forward, cam.transform.position + cam.transform.forward * cam.nearClipPlane);
				Ray r = new Ray(p, p2 - p);
				float enter;
				
				if (!Plane.Raycast(r, out enter)) return Vector2.zero;

				Vector3 proj = r.origin + r.direction * enter;

				viewPos = cam.WorldToViewportPoint(proj);
			}
			float scaleFactor = 1f;
			return new Vector2(
				viewPos.x *  cam.scaledPixelWidth,
				(1 - viewPos.y) * cam.scaledPixelHeight) * scaleFactor;
		}
	}
}