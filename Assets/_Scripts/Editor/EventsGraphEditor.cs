using UnityEngine;
using UnityEditor;
using EventVisualizer.Base;
using System.Collections.Generic;

public class EventsGraphEditor : EditorWindow
{
    Dictionary<Component, Rect> windows = new Dictionary<Component, Rect>();
    
    [MenuItem("Window/Events Graph editor")]

    static void ShowEditor()
    {
        EventsGraphEditor editor = EditorWindow.GetWindow<EventsGraphEditor>();
        editor.Init();
    }

    public void Init()
    {
    }

    void OnGUI()
    {

        List<EventCall> eventCalls = EventsFinder.FindAllEvents();

        BeginWindows();
        foreach(var call in eventCalls)
        {
            DrawEventCall(call);
        }
        EndWindows();
    }


    void DrawEventCall(EventCall call)
    {
        StoreWindow(call.Sender);
        StoreWindow(call.Receiver);

        windows[call.Sender] = GUI.Window(call.Sender.GetInstanceID(), windows[call.Sender], DrawNodeWindow, call.Sender.name);
        windows[call.Receiver] = GUI.Window(call.Receiver.GetInstanceID(), windows[call.Receiver], DrawNodeWindow, call.Receiver.name);

        DrawNodeCurve(windows[call.Sender], windows[call.Receiver]);
    }

    void StoreWindow(Component comp) 
    {
        if(!windows.ContainsKey(comp))
        {
            windows.Add(comp, new Rect(Random.Range(0,600), Random.Range(0, 600), 100,100)); 
        }
    }

    void DrawNodeWindow(int id)
    {
        GUI.DragWindow();
        Selection.activeInstanceID = id;
    }

    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for (int i = 0; i < 3; i++) // Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
}
