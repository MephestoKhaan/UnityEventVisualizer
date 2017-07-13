using UnityEngine;
using UnityEditor;
using EventVisualizer.Base;
using System.Collections.Generic;
using UnityEditor.Graphs;

namespace EventVisualizer.Base
{
    public class EventsGraphGUI : GraphGUI
    {
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
        }
        
        
    }
}