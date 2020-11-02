using UnityEditor;
using UnityEngine;

namespace EventVisualizer.Base
{
    public static class FindInGraphButton
    {
        [MenuItem("GameObject/EventGraph/Find in current graph", false, 0)]
        static void FindEvents()
        {
            EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
            if(window != null)
            {
                window.OverrideSelection(Selection.activeInstanceID);
            }
        }

		[MenuItem("GameObject/EventGraph/Graph just this", false, 0)]
		static void GraphSelection()
		{
			EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
			window.RebuildGraph(new GameObject[] { Selection.activeGameObject }, false);
		}
		[MenuItem("GameObject/EventGraph/Graph this hierarchy", false, 0)]
		static void GraphSelectionHierarchy()
		{
			EventsGraphWindow window = EditorWindow.GetWindow<EventsGraphWindow>();
			window.RebuildGraph(new GameObject[] { Selection.activeGameObject }, true);
		}
	}

}
