using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Graphs;
using System.Linq;
using UnityEditor;

namespace EventVisualizer.Base
{
	[System.Serializable]
	public class EventsGraph : Graph
	{
		private const int WARNING_CALLS_THRESOLD = 1000;
		private GameObject[] selectedRoots;
		private bool searchingHierarchy;

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

		public void RebuildGraph(GameObject[] roots, bool searchHierarchy)
		{
			this.selectedRoots = roots;
			this.searchingHierarchy = searchHierarchy;
			BuildGraph(selectedRoots, searchHierarchy);
			SortGraph(nodes,false);
		}

		public void RefreshGraphConnections()
		{
			Dictionary<string, Rect> positions = new Dictionary<string, Rect>();
			List<UnityEditor.Graphs.Node> adriftNodes = new List<UnityEditor.Graphs.Node>();
			foreach (NodeGUI node in nodes)
			{
				positions.Add(node.name, node.position);
			}
			BuildGraph(this.selectedRoots, this.searchingHierarchy);

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

		private void BuildGraph(GameObject[] roots, bool searchHierarchy)
		{
			NodeData.ClearAll();
			Clear(true);
			List<EventCall> calls = EventsFinder.FindAllEvents(roots, searchHierarchy);
			if(calls.Count > WARNING_CALLS_THRESOLD)
			{
				bool goAhead = EditorUtility.DisplayDialog("Confirm massive graph",
					"You are about to generate a graph with "+ calls.Count+" events.\n"
					+ "Tip: You can select some gameobjects and search events in just those or their children instead.",
					"Go ahead",
					"Abort");

				if(goAhead)
				{
					GenerateGraphFromCalls(calls);
				}
			}
			else
			{
				GenerateGraphFromCalls(calls);
			}
		}

		private void GenerateGraphFromCalls(List<EventCall> calls)
		{
			foreach (EventCall call in calls)
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
		private HashSet<UnityEditor.Graphs.Node> positionedNodes = new HashSet<UnityEditor.Graphs.Node>();
		private const float VERTICAL_SPACING = 80f;
		private const float HORIZONTAL_SPACING = 400f;
		private void SortGraph(List<UnityEditor.Graphs.Node> nodes, bool skipParents)
		{
			positionedNodes.Clear();

			List<UnityEditor.Graphs.Node> sortedNodes = new List<UnityEditor.Graphs.Node>(nodes); //cannot sort the original collection so a clone is needed
			sortedNodes.Sort((x, y) =>
			{
				int xScore = x.outputEdges.Count() - x.inputEdges.Count();
				int yScore = y.outputEdges.Count() - y.inputEdges.Count();
				return yScore.CompareTo(xScore);
			});

			Vector2 position = Vector2.zero;
			foreach (UnityEditor.Graphs.Node node in sortedNodes)
			{
				if (!positionedNodes.Contains(node))
				{
					positionedNodes.Add(node);
					position.y += PositionNodeHierarchy(node, position, skipParents);
				}
			}
		}


		private float PositionNodeHierarchy(UnityEditor.Graphs.Node currentNode, Vector2 masterPosition, bool skipParents)
		{
			float height = VERTICAL_SPACING;
			if (!skipParents)
			{
				foreach (var outputEdge in currentNode.outputEdges)
				{
					UnityEditor.Graphs.Node node = outputEdge.toSlot.node;
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