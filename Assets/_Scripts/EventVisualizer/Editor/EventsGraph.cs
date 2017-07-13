using UnityEngine;
using UnityEditor;
using EventVisualizer.Base;
using System.Collections.Generic;
using UnityEditor.Graphs;

namespace EventVisualizer.Base
{
    public class EventsGraph : Graph
    {
        static public EventsGraph Create()
        {
            var graph = CreateInstance<EventsGraph>();
            return graph;
        }

        public EventsGraphGUI GetEditor()
        {
            var gui = CreateInstance<EventsGraphGUI>();
            gui.graph = this;
            gui.hideFlags = HideFlags.HideAndDontSave;
            return gui;
            
        }

        public void BuildGraph()
        {
            NodeData.Clear(); 
            Clear(true);

            foreach (var call in EventsFinder.FindAllEvents())
            {
                NodeData.RegisterEvent(call);
            }
            
            foreach(NodeData node in NodeData.Nodes)
            {
                AddNode(NodeGUI.Create(node));
            }

            foreach (NodeGUI node in nodes)
            {
                node.PopulateEdges();
            }

        }
        
    }
}