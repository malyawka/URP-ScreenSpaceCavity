# URP-ScreenSpaceCavity

<b>Blender Cavity Effect for Unity</b>

<img src="/../pics/pics/g-preview.gif" width="50%" height="50%">

How to preview:
-----------
* Install [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/).
* Download and import the [Unity package](https://github.com/malyawka/URP-ScreenSpaceCavity/releases/tag/Unity), or clone this repository.
* Open scene from <b>Assets/PolygonStarter/Scenes/Demo.unity</b>.

Configurable parameters:
-----------
<br> <img src="/../pics/pics/params.jpg" width="50%" height="50%">
* <b>Type</b>:
  * <b>Curvature</b> - highlights only the edges of objects.
  * <b>Cavity</b> - highlights the edges with the Ambient Occlusion effect.
  * <b>Both</b> - well, it's understandable 🤷‍♂️
* <b>Curvature</b>:
  * <b>Scale</b> - effect width.
  * <b>Ridge</b> - effect ntensivity for ridge (white).
  * <b>Valley</b> - effect ntensivity for valley (black).
* <b>Cavity</b>:
  * <b>Distance</b> - distance of effect from edge.
  * <b>Attenuation</b> - fading out the effect relative to the camera (relevant for nearby objects).
  * <b>Ridge</b> - effect ntensivity for ridge (white).
  * <b>Valley</b> - effect ntensivity for valley (black).
  * <b>Samples</b> - number of passes to calculate the effect.

