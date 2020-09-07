# UnityFourier <img src="https://img.shields.io/badge/Version-1.0-informational" /> <img src="https://img.shields.io/badge/License-GPL--2.0-informational" /> ![.NET Core](https://github.com/francescomesianodev/UnityFourier/workflows/.NET%20Core/badge.svg)

Index
=======

<!--ts-->
   * [Introduction](#UnityFourier)
      * [What does it provide?](#what-does-it-provide)
   * [How does it work?](#how-does-it-work)
      * [Code Example](#code-example)
      * [Additional Notes](#additional-noted)
   * [Support](#support)
<!--te-->

**UnityFourier** is a <a href="https://unity.com">Unity Engine</a> C# library to allow easy audio data gathering during runtime using a single class.

### What does it provide?

**UnityFourier** provides a single class that contains data coming from the <see href="https://en.wikipedia.org/wiki/Fast_Fourier_transform">FFT theory</see> allowing to re-use it and manipulate it to achieve the results you want.

`More information can be found inside AudioPeer.cs including documentation`

### How does it work?

Before you start coding make sure that in unity you have the AudioPeer class attached to a gameobject.

#### Code Example

This simple example shows how to make a cube scale on the rythm of a frequency band:

```c#
using UnityEngine;
using UnityFourier;

public class ScalingCube : MonoBehaviour
{
  public AudioSource source;
  
  public int targetBand;        // if augmentBands is false the maximum bands would be 8, set it to true if you want more bands to use
  public float startingScale, scaleMultiplier;
  
  void Start() => AudioPeer.SharedInstance.TargetSource = source;
  
  void Update()
  {
    transform.localScale = new Vector3(transform.localScale.x, (AudioPeer.SharedInstance.FrequencyBands[targetBand] * scaleMultiplier) + startingScale, transform.localScale.z);
  }
}
```

### Additional Notes

UnityFourier **as been developed using Unity 2020.1.3f1** but it can be used with older versions too! (*min. 5.6*)

## Support

If you have any problem with UnityFourier contact me at francescomesianodev@gmail.com otherwise fill an issue.
