using UnityEngine;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
#if XR_HANDS_1_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;
#endif
#endif

namespace WebXR.InputSystem
{
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
  using InputSystem = UnityEngine.InputSystem.InputSystem;
#endif
  public class WebXRInputSystem : MonoBehaviour
  {
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    private static bool initialized = false;
    WebXRController left = null;
    WebXRController right = null;
    WebXRHMD hmd = null;

#if XR_HANDS_1_1_OR_NEWER
    WebXRHandsSubsystem webXRHandsSubsystem = null;
    XRHandProviderUtility.SubsystemUpdater subsystemUpdater;
#endif

    void Awake()
    {
      if (initialized)
      {
        return;
      }
      initialized = true;
      InputSystem.RegisterLayout<WebXRController>(
        matches: new InputDeviceMatcher()
        .WithInterface("WebXRController"));
      InputSystem.RegisterLayout<WebXRHMD>(
        matches: new InputDeviceMatcher()
        .WithInterface("WebXRHMD"));
#if XR_HANDS_1_1_OR_NEWER
      var descriptors = new List<XRHandSubsystemDescriptor>();
      SubsystemManager.GetSubsystemDescriptors(descriptors);
      for (var i = 0; i < descriptors.Count; ++i)
      {
        var descriptor = descriptors[i];
        if (descriptor.id == WebXRHandsProvider.id)
        {
          webXRHandsSubsystem = descriptor.Create() as WebXRHandsSubsystem;
          break;
        }
      }
      webXRHandsSubsystem?.SetUpdateHandsAllowed(true);
      subsystemUpdater = new XRHandProviderUtility.SubsystemUpdater(webXRHandsSubsystem);
#endif
    }

    protected void OnEnable()
    {
      WebXRManager.OnXRChange += OnXRChange;
      WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
      WebXRManager.OnControllerUpdate += OnControllerUpdate;
#if XR_HANDS_1_1_OR_NEWER
      WebXRManager.OnHandUpdate += OnHandUpdate;
      webXRHandsSubsystem?.Start();
      subsystemUpdater?.Start();
#endif
    }

    protected void OnDisable()
    {
      RemoveAllDevices();
      WebXRManager.OnXRChange -= OnXRChange;
      WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
      WebXRManager.OnControllerUpdate -= OnControllerUpdate;
#if XR_HANDS_1_1_OR_NEWER
      WebXRManager.OnHandUpdate += OnHandUpdate;
      webXRHandsSubsystem?.Stop();
      subsystemUpdater?.Stop();
#endif
    }

#if XR_HANDS_1_1_OR_NEWER
    void OnDestroy()
    {
      webXRHandsSubsystem?.Destroy();
      subsystemUpdater?.Destroy();
      webXRHandsSubsystem = null;
      subsystemUpdater = null;
    }
#endif

    private void OnXRChange(
      WebXRState state,
      int viewsCount, Rect leftRect, Rect rightRect)
    {
      if (state == WebXRState.NORMAL)
      {
        RemoveAllDevices();
      }
    }

    private void RemoveAllDevices()
    {
      RemoveDevice(left);
      RemoveDevice(right);
      RemoveDevice(hmd);
      left = null;
      right = null;
      hmd = null;
#if XR_HANDS_1_1_OR_NEWER
      DestroyHandLeft();
      DestroyHandRight();
      webXRHandsSubsystem?.SetUpdateHandsAllowed(false);
#endif
    }

    private void OnHeadsetUpdate(
        Matrix4x4 leftProjectionMatrix,
        Matrix4x4 rightProjectionMatrix,
        Quaternion leftRotation,
        Quaternion rightRotation,
        Vector3 leftPosition,
        Vector3 rightPosition)
    {
      SetWebXRHMD();
      Vector3 devicePosition = leftPosition;
      Quaternion deviceRotation = leftRotation;
      if (WebXRManager.Instance.ViewsCount == 2)
      {
        devicePosition = (leftPosition + rightPosition) * 0.5f;
      }
      hmd.OnHeadsetUpdate(
        devicePosition, deviceRotation,
        leftRotation, rightRotation,
        leftPosition, rightPosition);
    }

