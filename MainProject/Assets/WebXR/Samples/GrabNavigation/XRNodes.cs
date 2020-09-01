using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class XRNodes : MonoBehaviour
{

    private List<InputDevice> triggers;
    List<XRNodeState> nodeStates = new List<XRNodeState>();

    private void OnEnable()
    {
        triggers = new List<InputDevice>();
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;

    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        triggers.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        if (device.TryGetFeatureValue(CommonUsages.trigger, out var _) &&
            device.TryGetFeatureValue(CommonUsages.devicePosition, out var __) &&
            device.TryGetFeatureValue(CommonUsages.deviceRotation, out var ___))
            triggers.Add(device);
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (triggers.Contains(device))
            triggers.Remove(device);
    }

    public Transform leftHand, rightHand;
    public float leftTrigger, rightTrigger;
    public float leftGrip, rightGrip;

    // Update is called once per frame
    void Update()
    {
        var lefts = triggers.Where(x => x.characteristics.HasFlag(InputDeviceCharacteristics.Left)).FirstOrDefault();
        var rights = triggers.Where(x => x.characteristics.HasFlag(InputDeviceCharacteristics.Right)).FirstOrDefault();

        lefts.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);
        lefts.TryGetFeatureValue(CommonUsages.grip, out leftGrip);
        lefts.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPos);
        lefts.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRot);
        lefts.TryGetFeatureValue(CommonUsages.isTracked, out var leftIsTracked);

        rights.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);
        rights.TryGetFeatureValue(CommonUsages.grip, out rightGrip);
        rights.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPos);
        rights.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRot);
        rights.TryGetFeatureValue(CommonUsages.isTracked, out var rightIsTracked);

        // Debug.Log("left: " + leftTrigger + ", right: " + rightTrigger + "lp:" + leftPos + "rp: " + rightPos);

        var leftPosChanged = Vector3.Distance(leftHand.localPosition, leftPos) > 0.0005f;
        var rightPosChanged = Vector3.Distance(rightHand.localPosition, rightPos) > 0.0005f;
        var leftRotChanged = Quaternion.Angle(leftHand.localRotation, leftRot) > 0.0001f;
        var rightRotChanged = Quaternion.Angle(rightHand.localRotation, rightRot) > 0.0001f;

        leftHand.localPosition = leftPos;
        leftHand.localRotation = leftRot;
        rightHand.localPosition = rightPos;
        rightHand.localRotation = rightRot;

        // TODO fade hands based on controller activity
        leftHand.gameObject.SetActive(leftIsTracked); // && (leftPosChanged || leftRotChanged));
        rightHand.gameObject.SetActive(rightIsTracked); // && (rightPosChanged || rightRotChanged));

        //var str = "";
        //InputTracking.GetNodeStates(nodeStates);
        //foreach(var state in nodeStates)
        //{
        //    state.TryGetPosition(out var pos);
        //    state.TryGetRotation(out var rot);
        //    str += state.nodeType + ": " + state.tracked + ", " + pos + ", " + rot;
        //}

        //Debug.Log(str);
    }
}
