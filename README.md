# UnityEventVisualizer

[![openupm](https://img.shields.io/npm/v/com.mefistofiles.unity-event-visualizer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.mefistofiles.unity-event-visualizer/)


Now also available at the [Unity asset store](https://assetstore.unity.com/packages/tools/utilities/event-visualizer-163380)! (free, reviews appreciated)

<h2>What</h2>
Have you ever come across a project that abuses linking <b>UnityEvents</b> in the inspector and now you can not find who is calling what?
<b>Unity Event Visualizer</b> is a visual tool that allows you to see all the UnityEvents in a scene at a glance and when they are being triggered. It creates a graph in which nodes are <b>gameobjects</b>, outputs are any type of UnityEvent (custom ones supported as well!) and inputs are methods.

![Animation](https://media.giphy.com/media/cA3VUiWT0FIlKebCRS/giphy.gif)

![SceneView](https://media.giphy.com/media/AFvTp2k8L5R1pKXJZA/giphy.gif)

<h2>Install</h2>

**Via OpenUPM**

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.mefistofiles.unity-event-visualizer
```

**Via Git URL**

Open Packages/manifest.json with your favorite text editor. Add the following line to the dependencies block.

```
"com.mefistofiles.unity-event-visualizer": "https://github.com/MephestoKhaan/UnityEventVisualizer.git"
```

**Via .unitypackage**

Grab the installer from the <b>Releases</b> section and import it into your project.

**Via Unity asset store**

Download it directly from the [Unity asset store](https://assetstore.unity.com/packages/tools/utilities/event-visualizer-163380).


<h2>How</h2>

- Select ```Windows/Events Graph Editor``` you can open the graph.
![Selector](https://media.giphy.com/media/l1J9LcPkjgvxoUsBW/giphy.gif)
- Select any root gameobject(s) and click on ```Rebuild on selected hierarchy``` to generate a graph
of all events being fired by the selected hierarchy, or  ```Rebuild JUST selected``` to generate a
graph of all events being fired by exactly the selected gameobjects. You can deselect everything and 
click any of the buttons to generate the graph of the entire scene, but beware for massive graphs!



- Click on any node to highlight that <b>gameobjects</b> in your hiearchy. 
Alternatively right-click on any element in the hierarchy and select ```UnityEventGraph/FindThis``` 
to highlight it in the graph. Or ```UnityEventGraph/Graph Just This``` and  ```UnityEventGraph/Graph This Hierarchy``` 
in order to create a graph starting just in this gameobject or any of its children respectively.
![Finder](https://media.giphy.com/media/3ohhwhMwWW0URb8mfS/giphy.gif)

- Scene View https://www.youtube.com/watch?v=IhG0LRFLmdo.
[![Scene View preview](http://i3.ytimg.com/vi/IhG0LRFLmdo/hqdefault.jpg)](https://www.youtube.com/watch?v=IhG0LRFLmdo)

Pull requests welcome!

<h2>Who</h2>

- Original idea by [SoylentGraham](https://github.com/SoylentGraham)
- Code by [Luca Mefisto](https://github.com/MephestoKhaan) (myself)
- Inspired by [Keijiro Takahashi](https://github.com/keijiro)
- SceneView representation by [Andrés Leone](https://github.com/forestrf)
