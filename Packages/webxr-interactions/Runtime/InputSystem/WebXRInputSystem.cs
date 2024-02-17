using System;
using UnityEngine;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

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
#pragma warning disable CS0067
    public static event Action OnLeftControllerProfiles;
    public static event Action OnRightControllerProfiles;
#pragma warning restore CS0067

    private static string[] leftProfiles = null;
    private static string[] rightProfiles = null;

    public static string[] GetLeftProfiles()
    {
      return leftProfiles;
    }
    
    public static string[] GetRightProfiles()
    {
      return rightProfiles;
    }
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    private static bool initialized = false;
    private static int instances = 0;
    private static WebXRController left = null;
    private static WebXRController right = null;
    private static bool hasLeftProfiles = false;
    private static bool hasRightProfiles = false;

#if XR_HANDS_1_1_OR_NEWER
    private static WebXRHandsSubsystem webXRHandsSubsystem = null;
    private static XRHandProviderUtility.SubsystemUpdater subsystemUpdater;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void TryAutoLoad()
    {
      WebXRSettings settings = WebXRSettings.GetSettings();
      if (settings?.AutoLoadWebXRInputSystem == true)
      {
        var webxrInputSystem = new GameObject("WebXRInputSystem");
        webxrInputSystem.AddComponent<WebXRInputSystem>();
        DontDestroyOnLoad(webxrInputSystem);
      }
    }

    private void Awake()
    {
      if (initialized)
      {
        return;
      }
      initialized = true;

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
#endif
    }

    private void OnEnable()
    {
      instances++;
      if (instances > 1)
      {
        return;
      }
      unsafe
      {
        InputSystem.onDeviceCommand += HandleOnDeviceCommand;
      }
      WebXRManager.OnXRChange += OnXRChange;
      WebXRManager.OnControllerUpdate += OnControllerUpdate;
#if XR_HANDS_1_1_OR_NEWER
      WebXRManager.OnHandUpdate += OnHandUpdate;
      webXRHandsSubsystem?.Start();
      subsystemUpdater?.Start();
#endif
    }

    private void OnDisable()
    {
      instances--;
      if (instances > 0)
      {
        return;
      }
      unsafe
      {
        InputSystem.onDeviceCommand -= HandleOnDeviceCommand;
      }
      RemoveAllDevices();
      WebXRManager.OnXRChange -= OnXRChange;
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
      if (instances > 0)
      {
        return;
      }
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

    private static unsafe long? HandleOnDeviceCommand(
      UnityEngine.InputSystem.InputDevice inputDevice,
      InputDeviceCommand* command)
    {
      if (inputDevice != left && inputDevice != right)
      {
        return null;
      }
      if (command->type != InternalSendHapticImpulseCommand.Type)
      {
        return null;
      }

      var impulseCommand = *(InternalSendHapticImpulseCommand*)command;
      WebXRManager.Instance.HapticPulse(
        inputDevice == left ? WebXRControllerHand.LEFT : WebXRControllerHand.RIGHT,
        impulseCommand.amplitude,
        impulseCommand.duration * 1000f);
      return 0;
    }

    private static void OnXRChange(
      WebXRState state,
      int viewsCount, Rect leftRect, Rect rightRect)
    {
      if (state == WebXRState.NORMAL)
      {
        RemoveAllDevices();
      }
    }

    private static void RemoveAllDevices()
    {
      RemoveDevice(left);
      RemoveDevice(right);
      left = null;
      right = null;
#if XR_HANDS_1_1_OR_NEWER
      webXRHandsSubsystem?.SetUpdateHandsAllowed(false);
      DisableHandLeft();
      DisableHandRight();
#endif
    }

    private static void OnControllerUpdate(WebXRControllerData controllerData)
    {
      switch (controllerData.hand)
      {
        case 1:
          UpdateController(controllerData, ref left);
          if (!hasLeftProfiles && controllerData.profiles != null)
          {
            leftProfiles = controllerData.profiles;
            hasLeftProfiles = true;
            OnLeftControllerProfiles?.Invoke();
          }
          break;
        case 2:
          UpdateController(controllerData, ref right);
          if (!hasRightProfiles && controllerData.profiles != null)
          {
            rightProfiles = controllerData.profiles;
            hasRightProfiles = true;
            OnRightControllerProfiles?.Invoke();
          }
          break;
      }
    }

    private static void UpdateController(WebXRControllerData controllerData, ref WebXRController hand)
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
    private static void OnHandUpdate(WebXRHandData handData)
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
            new Pose(handData.pointerPosition, handData.pointerRotation),
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
            new Pose(handData.pointerPosition, handData.pointerRotation),
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

    private static void DisableHandLeft()
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
        WebXRManager.Instance.StartCoroutine(RemoveDeviceAfterFrame(device));
      }
    }

    private static void DisableHandRight()
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
        WebXRManager.Instance.StartCoroutine(RemoveDeviceAfterFrame(device));
      }
    }

    // Hack - triggers XRInputModalityManager OnDeviceChange.
    private static IEnumerator RemoveDeviceAfterFrame(UnityEngine.InputSystem.InputDevice device)
    {
      yield return null;
      InputSystem.RemoveDevice(device);
    }
#endif

    private static WebXRController GetWebXRController(int hand)
    {
      string usage = hand == 2 ? "RightHand" : "LeftHand";
      var device = InputSystem.AddDevice(
        new InputDeviceDescription
        {
          interfaceName = "XRInput",
          product = "WebXR Controller"
        });
      InputSystem.AddDeviceUsage(device, usage);
      return (WebXRController)device;
    }

    private static void RemoveDevice(TrackedDevice device)
    {
      if (device != null && device.added)
      {
        InputSystem.RemoveDevice(device);
      }
    }
#endif
  }
}
