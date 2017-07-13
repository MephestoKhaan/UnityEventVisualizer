using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace EventVisualizer.Base
{
    public class EventsGraphWindow : EditorWindow
    {
        private EventsGraph _graph;
        private EventsGraphGUI _graphGUI;

        [MenuItem("Window/Events Graph editor")]
        static void ShowEditor()
        {
            EventsGraphWindow editor = EditorWindow.GetWindow<EventsGraphWindow>();
            editor.Initialize();
        }

        void Initialize()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _graph = EventsGraph.Create();
            _graphGUI = _graph.GetEditor();


            _graph.BuildGraph();
        }

        void OnGUI()
        {
            const float kBarHeight = 17;
            var width = position.width;
            var height = position.height;


            // Main graph area
            _graphGUI.BeginGraphGUI(this, new Rect(0, 0, width, height - kBarHeight));
            _graphGUI.OnGraphGUI();
            _graphGUI.EndGraphGUI();

            _graphGUI.CenterGraph();

            // Clear selection on background click
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 1)
                _graphGUI.ClearSelection();

            // Status bar
            GUILayout.BeginArea(new Rect(0, height - kBarHeight, width, kBarHeight));
            GUILayout.Label(_graph.name);
            GUILayout.EndArea();
        }
        
    }
}
