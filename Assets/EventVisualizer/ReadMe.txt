#EventVisualizer

##What
Have you ever come across a project that abuses linking UnityEvents in the inspector and now you can
not find who is calling what?
Event Visualizer is a visual tool that allows you to see all the UnityEvents in a scene at
a glance and when they are being triggered. It creates a graph in which nodes are gameobjects,
outputs are any type of UnityEvent (custom ones supported as well!) and inputs are methods.

##How

- Select ```Windows/Events Graph Editor``` you can open the graph.
- Select any root gameobject(s) and click on ```Rebuild on selected hierarchy``` to generate a graph
of all events being fired by the selected hierarchy, or  ```Rebuild JUST selected``` to generate a
graph of all events being fired by exactly the selected gameobjects. You can deselect everything and 
click any of the buttons to generate the graph of the entire scene, but beware for massive graphs!
- Click on any node to highlight that gameobjects in your hiearchy.
Alternatively right-click on any element in the hierarchy and select ```UnityEventGraph/FindThis``` 
to highlight it in the graph. Or ```UnityEventGraph/Graph Just This``` and  ```UnityEventGraph/Graph This Hierarchy``` 
in order to create a graph starting just in this gameobject or any of its children respectively.
- From the graph you can also activate the Scene View Graph https://www.youtube.com/watch?v=IhG0LRFLmdo.
- Check the scene under /Demo 

##Who
- Original idea by SoylentGraham(https://github.com/SoylentGraham)
- Code by Luca Mefisto(https://github.com/MephestoKhaan) (myself)
- Inspired by Keijiro Takahashi(https://github.com/keijiro)
- SceneView representation by Andrés Leone(https://github.com/forestrf)
