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

                // Show the subwindow of this node.
                node.position = GUILayout.Window(
                    node.GetInstanceID(), node.position,
                    delegate { NodeGUI(node2); },
                    node.title, style, GUILayout.Width(150)
                );
            }

            // Workaround: If there is no node in the graph, put an empty
            // window to avoid corruption due to a bug.
            if (graph.nodes.Count == 0)
                GUILayout.Window(0, new Rect(0, 0, 1, 1), delegate {}, "", "MiniLabel");

            m_Host.EndWindows();

            // Graph edges
            edgeGUI.DoEdges();
            edgeGUI.DoDraggedEdge();

            // Mouse drag
            DragSelection(new Rect(-5000, -5000, 10000, 10000));

            // Context menu
            HandleMenuEvents();
        }
        /*
         

        public override void OnGraphGUI()
        {
            eventCalls = EventsFinder.FindAllEvents(); 
            m_Host.BeginWindows();
            foreach (var call in eventCalls)
            {
                DrawEventCall(call);
            }
            m_Host.EndWindows();
        }


        void DrawEventCall(EventCall call)
        {
            StoreWindow(call.Sender);
            StoreWindow(call.Receiver);

            

            windows[call.Sender] = GUI.Window(call.Sender.GetInstanceID(), windows[call.Sender], DrawNodeWindow, call.Sender.name);
            windows[call.Receiver] = GUI.Window(call.Receiver.GetInstanceID(), windows[call.Receiver], DrawNodeWindow, call.Receiver.name);

            DrawNodeCurve(windows[call.Sender], windows[call.Receiver]);
        }
        
        void StoreWindow(Component comp)
        {
            if (!windows.ContainsKey(comp))
            {
                windows.Add(comp, new Rect(Random.Range(0, 600), Random.Range(0, 600), 100, 100));
            }
        }
        */
        void DrawNodeWindow(int id)
        {
            GUI.DragWindow();
            Selection.activeInstanceID = id;
        }

        void DrawNodeCurve(Rect start, Rect end)
        {
            Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 3);


            float arrowSize = 6f;
            Handles.color = Color.black;
            Handles.DrawAAConvexPolygon(new Vector3[]
            {
            endPos + new Vector3(-1,-1,0) * arrowSize,
            endPos,
            endPos +  new Vector3(-1,1,0) * arrowSize });


        }
    }
}