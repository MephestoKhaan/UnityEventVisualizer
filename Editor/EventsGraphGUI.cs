using UnityEngine;
using UnityEditor;
using EventVisualizer.Base;
using System.Collections.Generic;
using UnityEditor.Graphs;

namespace EventVisualizer.Base
{
    [System.Serializable]
	public class EventsGraphGUI : GraphGUI
    {
        [SerializeField]
        public int SelectionOverride;
		
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

			
			// Mouse drag
#if UNITY_2017 || UNITY_2017_1_OR_NEWER
			DragSelection();
#else
			DragSelection(new Rect(-5000, -5000, 10000, 10000));
#endif
			
		}
        
        public override IEdgeGUI edgeGUI
        {
            get
            {
                if (m_EdgeGUI == null)
                    m_EdgeGUI = new EdgeGUI { host = this };
                return m_EdgeGUI;
            }
        }

        
        

        public override void NodeGUI(UnityEditor.Graphs.Node node)
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
            OverrideSelection();
            if (selection.Count > 0)
            {
                int[] selectedIds = new int[selection.Count];
                for (int i = 0; i < selection.Count; i++)
                {
					if(selection[i] != null)
					{
						selectedIds[i] = int.Parse(selection[i].name);
					}
                }
                Selection.instanceIDs = selectedIds;
            }
        }

        private void OverrideSelection()
        {
            if (SelectionOverride != 0)
            {
                UnityEditor.Graphs.Node selectedNode = graph[SelectionOverride.ToString()];
                if (selectedNode != null)
                {
                    selection.Clear();
                    selection.Add(selectedNode);
                    CenterGraph(selectedNode.position.position);
                    
                }
                SelectionOverride = 0;
            }
        }
             
        
        

    }
}