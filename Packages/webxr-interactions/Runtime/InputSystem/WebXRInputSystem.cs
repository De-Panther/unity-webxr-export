using UnityEngine;
using InputDevice = UnityEngine.XR.InputDevice;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
#if XR_HANDS_1_1_OR_NEWER
using System.Collections;
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
    public Transform[] leftHandTransforms;
    public Transform[] rightHandTransforms;
    private Pose[] leftPoses;
    private Pose[] rightPoses;
    public Vector3 rightPos;
    public Vector3 leftPos;
    public Vector3 headPos;
    public int devicesType;

#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    private static bool initialized = false;
    WebXRController left = null;
    WebXRController right = null;
    WebXRHMD hmd = null;

#if XR_HANDS_1_1_OR_NEWER
    WebXRHandsSubsystem webXRHandsSubsystem = null;
    XRHandProviderUtility.SubsystemUpdater subsystemUpdater;
#endif

    private void Awake()
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
      subsystemUpdater = new XRHandProviderUtility.SubsystemUpdater(webXRHandsSubsystem);
      
      leftPoses = new Pose[leftHandTransforms.Length];
      rightPoses = new Pose[rightHandTransforms.Length];
      for (int i = 0; i < leftHandTransforms.Length; i++)
      {
        leftPoses[i] = new Pose(leftHandTransforms[i].position, leftHandTransforms[i].rotation);
        rightPoses[i] = new Pose(rightHandTransforms[i].position, rightHandTransforms[i].rotation);
      }
#endif
    }

    private void OnEnable()
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

    private void OnDisable()
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
    private void OnDestroy()
    {
      if (MetaAimHand.left != null && MetaAimHand.left.added)
      {
        InputSystem.RemoveDevice(MetaAimHand.left);
        MetaAimHand.left = null;
      }
      if (MetaAimHand.right != null && MetaAimHand.right.added)
      {
        InputSystem.RemoveDevice(MetaAimHand.right);
        MetaAimHand.right = null;
      }
      webXRHandsSubsystem?.Destroy();
      subsystemUpdater?.Destroy();
      webXRHandsSubsystem = null;
      subsystemUpdater = null;
    }
#endif

    /*
    private void Update1()
    {
      OnHeadsetUpdate(
        Matrix4x4.identity,
        Matrix4x4.identity,
        Quaternion.identity,
        Quaternion.identity,
        headPos,
        headPos
      );
      WebXRHandData handDataLeft = new();
      handDataLeft.frame = Time.frameCount;
      handDataLeft.enabled = devicesType == 2;
      handDataLeft.hand = 1;
      handDataLeft.trigger = 1;
      for (int i = 0; i < handDataLeft.joints.Length; i++)
      {
        handDataLeft.joints[i].radius = 0.01f;
        handDataLeft.joints[i].position = leftPoses[i].position;
        handDataLeft.joints[i].rotation = leftPoses[i].rotation;
        handDataLeft.joints[i].position += leftPos;
      }
      OnHandUpdate(handDataLeft);
      WebXRHandData handDataRight = new();
      handDataRight.frame = Time.frameCount;
      handDataRight.enabled = devicesType == 2;
      handDataRight.hand = 2;
      handDataRight.trigger = 1;
      for (int i = 0; i < handDataRight.joints.Length; i++)
      {
        handDataRight.joints[i].radius = 0.01f;
        handDataRight.joints[i].position = rightPoses[i].position;
        handDataRight.joints[i].rotation = rightPoses[i].rotation;
        handDataRight.joints[i].position += rightPos;
      }
      OnHandUpdate(handDataRight);

      WebXRControllerData controllerDataLeft = new WebXRControllerData();
      controllerDataLeft.frame = Time.frameCount;
      controllerDataLeft.enabled = devicesType == 1;
      controllerDataLeft.hand = 1;
      controllerDataLeft.position = leftPos;
      controllerDataLeft.rotation = Quaternion.identity;
      controllerDataLeft.gripPosition = leftPos;
      controllerDataLeft.gripRotation = Quaternion.identity;
      OnControllerUpdate(controllerDataLeft);
      WebXRControllerData controllerDataRight = new WebXRControllerData();
      controllerDataRight.frame = Time.frameCount;
      controllerDataRight.enabled = devicesType == 1;
      controllerDataRight.hand = 2;
      controllerDataRight.position = rightPos;
      controllerDataRight.rotation = Quaternion.identity;
      controllerDataRight.gripPosition = rightPos;
      controllerDataRight.gripRotation = Quaternion.identity;
      OnControllerUpdate(controllerDataRight);
    }
    */

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
      webXRHandsSubsystem?.SetUpdateHandsAllowed(false);
      DisableHandLeft();
      DisableHandRight();
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
        // Must wait one update after creating controller.
        if (hand == null)
        {
          hand = GetWebXRController(controllerData.hand);
        }
        else
        {
          hand.OnControllerUpdate(controllerData);
        }
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
          MetaAimHand.left ??= MetaAimHand.CreateHand(InputDeviceCharacteristics.Left);
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
          MetaAimHand.right ??= MetaAimHand.CreateHand(InputDeviceCharacteristics.Right);
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
          DisableHandLeft();
        }
        else
        {
          DisableHandRight();
        }
      }
    }

    void DisableHandLeft()
    {
      if (MetaAimHand.left != null
          && MetaAimHand.left.added
          && MetaAimHand.left.aimFlags.value != 0)
      {
        MetaAimHand.left.UpdateHand(
          false,
          MetaAimFlags.None,
          Pose.identity,
          0,
          0,
          0,
          0);
        // Hack - triggers XRInputModalityManager OnDeviceChange.
        var device = InputSystem.AddDevice("XRController");
        InputSystem.AddDeviceUsage(device, "LeftHand");
        StartCoroutine(RemoveDeviceAfterFrame(device));
      }
    }

    void DisableHandRight()
    {
      if (MetaAimHand.right != null
          && MetaAimHand.right.added
          && MetaAimHand.right.aimFlags.value != 0)
      {
        MetaAimHand.right.UpdateHand(
          false,
          MetaAimFlags.None,
          Pose.identity,
          0,
          0,
          0,
          0);
        // Hack - triggers XRInputModalityManager OnDeviceChange.
        var device = InputSystem.AddDevice("XRController");
        InputSystem.AddDeviceUsage(device, "RightHand");
        StartCoroutine(RemoveDeviceAfterFrame(device));
      }
    }

    // Hack - triggers XRInputModalityManager OnDeviceChange.
    IEnumerator RemoveDeviceAfterFrame(UnityEngine.InputSystem.InputDevice device)
    {
      yield return null;
      InputSystem.RemoveDevice(device);
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
      string product = "WebXRController Left";
      string usage = "LeftHand";
      if (hand == 2)
      {
        product = "WebXRController Right";
        usage = "RightHand";
      }
      var device = InputSystem.AddDevice(
        new InputDeviceDescription
        {
          interfaceName = "WebXRController",
          product = product
        });
      InputSystem.AddDeviceUsage(device, usage);
      return (WebXRController)device;
    }

    private void RemoveDevice(TrackedDevice device)
    {
      if (device != null && device.added)
      {
        InputSystem.RemoveDevice(device);
      }
    }
#endif
  }
}
