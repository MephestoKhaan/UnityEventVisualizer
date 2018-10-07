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

		public void RebuildGraph()
		{
			BuildGraph();
			SortGraph(nodes,false);
		}

		public void RefreshGraphConnections()
		{
			Dictionary<string, Rect> positions = new Dictionary<string, Rect>();
			List<Node> adriftNodes = new List<Node>();
			foreach (NodeGUI node in nodes)
			{
				positions.Add(node.name, node.position);
			}
			BuildGraph();

			foreach (NodeGUI node in nodes)
			{
				if(positions.ContainsKey(node.name))
				{
					node.position = positions[node.name];
				}
				else
				{
					adriftNodes.Add(node);
				}
			}
			SortGraph(adriftNodes,true);
		}

		private void BuildGraph()
		{
			NodeData.ClearAll();
			Clear(true);
			foreach (EventCall call in EventsFinder.FindAllEvents())
			{
				NodeData.RegisterEvent(call);
			}

			foreach (NodeData data in NodeData.Nodes)
			{
				NodeGUI node = NodeGUI.Create(data);
				if (!nodes.Contains(node))
				{
					AddNode(node);
				}
			}
			foreach (NodeGUI node in nodes)
			{
				node.PopulateEdges();
			}
		}

		#region sorting

		[SerializeField]
		private HashSet<Node> positionedNodes = new HashSet<Node>();
		private const float VERTICAL_SPACING = 80f;
		private const float HORIZONTAL_SPACING = 400f;
		private void SortGraph(List<Node> nodes, bool skipParents)
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
					position.y += PositionNodeHierarchy(node, position, skipParents);
				}
			}
		}


		private float PositionNodeHierarchy(Node currentNode, Vector2 masterPosition, bool skipParents)
		{
			float height = VERTICAL_SPACING;
			if (!skipParents)
			{
				foreach (var outputEdge in currentNode.outputEdges)
				{
					Node node = outputEdge.toSlot.node;
					if (!positionedNodes.Contains(node))
					{
						positionedNodes.Add(node);
						height += PositionNodeHierarchy(node, masterPosition
							+ Vector2.right * HORIZONTAL_SPACING
							+ Vector2.up * height, skipParents);
					}
				}
			}
			currentNode.position = new Rect(masterPosition + Vector2.up * height * 0.5f, currentNode.position.size);

			return height;
		}

		#endregion

	}
}