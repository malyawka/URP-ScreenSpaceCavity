# URP-ScreenSpaceCavity

<img src="/../pics/pics/g-preview.gif" width="100%" height="100%"></img>

How to preview
-----------
* Install [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/).
* Download and import the [Unity package](https://github.com/malyawka/URP-ScreenSpaceCavity/releases/tag/Unity).
* Open scene from <b>Assets/PolygonStarter/Scenes/Demo.unity</b>.

<b>Tested with</b>
Unity version - 2020.3
URP version - 10.7

Configurable parameters
-----------
<img src="/../pics/pics/params.jpg" width="100%" height="100%"></img>
* <b>Type</b>:
  * <b>Curvature</b> - highlights only the edges of objects.
  * <b>Cavity</b> - highlights the edges with the Ambient Occlusion effect.
  * <b>Both</b> - well, it's understandable :man_shrugging:.
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

Shader setup
-----------
Here is an example of parts of the code for the shader to work with Cavity:
```hlsl
#if defined (_SCREEN_SPACE_CAVITY)
  #include "CavityInput.hlsl"
#endif
```
```hlsl
#pragma multi_compile_fragment _ _SCREEN_SPACE_CAVITY
#pragma multi_compile _ _CAVITY_DEBUG
```
```hlsl
#if defined (_SCREEN_SPACE_CAVITY)
  if (_CavityEnabled)
  {
    float2 normalizedUV = GetNormalizedScreenSpaceUV(input.positionCS);
    half cavity = SampleCavity(normalizedUV);
    #ifdef _CAVITY_DEBUG
      albedo.rgb = cavity * 2.0;
    #else
      bakedGI *= cavity * 4.0;
      lightColor *= cavity * 4.0;
    #endif
  }
#endif
```

<b>The main thing is to get the cavity value and use it to apply the color:</b>
```hlsl
#include "CavityInput.hlsl"
float2 normalizedUV = GetNormalizedScreenSpaceUV(input.positionCS);
half cavity = SampleCavity(normalizedUV);
color *= cavity * 4.0;
```

Attention! A custom shader must have passes for normals and depths.

Notes
------
* As an example, use the free [POLYGON Starter Pack](https://assetstore.unity.com/packages/3d/props/polygon-starter-pack-low-poly-3d-art-by-synty-156819) asset from reputable [Synty Studios](https://assetstore.unity.com/publishers/5217).
* If you are not familiar with the Universal Render Pipeline, you can find the [official tutorial here](https://learn.unity.com/tutorial/introduction-to-urp#).
* [Writing Shaders](https://docs.unity3d.com/Manual/ShadersOverview.html).

Good to everyone!:v:
