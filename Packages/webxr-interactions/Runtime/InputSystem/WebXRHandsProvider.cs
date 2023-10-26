#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER && XR_HANDS_1_1_OR_NEWER
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace WebXR.InputSystem
{
  [Preserve]
  class WebXRHandsProvider : XRHandSubsystemProvider
  {
    public static string id { get; }

    public bool updateHandsAllowed { get; set; } = true;

    private static Vector3 MIDDLE_METACARPAL_TO_PALM = new Vector3(0, 0.006376f, 0.013537f);

    [Preserve]
    private WebXRHandData leftHandData = new WebXRHandData();
    [Preserve]
    private WebXRHandData rightHandData = new WebXRHandData();

    static WebXRHandsProvider() => id = "WebXR Hands Provider";

    public override void Start()
    {
    }

    public override void Stop()
    {
    }

    public override void Destroy()
    {
    }

    public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
    {
      handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;

      handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;

      handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;

      handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;

      handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;

      handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
      handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;
    }

    public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(XRHandSubsystem.UpdateType updateType,
        ref Pose leftHandRootPose, NativeArray<XRHandJoint> leftHandJoints,
        ref Pose rightHandRootPose, NativeArray<XRHandJoint> rightHandJoints)
    {
      if (!updateHandsAllowed)
        return XRHandSubsystem.UpdateSuccessFlags.None;

      UpdateData(Handedness.Left, leftHandData, leftHandJoints, ref leftHandRootPose);

      UpdateData(Handedness.Right, rightHandData, rightHandJoints, ref rightHandRootPose);

      var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;
      if (leftHandData.enabled)
        successFlags |= XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;

      if (rightHandData.enabled)
        successFlags |= XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
      return successFlags;
    }

    [Preserve]
    void UpdateData(Handedness handedness, WebXRHandData handData, NativeArray<XRHandJoint> handJointArray, ref Pose rootPose)
    {
      rootPose = new Pose(handData.joints[0].position, handData.joints[0].rotation);
      for (int jointIndex = 0; jointIndex < handJointArray.Length; ++jointIndex)
      {
        if (jointIndex == 1)
        {
          UpdatePalmJoint(jointIndex, handedness, handData, handJointArray);
          continue;
        }
        UpdateJoint(jointIndex, Mathf.Max(0, jointIndex - 1), handedness, handData, handJointArray);
      }
    }

    private void UpdateJoint(int jointIndex, int webxrJointIndex, Handedness handedness, WebXRHandData handData, NativeArray<XRHandJoint> handJointArray)
    {
      handJointArray[jointIndex].TryGetPose(out var pose);
      pose.position = handData.joints[webxrJointIndex].position;
      pose.rotation = handData.joints[webxrJointIndex].rotation;

      handJointArray[jointIndex] = XRHandProviderUtility.CreateJoint(
          handedness,
          XRHandJointTrackingState.Pose | XRHandJointTrackingState.Radius,
          XRHandJointIDUtility.FromIndex(jointIndex),
          pose,
          handData.joints[webxrJointIndex].radius);
    }

    private void UpdatePalmJoint(int jointIndex, Handedness handedness, WebXRHandData handData, NativeArray<XRHandJoint> handJointArray)
    {
      int webxrJointIndex = 10; // Middle finger metacarpal
      handJointArray[jointIndex].TryGetPose(out var pose);
      pose.position = handData.joints[webxrJointIndex].position;
      pose.rotation = handData.joints[webxrJointIndex].rotation;
      pose.position += pose.rotation * MIDDLE_METACARPAL_TO_PALM;

      handJointArray[jointIndex] = XRHandProviderUtility.CreateJoint(
          handedness,
          XRHandJointTrackingState.Pose | XRHandJointTrackingState.Radius,
          XRHandJointIDUtility.FromIndex(jointIndex),
          pose,
          handData.joints[webxrJointIndex].radius);
    }

    public void UpdateHandJoints(WebXRHandData handData)
    {
      if (handData.hand == 1)
      {
        leftHandData = handData;
      }
      else
      {
        rightHandData = handData;
      }
    }

    public void SetIsTracked(Handedness handedness, bool isTracked)
    {
      if (handedness == Handedness.Invalid)
        return;

      var handState = handedness == Handedness.Left ? leftHandData : rightHandData;
      handState.enabled = isTracked;

      if (!leftHandData.enabled && !rightHandData.enabled)
      {
        updateHandsAllowed = false;
      }
    }

    [Preserve, RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Register()
    {
      var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
      {
        id = id,
        providerType = typeof(WebXRHandsProvider),
        subsystemTypeOverride = typeof(WebXRHandsSubsystem),
      };
      XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
    }
  }
}
#endif
