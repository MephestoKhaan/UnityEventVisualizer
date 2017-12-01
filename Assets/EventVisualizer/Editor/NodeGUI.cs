using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;
using System.Reflection;
using Graphs = UnityEditor.Graphs;
using System.Linq;
using UnityEditor.Graphs;
using System.Collections.Generic;

namespace EventVisualizer.Base
{
    public class NodeGUI : Graphs.Node
    {
        #region Public class methods

        // Factory method
        static public NodeGUI Create(NodeData dataInstance)
        {
            var node = CreateInstance<NodeGUI>();
            node.Initialize(dataInstance);
            node.name = dataInstance.Entity.GetInstanceID().ToString();
            return node;
        }

        #endregion

        #region Public member properties and methods

        // Runtime instance access
        public NodeData runtimeInstance
        {
            get { return _runtimeInstance; }
        }

        // Validity check
        public bool isValid
        {
            get { return _runtimeInstance != null; }
        }

        #endregion

        #region Overridden virtual methods

        // Node display title
        public override string title
        {
            get { return isValid ? _runtimeInstance.Name : "<Missing>"; }
        }

        // Dirty callback
        public override void Dirty()
        {
            base.Dirty();
        }

        #endregion

        #region Private members

        // Runtime instance of this node
        NodeData _runtimeInstance;

        // Initializer (called from the Create method)
        void Initialize(NodeData runtimeInstance)
        {
            hideFlags = HideFlags.DontSave;

            // Object references
            _runtimeInstance = runtimeInstance;
            position = new Rect(Vector2.one * UnityEngine.Random.Range(0, 500), Vector2.zero);

            PopulateSlots();
        }

        void PopulateSlots()
        {
            foreach (EventCall call in _runtimeInstance.Outputs)
            {
                string name = call.EventName;
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
                List<EventCall> outCalls = _runtimeInstance.Outputs.FindAll(call => call.EventName == outSlot.name);

                foreach (EventCall call in outCalls)
                {
                    var targetNode = graph[call.Receiver.GetInstanceID().ToString()];
                    var inSlot = targetNode[call.MethodFullPath];

                    if (!graph.Connected(outSlot, inSlot))
                    {
                        Edge edge = graph.Connect(outSlot, inSlot);
                    }
                }
            }
        }

        #endregion
    }
}
