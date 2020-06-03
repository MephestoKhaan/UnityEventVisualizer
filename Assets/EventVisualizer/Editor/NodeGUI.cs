using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Graphs;
using System.Collections.Generic;

namespace EventVisualizer.Base
{
    public class NodeGUI : UnityEditor.Graphs.Node
    {
        #region Public class methods

        // Factory method
        static public NodeGUI Create(NodeData dataInstance)
        {
			bool isGameObject = dataInstance.Entity is GameObject;

			var node = CreateInstance<NodeGUI>();
            node.Initialize(dataInstance);
            node.name = dataInstance.Entity.GetInstanceID().ToString();
			node.icon = (Texture2D)EditorGUIUtility.IconContent(isGameObject?"Gameobject Icon" : "ScriptableObject Icon").image;
			return node;
        }

		#endregion

		#region Public member properties and methods

		private Texture2D icon;
		
        public bool isValid
        {
            get { return _runtimeInstance != null; }
        }

        #endregion

        #region Overridden virtual methods
		
        public override string title
        {
            get { return isValid ? _runtimeInstance.Name : "<Missing>"; }
        }


		public override void NodeUI(GraphGUI host)
		{
			base.NodeUI(host);
			if (icon != null)
			{
				GUI.DrawTexture(new Rect(Vector2.one * 5, new Vector2(20, 20)), icon);
			}
		}

		#endregion

		#region Private members

		NodeData _runtimeInstance;
		
		void Initialize(NodeData runtimeInstance)
        {
            hideFlags = HideFlags.DontSave;
			
            _runtimeInstance = runtimeInstance;
            position = new Rect(Vector2.one * UnityEngine.Random.Range(0, 500), Vector2.zero);

            PopulateSlots();
        }


		void PopulateSlots()
        {
            foreach (EventCall call in _runtimeInstance.Outputs)
            {
                string name = call.eventShortName;
                string title = ObjectNames.NicifyVariableName(name);
                if (!outputSlots.Any(s => s.title == title))
                {
                    var slot = AddOutputSlot(name);
                    slot.title = title;
                }
            }

            foreach (EventCall call in _runtimeInstance.Inputs)
            {
                string name = call.MethodFullPath;
                string title = ObjectNames.NicifyVariableName(name);
                if (!inputSlots.Any(s => s.title == title))
                {
                    var slot = AddInputSlot(name);
                    slot.title = title;
                }
            }
        }

        public void PopulateEdges()
        {
            foreach (var outSlot in outputSlots)
            {
                List<EventCall> outCalls = _runtimeInstance.Outputs.FindAll(call => call.eventShortName == outSlot.name);

                foreach (EventCall call in outCalls)
                {
                    var targetNode = graph[call.receiver.GetInstanceID().ToString()];
                    var inSlot = targetNode[call.MethodFullPath];
					
					if (graph.Connected(outSlot, inSlot))
					{
						Edge existingEdge = graph.edges.Find(e => e.fromSlot == outSlot && e.toSlot == inSlot);
						graph.RemoveEdge(existingEdge);
					}

					Edge edge = graph.Connect(outSlot, inSlot);
					call.OnTriggered += (() => EdgeTriggersTracker.RegisterTrigger(edge, call));
				}
            }
        }

        #endregion
    }
}
