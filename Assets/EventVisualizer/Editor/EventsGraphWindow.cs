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
			_graph.RebuildGraph();

			_graphGUI = _graph.GetEditor();
			_graphGUI.CenterGraph();

			EditorUtility.SetDirty(_graphGUI);
			EditorUtility.SetDirty(_graph);
		}

		private static readonly string[] toolbarStrings = new string[] { "Update connections", "Clear" };

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
			if (result == 0)
			{
				RefreshGraphConnections();
			}
			else if(result == 1)
			{
				RebuildGraph();
			}
			GUILayout.EndArea();

		}
		public GUISkin guiSkin;

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

		void RebuildGraph()
		{
			if(_graph != null)
			{
				_graph.RebuildGraph();
			}
		}
		void RefreshGraphConnections()
		{
			if (_graph != null)
			{
				_graph.RefreshGraphConnections();
			}
		}



		void OnFocus() {
			// Remove delegate listener if it has previously been assigned.
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			// Add (or re-add) the delegate.
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		void OnDestroy() {
			// When the window is destroyed, remove the delegate so that it will no longer do any drawing.
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		private struct Bezier {
			public enum Tangent { Auto, Positive, Negative, PositiveUnscaled, NegativeUnscaled }
			public Vector2 start;
			public Vector2 end;
			public Tangent startTangent, endTangent;
		}

		void OnSceneGUI(SceneView sceneView) {
			Handles.BeginGUI();
			GUI.skin = guiSkin;

			Dictionary<EventCall, Bezier> beziersToDraw = new Dictionary<EventCall, Bezier>();
			
			foreach (var elem in NodeData.Nodes) {
				GameObject entity = elem.Entity as GameObject;
				if (null != entity) {
					bool isEntitySelected = Selection.Contains(entity.gameObject);
					var senderPos2D = HandleUtility.WorldToGUIPoint(entity.transform.position);

					if (isEntitySelected) {
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject receiver = ev.Receiver as GameObject;
							GameObject sender = ev.Sender as GameObject;

							if (null != receiver && receiver != ev.Sender) {
								AddEventText(sb, "➜● | ", ev);
								var rect = DrawEventBox(ref senderPos2D, new GUIContent(sb.ToString(), "IN"), ev.color);
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
							GameObject receiver = ev.Receiver as GameObject;
							if (receiver == ev.Sender) {
								AddEventText(sb, "● | ", ev);
								var rect = DrawEventBox(ref senderPos2D, new GUIContent(sb.ToString(), "IN-OUT"), ev.color);
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
							GameObject receiver = ev.Receiver as GameObject;
							if (null != receiver && receiver != ev.Sender) {
								AddEventText(sb, "●➜ | ", ev);
								var rect = DrawEventBox(ref senderPos2D, new GUIContent(sb.ToString(), "OUT"), ev.color);
								sb.Length = 0;

								Bezier b;
								beziersToDraw.TryGetValue(ev, out b);
								b.startTangent = Bezier.Tangent.Positive;
								b.start = new Vector2(rect.x + rect.width, rect.y + rect.height * 0.5f);
								beziersToDraw[ev] = b;
							}
						}
					}
					else {
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject sender = ev.Sender as GameObject;
							if (null != sender) {
								bool isSenderSelected = Selection.Contains(sender.gameObject);

								if (isSenderSelected || ev.lastTimeExecuted + EdgeTriggersTracker.TimeToLive >= EditorApplication.timeSinceStartup) {
									var localEnd2D = senderPos2D + new Vector2(0, separation * ev.nodeSender.Inputs.IndexOf(ev));
									
									Bezier b;
									beziersToDraw.TryGetValue(ev, out b);
									b.endTangent = Bezier.Tangent.Auto;
									b.end = localEnd2D;
									beziersToDraw[ev] = b;
								}
							}
						}
						
						for (int i = 0; i < elem.Outputs.Count; i++) {
							var ev = elem.Outputs[i];
							GameObject receiver = ev.Receiver as GameObject;
							if (null != receiver) {
								bool isReceiverSelected = Selection.Contains(receiver.gameObject);

								if (isReceiverSelected || ev.lastTimeExecuted + EdgeTriggersTracker.TimeToLive >= EditorApplication.timeSinceStartup) {
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

			Handles.EndGUI();
		}

		private static Rect DrawEventBox(ref Vector2 boxPos, GUIContent content, Color color) {
			var originalContentColor = GUI.contentColor;
			var originalBackgroundColor = GUI.backgroundColor;

			GUI.backgroundColor = color;
			GUI.contentColor = Brightness(color) < 0.5f ? Color.white : Color.black;

			var size = GUI.skin.box.CalcSize(content);
			var rect = new Rect(boxPos, size);
			GUI.Box(rect, content);

			GUI.contentColor = originalContentColor;
			GUI.backgroundColor = originalBackgroundColor;

			boxPos.y += size.y;

			return rect;
		}

		private static float Brightness(Color color) {
			return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
		}

		private static void AddEventText(StringBuilder sb, string type, EventCall ev) {
			sb.Append(type).Append("(").Append(ev.timesExecuted).Append(") ").Append(ev.EventName).Append("  ▶  ").Append(ev.ReceiverComponentName).Append("/").Append(ev.Method);
		}

		private static void DrawConnection(EventCall ev, Bezier b) {
			Vector2 start = b.start;
			Vector2 end = b.end;

			const float tangentSize = 50;

			float diff = (end.x - start.x);
			diff = Mathf.Sign(diff) * Mathf.Min(Mathf.Abs(diff), tangentSize);

			var p1 = start;
			var p2 = end;
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
	}
}