    private void OnControllerUpdate(WebXRControllerData controllerData)
    {
      switch (controllerData.hand)
      {
        case 1:
          UpdateController(controllerData, ref left);
          break;
        case 2:
          UpdateController(controllerData, ref right);
          break;
      }
    }

    private void UpdateController(WebXRControllerData controllerData, ref WebXRController hand)
    {
      if (controllerData.enabled)
      {
        if (hand == null)
        {
          hand = GetWebXRController(controllerData.hand);
        }
        hand.OnControllerUpdate(controllerData);
      }
      else if (hand != null)
      {
        InputSystem.RemoveDevice(hand);
        hand = null;
      }
    }

#if XR_HANDS_1_1_OR_NEWER
    private void OnHandUpdate(WebXRHandData handData)
    {
      webXRHandsSubsystem?.SetIsTracked((Handedness)handData.hand, handData.enabled);
      if (handData.enabled)
      {
        webXRHandsSubsystem?.SetUpdateHandsAllowed(true);
        webXRHandsSubsystem?.UpdateHandJoints(handData);
        MetaAimFlags aimFlags = MetaAimFlags.Computed | MetaAimFlags.Valid;
        if (handData.trigger > MetaAimHand.pressThreshold)
        {
          aimFlags |= MetaAimFlags.IndexPinching;
        }
        if (handData.hand == 1)
        {
          CreateHandLeft();
          MetaAimHand.left.UpdateHand(
            true,
            aimFlags,
            new Pose(handData.joints[10].position, handData.joints[10].rotation),
            handData.trigger,
            0,
            0,
            0);
        }
        else
        {
          CreateHandRight();
          MetaAimHand.right.UpdateHand(
            true,
            aimFlags,
            new Pose(handData.joints[10].position, handData.joints[10].rotation),
            handData.trigger,
            0,
            0,
            0);
        }
      }
      else
      {
        if (handData.hand == 1)
        {
          DestroyHandLeft();
        }
        else
        {
          DestroyHandRight();
        }
      }
    }

    void CreateHandLeft()
    {
      MetaAimHand.left ??= MetaAimHand.CreateHand(InputDeviceCharacteristics.Left);
    }

    void CreateHandRight()
    {
      MetaAimHand.right ??= MetaAimHand.CreateHand(InputDeviceCharacteristics.Right);
    }

    void DestroyHandLeft()
    {
      if (MetaAimHand.left != null)
      {
        InputSystem.RemoveDevice(MetaAimHand.left);
        MetaAimHand.left = null;
      }
    }

    void DestroyHandRight()
    {
      if (MetaAimHand.right != null)
      {
        InputSystem.RemoveDevice(MetaAimHand.right);
        MetaAimHand.right = null;
      }
    }
#endif

    private void SetWebXRHMD()
    {
      if (hmd != null)
      {
        return;
      }
      hmd = (WebXRHMD)InputSystem.AddDevice(
        new InputDeviceDescription
        {
          interfaceName = "WebXRHMD",
          product = "WebXRHMD"
        });
    }

    private WebXRController GetWebXRController(int hand)
    {
      string name = "WebXRController Left";
      string usage = "LeftHand";
      if (hand == 2)
      {
        name = "WebXRController Right";
        usage = "RightHand";
      }
      var device = InputSystem.AddDevice(
        new InputDeviceDescription
        {
          interfaceName = "WebXRController",
          product = name
        });
      InputSystem.AddDeviceUsage(device, usage);
      return (WebXRController)device;
    }

    private void RemoveDevice(TrackedDevice device)
    {
      if (device != null)
      {
        InputSystem.RemoveDevice(device);
      }
    }
#endif
  }
}
