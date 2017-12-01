using UnityEngine;
using EventVisualizer.Base;
using System.Collections.Generic;
using UnityEditor.Graphs;
using System.Linq;

namespace EventVisualizer.Base
{
    [System.Serializable]
    public class EventsGraph : Graph
    {
        static public EventsGraph Create()
        {
            var graph = CreateInstance<EventsGraph>();
            graph.hideFlags = HideFlags.HideAndDontSave;
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
            foreach (EventCall call in EventsFinder.FindAllEvents())
            {
                NodeData.RegisterEvent(call);
            }
            foreach (NodeData data in NodeData.Nodes)
            {
                AddNode(NodeGUI.Create(data));
            }

            foreach (NodeGUI node in nodes)
            {
                node.PopulateEdges();
            }

            SortGraph();
        }

#region sorting

        [SerializeField]
        private HashSet<Node> positionedNodes = new HashSet<Node>();
        private const float VERTICAL_SPACING = 80f;
        private const float HORIZONTAL_SPACING = 400f;
        private void SortGraph()
        {
            positionedNodes.Clear();
            
            List<Node> sortedNodes = new List<Node>(nodes); //cannot sort the original collection so a clone is needed
            sortedNodes.Sort((x, y) =>
            {
                int xScore = x.outputEdges.Count() - x.inputEdges.Count();
                int yScore = y.outputEdges.Count() - y.inputEdges.Count();
                return yScore.CompareTo(xScore);
            });

            Vector2 position = Vector2.zero;
            foreach (Node node in sortedNodes)
            {
                if (!positionedNodes.Contains(node))
                {
                    positionedNodes.Add(node);
                    position.y += PositionNodeHierarchy(node, position);
                }
            }
        }
        

        private float PositionNodeHierarchy(Node currentNode, Vector2 masterPosition)
        {
            float height = VERTICAL_SPACING;
            foreach (var outputEdge in currentNode.outputEdges)
            {
                Node node = outputEdge.toSlot.node;
                if (!positionedNodes.Contains(node))
                {
                    positionedNodes.Add(node);
                    height += PositionNodeHierarchy(node, masterPosition 
                        + Vector2.right * HORIZONTAL_SPACING
                        + Vector2.up * height);
                }
            }
            currentNode.position = new Rect(masterPosition + Vector2.up * height * 0.5f, currentNode.position.size);

            return height;
        }

#endregion
        
    }
}