using UnityEngine;
using UnityEditor;
using EventVisualizer.Base;
using System.Collections.Generic;
using UnityEditor.Graphs;

namespace EventVisualizer.Base
{
    public class EventsGraphGUI : GraphGUI
    {
        private bool ignoreNextSelection = false;

        public override void OnGraphGUI()
        {
            // Show node subwindows.
            m_Host.BeginWindows();

            foreach (var node in graph.nodes)
            {
                // Recapture the variable for the delegate.
                var node2 = node;

                // Subwindow style (active/nonactive)
                var isActive = selection.Contains(node);
                var style = Styles.GetNodeStyle(node.style, node.color, isActive);
                
                node.position = GUILayout.Window(
                    node.GetInstanceID(), 
                    node.position,
                    delegate { NodeGUI(node2); },
                    node.title, 
                    style, 
                    GUILayout.Width(150)
                );
            }
            
            if (graph.nodes.Count == 0)
            { 
                GUILayout.Window(0, new Rect(0, 0, 1, 1), delegate {}, "", "MiniLabel");
            }

            m_Host.EndWindows();

            // Graph edges
            edgeGUI.DoEdges();
            edgeGUI.DoDraggedEdge();

            // Mouse drag
            DragSelection(new Rect(-5000, -5000, 10000, 10000));

            HandleRemoteSelection();
        }

        public override void NodeGUI(Node node)
        {
            SelectNode(node);

            foreach (var slot in node.inputSlots)
                LayoutSlot(slot, slot.title, false, true, true, Styles.triggerPinIn);

            node.NodeUI(this);

            foreach (var slot in node.outputSlots)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                LayoutSlot(slot, slot.title, true, false, true, Styles.triggerPinOut);
                EditorGUILayout.EndHorizontal();
            }

            DragNodes();

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (selection.Count > 0)
            {
                int[] selectedIds = new int[selection.Count];
                for (int i = 0; i < selection.Count; i++)
                {
                    selectedIds[i] = int.Parse(selection[i].name);
                }
                ignoreNextSelection = true;
                Selection.instanceIDs = selectedIds;
            }
        }

        private void HandleRemoteSelection()
        {
            selection.Clear();

            foreach(int id in Selection.instanceIDs)
            {
                Node node = graph[id.ToString()];
                if(node != null)
                {
                    selection.Add(node);
                }
            }
        }
    }
}