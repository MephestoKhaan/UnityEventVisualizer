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
            _graph.BuildGraph();

            _graphGUI = _graph.GetEditor();
            _graphGUI.CenterGraph();

            EditorUtility.SetDirty(_graphGUI);
            EditorUtility.SetDirty(_graph);
        }

        void OnGUI()
        {
            var width = position.width;
            var height = position.height;

            if(_graphGUI != null)
            {
                // Main graph area
                _graphGUI.BeginGraphGUI(this, new Rect(0, 0, width, height - kBarHeight));
                _graphGUI.OnGraphGUI();
                _graphGUI.EndGraphGUI();

                // Clear selection on background click
                var e = Event.current;
                if (e.type == EventType.MouseDown && e.clickCount == 1)
                    _graphGUI.ClearSelection();
            }


            // Status bar
            GUILayout.BeginArea(new Rect(0, height - kBarHeight, width, kBarHeight));
            if (GUILayout.Button("Refresh"))
            {
                Refresh();
            }
            GUILayout.EndArea();
        }
        
        public void OverrideSelection(int overrideIndex)
        {
            _graphGUI.SelectionOverride = overrideIndex;
            EditorUtility.SetDirty(this);
        }


        void Refresh()
        {
            _graph.Clear();
            _graph.BuildGraph();
        }
    }
}
