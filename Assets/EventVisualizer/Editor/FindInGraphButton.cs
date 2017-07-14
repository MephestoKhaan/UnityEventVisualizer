using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{
    public static class FindInGraphButton
    {
        [MenuItem("GameObject/EventGraph/Find this", false, 0)]
        static void FindEvents()
        {
            EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
            if(window != null)
            {
                window.OverrideSelection(Selection.activeInstanceID);
            }
        }

    }

}
