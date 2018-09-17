using System.Collections.Generic;
using UnityEngine;

namespace EventVisualizer.Base
{
    [System.Serializable]
    public class NodeData
    {
        public Object Entity { get; private set; }

        public string Name
        {
            get
            {
                return Entity != null ? Entity.name : "<Missing>";
            }
        }
        
        public List<EventCall> Outputs { get; private set; }
        public List<EventCall> Inputs { get; private set; }

        [SerializeField]
        private static Dictionary<int, NodeData> nodes = new Dictionary<int, NodeData>();

        public static ICollection<NodeData> Nodes
        {
            get
            {
                return nodes != null ? nodes.Values : null;
            }
        }
        


        public static void ClearAll() 
        {
            nodes.Clear();
        }

		public static void ClearSlots()
		{
			foreach(var node in nodes.Keys)
			{
				nodes[node].Outputs.Clear();
				nodes[node].Inputs.Clear();
			}
		}
        
        public static void RegisterEvent(EventCall eventCall)
        {
            CreateNode(eventCall.Sender);
            CreateNode(eventCall.Receiver);

			nodes[eventCall.Sender.GetInstanceID()].Outputs.Add(eventCall);
            nodes[eventCall.Receiver.GetInstanceID()].Inputs.Add(eventCall);
        }



        private static void CreateNode(Object entity)
        {
			int id = entity.GetInstanceID();

			if (!nodes.ContainsKey(id))
            {
                nodes.Add(id, new NodeData(entity));
            }
        }
        
        public NodeData(Object entity)
        {
            Entity = entity;
            Outputs = new List<EventCall>();
            Inputs = new List<EventCall>();
        }
    }

}