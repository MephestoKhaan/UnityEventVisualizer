# UnityEventVisualizer

<h2>What</h2>
Have you ever come across a project that abuses linking <b>UnityEvents</b> in the inspector and now you can not find who is calling what?
<b>Unity Event Visualizer</b> is a visual tool that allows you to see all the UnityEvents in a scene at a glance and when they are being triggered. It creates a graph in which nodes are <b>gameobjects</b>, outputs are any type of UnityEvent (custom ones supported as well!) and inputs are methods.

![Animation](https://media.giphy.com/media/cA3VUiWT0FIlKebCRS/giphy.gif)

![Screenshot](https://i.gyazo.com/414775c117a536f14c2a4103202798c7.png)

<h2>How</h2>

- Grab the installer from the <b>Releases</b> section and import it into your project. Works in <b>Unity 5.6+</b>

- Select ```Windows/Events Graph Editor``` you can open the graph.
![Selector](https://media.giphy.com/media/l1J9LcPkjgvxoUsBW/giphy.gif)

- Click on any node to highlight that <b>gameobjects</b> in your hiearchy. Alternatively right-click on any element in the hierarchy and select ```UnityEventGraph/FindThis``` to highlight it in the graph.
![Finder](https://media.giphy.com/media/3ohhwhMwWW0URb8mfS/giphy.gif)

- Scene View https://www.youtube.com/watch?v=IhG0LRFLmdo.
[![Scene View preview](http://i3.ytimg.com/vi/IhG0LRFLmdo/hqdefault.jpg)](https://www.youtube.com/watch?v=IhG0LRFLmdo)

This is a work-in-progress. Pull requests welcome!

<h2>Who</h2>

- Original idea by [SoylentGraham](https://github.com/SoylentGraham)
- Code by [Luca Mefisto](https://github.com/MephestoKhaan) (myself)
- Inspired by [Keijiro Takahashi](https://github.com/keijiro)
- SceneView representation by [Andrés Leone](https://github.com/forestrf)
