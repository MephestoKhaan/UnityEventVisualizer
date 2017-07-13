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
        static public NodeGUI Create(NodeData runtimeInstance)
        {
            var node = CreateInstance<NodeGUI>();
            node.Initialize(runtimeInstance);
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

            // Update serialized position info if it's changed.
            if (isValid)
            {
                _serializedObject.Update();
            }
        }

        #endregion

        #region Private members

        // Runtime instance of this node
        [NonSerialized] NodeData _runtimeInstance;

        // Serialized property accessor
        SerializedObject _serializedObject;
        SerializedProperty _serializedPosition;




        // Initializer (called from the Create method)
        void Initialize(NodeData runtimeInstance)
        {
            hideFlags = HideFlags.DontSave;

            // Object references
            _runtimeInstance = runtimeInstance;
            _serializedObject = new UnityEditor.SerializedObject(runtimeInstance.Entity);
            _serializedPosition = _serializedObject.FindProperty("_wiringNodePosition");
            position = new Rect(Vector2.one * UnityEngine.Random.Range(0, 500), Vector2.zero);

            PopulateSlots();
        }

        void PopulateSlots()
        {
            foreach (EventCall call in _runtimeInstance.Inputs)
            {
                string title = ObjectNames.NicifyVariableName(call.EventName);
                if (!outputSlots.Any(s => s.title == title))
                {
                    var slot = AddOutputSlot(call.EventName);
                    slot.title = title;
                }
            }

            foreach (EventCall call in _runtimeInstance.Outputs)
            {
                string title = ObjectNames.NicifyVariableName(call.Method);
                if (!inputSlots.Any(s => s.title == title))
                {
                    var slot = AddInputSlot(call.Method);
                    slot.title = title;
                }
            }
        }

        public void PopulateEdges() //TODO certainly this could be cleaner
        {
            foreach(var outSlot in outputSlots)
            {



            }


            foreach (EventCall call in _runtimeInstance.Outputs)
            {
                Slot inSlot = null;
                Slot outSlot = null;
                
                outSlot = FindSlotByName(outputSlots,call.EventName);
                if (outputSlots != null)
                {
                    NodeGUI receiverNode = graph.nodes.Find(n =>
                    {
                        NodeGUI node = n as NodeGUI;
                        if (node != null)
                        {
                            return node.runtimeInstance.Entity == call.Receiver;
                        }
                        return false;
                    }) as NodeGUI;

                    //if(receiverNode != null)
                    {
                        inSlot = FindSlotByName(receiverNode.inputSlots, call.Method);
                    }
                    
                    if (outSlot != null && inSlot != null)
                    {
                        graph.Connect(outSlot, inSlot);
                    }
                }
            }
        }

        //helping method because for some reason LinQ does not want to work
        static Slot FindSlotByName(IEnumerable<Slot> slotCollection, string name)
        {
            foreach(var slot in slotCollection)
            {
                if(slot.name == name)
                {
                    return slot;
                }
            }
            return null;
        }


        #endregion
    }
}
