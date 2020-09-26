using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TestXR : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
    var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
    SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
    Debug.LogError("Getting xrDisplaySubsystems " + xrDisplaySubsystems.Count);
    foreach (var xrDisplay in xrDisplaySubsystems)
    {
      Debug.LogError($"xrDisplay running {xrDisplay.running}");
    }
  }
}
