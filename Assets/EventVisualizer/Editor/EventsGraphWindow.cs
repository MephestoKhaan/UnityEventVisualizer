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

		public void Initialize()
		{
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

		void OnSceneGUI(SceneView sceneView) {
			Handles.BeginGUI();
			GUI.skin = guiSkin;
			foreach (var elem in NodeData.Nodes) {
				GameObject sender = elem.Entity as GameObject;
				if (null != sender) {
					bool isSenderSelected = Selection.Contains(sender.gameObject);
					var start2D = HandleUtility.WorldToGUIPoint(sender.transform.position);
					for (int i = 0; i < elem.Outputs.Count; i++) {
						var output = elem.Outputs[i];
						GameObject receiver = output.Receiver as GameObject;
						if (null != receiver) {
							//Gizmos.DrawIcon(endGo.transform.position, PngSender, true);
							bool isReceiverSelected = Selection.Contains(receiver.gameObject);
							bool isSelected = isSenderSelected || isReceiverSelected;

							if (isSelected || output.lastTimeExecuted + timeFading >= EditorApplication.timeSinceStartup - output.lastTimeExecuted) {
								var end = receiver.transform.position;
								var end2D = HandleUtility.WorldToGUIPoint(end);

								var localStart2D = start2D + new Vector2(0, separation * i);
								var localEnd2D = end2D + new Vector2(0, separation * output.nodeSender.Inputs.IndexOf(output));

								Vector2 diff = (localEnd2D - localStart2D);
								diff.y = 0;
								diff.x = Mathf.Sign(diff.x) * Mathf.Min(Mathf.Abs(diff.x), 50);

								Color color = EdgeGUI.ColorForIndex(Animator.StringToHash(output.EventName));

								if (!isSelected) color.a *= Mathf.Clamp01(1f - (float) (EditorApplication.timeSinceStartup - output.lastTimeExecuted) / timeFading);

								var p1 = localStart2D;
								var p2 = localEnd2D;
								var p3 = localStart2D + diff;
								var p4 = localEnd2D - diff;

								Color prevColor = Handles.color;
								Handles.color = color;
								Handles.DrawBezier(p1, p2, p3, p4, color, (Texture2D) UnityEditor.Graphs.Styles.selectedConnectionTexture.image, EdgeGUI.kEdgeWidth);
								foreach (var trigger in EdgeTriggersTracker.GetTimings(output)) {
									Vector3 pos = EdgeGUI.CalculateBezierPoint(trigger, p1, p3, p4, p2);
									Handles.DrawSolidArc(pos, Vector3.back, pos + Vector3.up, 360, EdgeGUI.kEdgeWidth);
								}
								Handles.color = prevColor;
							}
						}
					}









					if (isSenderSelected) {
						var boxPos = start2D;
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < elem.Inputs.Count; i++) {
							var ev = elem.Inputs[i];
							GameObject receiver = ev.Receiver as GameObject;
							if (null != receiver) {
								Color color = EdgeGUI.ColorForIndex(Animator.StringToHash(ev.EventName));
								AddEventText(sb, true, ev);
								DrawEventBox(ref boxPos, sb, color);
								sb.Length = 0;
							}
						}
						for (int i = 0; i < elem.Outputs.Count; i++) {
							var ev = elem.Outputs[i];
							GameObject receiver = ev.Receiver as GameObject;
							if (null != receiver) {
								Color color = EdgeGUI.ColorForIndex(Animator.StringToHash(ev.EventName));
								AddEventText(sb, false, ev);
								DrawEventBox(ref boxPos, sb, color);
								sb.Length = 0;
							}
						}
					}
				}
			}
			Handles.EndGUI();
		}

		private static void DrawEventBox(ref Vector2 boxPos, StringBuilder sb, Color color) {
			var originalContentColor = GUI.contentColor;
			var originalBackgroundColor = GUI.backgroundColor;

			GUI.backgroundColor = color;
			GUI.contentColor = Brightness(color) < 0.5f ? Color.white : Color.black;

			var content = new GUIContent(sb.ToString());
			var size = GUI.skin.box.CalcSize(content);
			GUI.Box(new Rect(boxPos, size), content);

			GUI.contentColor = originalContentColor;
			GUI.backgroundColor = originalBackgroundColor;

			boxPos.y += size.y;
		}

		private static float Brightness(Color color) {
			return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
		}

		private static void AddEventText(StringBuilder sb, bool isIn, EventCall ev) {
			sb.Append(isIn ? "[IN] " : "[OUT] ").Append("(").Append(ev.timesExecuted).Append(") ").Append(ev.EventName).Append(" => ").Append(ev.ReceiverComponentName).Append("/").Append(ev.Method);
		}
	}
}