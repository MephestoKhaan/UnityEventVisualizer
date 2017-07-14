using System;
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

        public void Initialize()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _graph = EventsGraph.Create();
            _graphGUI = _graph.GetEditor();
            
            _graph.BuildGraph();

            _graphGUI.CenterGraph();
        }

        void OnGUI()
        {
            if(_graph == null)
            {
                Initialize();
                return;
            }
            const float kBarHeight = 17;
            var width = position.width;
            var height = position.height;


            // Main graph area
            _graphGUI.BeginGraphGUI(this, new Rect(0, 0, width, height - kBarHeight));
            _graphGUI.OnGraphGUI();
            _graphGUI.EndGraphGUI();


            // Clear selection on background click
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 1)
                _graphGUI.ClearSelection();

            // Status bar
            GUILayout.BeginArea(new Rect(0, height - kBarHeight, width, kBarHeight));
            if(GUILayout.Button("Refresh"))
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
