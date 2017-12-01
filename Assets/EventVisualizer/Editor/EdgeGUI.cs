//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Graphs = UnityEditor.Graphs;
using UnityEditor.Graphs;

namespace EventVisualizer.Base
{
    // Specialized edge drawer class
    public class EdgeGUI : Graphs.IEdgeGUI
    {
        #region Public members

        public Graphs.GraphGUI host { get; set; }
        public List<int> edgeSelection { get; set; }

        public EdgeGUI()
        {
            edgeSelection = new List<int>();
        }

        #endregion

        #region IEdgeGUI implementation


        public void DoEdges()
        {
            // Draw edges on repaint.
            if (Event.current.type == EventType.Repaint)
            {
                foreach (var edge in host.graph.edges)
                {
                    if (edge == _moveEdge) continue;

                    float hue = HueOutputSlot(edge.fromSlot);
                    DrawEdge(edge, Color.HSVToRGB(hue, 1f, 1f));
                }
            }
        }


        private float HueOutputSlot(Slot slot)
        {
            int count = 0;
            int index = 0;
            foreach (var s in slot.node.outputSlots)
            {
                if (s == slot)
                {
                    index = count;
                }
                count++;
            }

            return Mathf.Repeat((index % 2 == 0 ? (float)index : index + (count + 1f) * 0.5f) / count, 1f);

        }

        public void DoDraggedEdge()
        {

        }

        public void BeginSlotDragging(Graphs.Slot slot, bool allowStartDrag, bool allowEndDrag)
        {

        }

        public void SlotDragging(Graphs.Slot slot, bool allowEndDrag, bool allowMultiple)
        {

        }

        public void EndSlotDragging(Graphs.Slot slot, bool allowMultiple)
        {

        }


        public void EndDragging()
        {

        }

        public Graphs.Edge FindClosestEdge()
        {
            return null;
        }


        #endregion

        #region Private members

        Graphs.Edge _moveEdge;
        Graphs.Slot _dragSourceSlot;
        Graphs.Slot _dropTarget;

        #endregion

        #region Edge drawer

        const float kEdgeWidth = 4;
        const float kEdgeSlotYOffset = 9;

        static void DrawEdge(Graphs.Edge edge, Color color)
        {
            var p1 = GetPositionAsFromSlot(edge.fromSlot);
            var p2 = GetPositionAsToSlot(edge.toSlot);
            DrawEdge(p1, p2, color * edge.color, EdgeTriggersTracker.GetTimings(edge));
        }

        static void DrawEdge(Vector2 p1, Vector2 p2, Color color, List<float> triggers)
        {
            Color prevColor = Handles.color;
            Handles.color = color;

            var l = Mathf.Min(Mathf.Abs(p1.y - p2.y), 50);
            Vector2 p3 = p1 + new Vector2(l, 0);
            Vector2 p4 = p2 - new Vector2(l, 0);
            var texture = (Texture2D)Graphs.Styles.connectionTexture.image;
            Handles.DrawBezier(p1, p2, p3, p4, color, texture, kEdgeWidth);


            foreach (var trigger in triggers)
            {
                Vector3 pos = CalculateBezierPoint(trigger, p1, p2, p3, p4);
                Handles.DrawSolidArc(pos, Vector3.back, pos + Vector3.up, 360, kEdgeWidth * 2);

            }

            Handles.color = prevColor;
        }

        #endregion

        #region Utilities to access private members

        // Accessors for Slot.m_Position
        static Rect GetSlotPosition(Graphs.Slot slot)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var func = typeof(Graphs.Slot).GetField("m_Position", flags);
            return (Rect)func.GetValue(slot);
        }

        static Vector2 GetPositionAsFromSlot(Graphs.Slot slot)
        {
            var rect = GetSlotPosition(slot);
            return GUIClip(new Vector2(rect.xMax, rect.y + kEdgeSlotYOffset));
        }

        static Vector2 GetPositionAsToSlot(Graphs.Slot slot)
        {
            var rect = GetSlotPosition(slot);
            return GUIClip(new Vector2(rect.x, rect.y + kEdgeSlotYOffset));
        }

        // Caller for GUIClip.Clip
        static Vector2 GUIClip(Vector2 pos)
        {
            var type = Type.GetType("UnityEngine.GUIClip,UnityEngine");
            var method = type.GetMethod("Clip", new Type[] { typeof(Vector2) });
            return (Vector2)method.Invoke(null, new object[] { pos });
        }

        #endregion

        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1.0f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }

}
