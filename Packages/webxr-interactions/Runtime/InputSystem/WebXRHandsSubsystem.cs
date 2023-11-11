#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER && XR_HANDS_1_1_OR_NEWER
using UnityEngine.Scripting;
using UnityEngine.XR.Hands;

namespace WebXR.InputSystem
{
  [Preserve]
  class WebXRHandsSubsystem : XRHandSubsystem
  {
    WebXRHandsProvider handsProvider => provider as WebXRHandsProvider;

    internal void UpdateHandJoints(WebXRHandData handData)
    {
      handsProvider.UpdateHandJoints(handData);
    }

    internal void SetUpdateHandsAllowed(bool allowed)
    {
      handsProvider.updateHandsAllowed = allowed;
    }

    internal void SetIsTracked(Handedness handedness, bool isTracked)
    {
      handsProvider.SetIsTracked(handedness, isTracked);
    }
  }
}
#endif