#EventVisualizer

##What
Have you ever come across a project that abuses linking UnityEvents in the inspector and now you can
not find who is calling what?
Event Visualizer is a visual tool that allows you to see all the UnityEvents in a scene at
a glance and when they are being triggered. It creates a graph in which nodes are gameobjects,
outputs are any type of UnityEvent (custom ones supported as well!) and inputs are methods.

##How

- Select ```Windows/Events Graph Editor``` you can open the graph.
- Click on any node to highlight that gameobjects in your hiearchy.
Alternatively right-click on any element in the hierarchy and select ```UnityEventGraph/FindThis``` 
to highlight it in the graph.
- From the graph you can also activate the Scene View Graph https://www.youtube.com/watch?v=IhG0LRFLmdo.
- Check the scene under /Demo 

##Who
- Original idea by SoylentGraham(https://github.com/SoylentGraham)
- Code by Luca Mefisto(https://github.com/MephestoKhaan) (myself)
- Inspired by Keijiro Takahashi(https://github.com/keijiro)
- SceneView representation by Andrés Leone(https://github.com/forestrf)
