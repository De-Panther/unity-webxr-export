using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabNavigation : MonoBehaviour {

    public class Hand : Transform { }

    public Transform leftHand, rightHand;
    [UnityEngine.Serialization.FormerlySerializedAs("steamRig")]
    public Transform vrRoot;
    
    public Transform fallbackRig;
    Transform lastLeft, lastRight;

    //public bool allowPosition = true, allowRotation = true;
    public bool horizonLock = true;
    public bool invertMotion = false;
    public bool useAcceleration = false;
    [Tooltip("Should be 1 at slow velocities, and much higher at higher velocities")]
    public AnimationCurve moveAcceleration;

    [Header("Settings")]
    public bool allowGrips = true;
    public bool allowIndexs = false;
    private bool needsThumbRest = false;
    public bool movementOnlyInXZ = false;

    // [Header("Output")]
    private bool leftHandGrip;
    private bool rightHandGrip;
    private bool leftTrigger, rightTrigger;
    private bool leftThumbRest, rightThumbRest;
    private bool leftGrip, rightGrip;
    private float leftVelocity, rightVelocity;

    Vector3 defaultPos;
    Quaternion defaultRot;
    Vector3 defaultScale;

    Vector3 defaultFallbackPos;
    Quaternion defaultFallbackRot;
    Vector3 defaultFallbackScale;

    private void Start()
    {
        defaultPos = vrRoot.position;
        defaultRot = vrRoot.rotation;
        defaultScale = vrRoot.localScale;

        if(fallbackRig != null)
        { 
            defaultFallbackPos = fallbackRig.position;
            defaultFallbackRot = fallbackRig.rotation;
            defaultFallbackScale = fallbackRig.localScale;
        }
    }

    public void ResetToDefaults()
    {
        vrRoot.position = defaultPos;
        vrRoot.rotation = defaultRot;
        vrRoot.localScale = defaultScale;

        fallbackRig.position = defaultFallbackPos;
        fallbackRig.rotation = defaultFallbackRot;
        fallbackRig.localScale = defaultFallbackScale;
    }

    // Update is called once per frame
    void Update ()
    {
        #region OCULUS
        //leftHandGrip  = allowGrips && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.5f;
        //rightHandGrip = allowGrips && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.5f;

        //leftThumbRest  = !needsThumbRest || OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons | OVRInput.NearTouch.SecondaryThumbButtons, OVRInput.Controller.LTouch);
        //rightThumbRest = !needsThumbRest || OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons | OVRInput.NearTouch.SecondaryThumbButtons, OVRInput.Controller.RTouch);

        //leftTrigger  = allowIndexs && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.5f;
        //rightTrigger = allowIndexs && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5f;
        #endregion

        #region VIVE
        //left = player.leftController;
        //right = player.rightController;

        //if (left != null)
        //    leftGrip = left.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        //else
        //    leftGrip = false;

        //if (right != null)
        //    rightGrip = right.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        //else
        //    rightGrip = false;

        // make sure these hands are not grabbing something right now
        #endregion

        #region
        var nodes = GetComponent<XRNodes>();
        leftHandGrip = allowGrips && nodes.leftGrip > 0.5f;
        rightHandGrip = allowGrips && nodes.rightGrip > 0.5f;

        leftTrigger = allowIndexs && nodes.leftTrigger > 0.5f;
        rightTrigger = allowIndexs && nodes.rightTrigger > 0.5f;
        #endregion

        //leftGrip    = leftGrabber.grabbedObject == null  && (leftHandGrip  || (leftTrigger && leftThumbRest));
        //rightGrip   = rightGrabber.grabbedObject == null && (rightHandGrip || (rightTrigger && rightThumbRest));

        leftGrip = leftHand.gameObject.activeInHierarchy && (leftHandGrip || leftTrigger);
        rightGrip = rightHand.gameObject.activeInHierarchy && (rightHandGrip || rightTrigger);

        var anyGrip = leftGrip || rightGrip;
        var bothGrip = leftGrip && rightGrip;
        
        if(anyGrip && !bothGrip)
        {
            HandleSingleGrip(leftGrip ? leftHand : rightHand);
        }
        else if(bothGrip)
        {
            HandleBothGrip(leftHand, rightHand);
        }

        // cleanup

        if (lastLeft != null && !leftGrip && activeHands.ContainsKey(lastLeft)) { 
            activeHands.Remove(lastLeft);
        }
        if (lastRight != null && !rightGrip && activeHands.ContainsKey(lastRight)) { 
            activeHands.Remove(lastRight);
        }

        lastLeft = leftHand;
        lastRight = rightHand;

        if (lastLeft != null && activeHands.ContainsKey(lastLeft))
            leftVelocity = activeHands[lastLeft].velocity;
        if (lastRight != null && activeHands.ContainsKey(lastRight))
            rightVelocity = activeHands[lastRight].velocity;
    }

    
    static Vector3 RelativePosition(Transform parent, Transform t)
    {
        if (parent == null) return t.localPosition;

        // return t.localPosition;
        return parent.InverseTransformPoint(t.position);
    }

    static Quaternion RelativeRotation(Transform parent, Transform t)
    {
        if (parent == null) return t.localRotation;

        return Quaternion.Inverse(parent.rotation) * t.rotation;
    }

    public class TransformData
    {
        public Vector3 relativePos;
        public Quaternion relativeRot;

        Vector3 lastRelativePos;
        Quaternion lastRelativeRot;

        Transform parent, source;
        
        public float velocity => positionOffset.magnitude / 0.011f; // HACK fixed time to prevent jitter movement
        public Vector3 positionOffset => lastRelativePos - relativePos;
        public Quaternion rotationOffset => Quaternion.Inverse(lastRelativeRot) * relativeRot;

        //public Quaternion GetRotationOffset(Transform parent)
        //{
        //    return Quaternion.Inverse(parent.TransformRotation(relativeRot)) * parent.TransformRotation(lastRelativeRot);
        //}

        public TransformData(Transform parent, Transform source)
        {
            this.parent = parent;
            this.source = source;

            Update(true);
        }

        public void Update(bool reset = false)
        {
            if(!reset)
            { 
                lastRelativePos = relativePos;
                lastRelativeRot = relativeRot;
            }

            relativePos = RelativePosition(parent, source);
            relativeRot = RelativeRotation(parent, source);

            if(reset)
            {
                lastRelativePos = relativePos;
                lastRelativeRot = relativeRot;
            }
        }
    }

    Dictionary<Transform, TransformData> activeHands = new Dictionary<Transform, TransformData>();


    //public Transform tmpTransform;
    //public Transform head;

    Vector3 targetMovement;
    Quaternion targetRotation;

    void HandleSingleGrip(Transform hand)
    {
        if(!activeHands.ContainsKey(hand))
        {
            activeHands.Add(hand, new TransformData(vrRoot, hand.transform));
        }

        var data = activeHands[hand];

        data.Update();

        //if (allowPosition)
        //{
        if(!movementOnlyInXZ)
            vrRoot.position += vrRoot.TransformDirection(data.positionOffset * (useAcceleration ? moveAcceleration.Evaluate(data.velocity) : 1) * vrRoot.localScale.x) * (invertMotion ? -1 : 1);
        else
        {
            Vector3 pos = vrRoot.position + vrRoot.TransformDirection(data.positionOffset * (useAcceleration ? moveAcceleration.Evaluate(data.velocity) : 1) * vrRoot.localScale.x) * (invertMotion ? -1 : 1);
            pos.y = vrRoot.position.y;
            vrRoot.position = pos;
        }

        //}

        //if (allowRotation)
        //{
        //    var prevHandPos = hand.transform.position;
        //    steamRig.rotation *= data.GetRotationOffset(head);
        //    var postHandPos = hand.transform.position;
        //    Debug.DrawLine(prevHandPos, postHandPos, Color.green);
        //    steamRig.position += prevHandPos - postHandPos;
        //}
    }
    
    Quaternion FromToRotation(Vector3 v2, Vector3 v1, float multiplier)
    { 
        var cross = Vector3.Cross(v2, v1);
        return Quaternion.AngleAxis(-Vector3.SignedAngle(v1, v2, cross) * multiplier, Vector3.Cross(v2, v1));
    }

    Quaternion lastAppliedRotation; 
    void HandleBothGrip(Transform a, Transform b)
    {
        var isFirstFrame = false;
        if (!activeHands.ContainsKey(a))
        {
            activeHands.Add(a, new TransformData(vrRoot, a.transform));
            isFirstFrame = true;
        }
        if (!activeHands.ContainsKey(b))
        {
            activeHands.Add(b, new TransformData(vrRoot, b.transform));
            isFirstFrame = true;
        }

        var dataA = activeHands[a];
        var dataB = activeHands[b];

        dataA.Update();
        dataB.Update();

        if (movementOnlyInXZ)
            return;

        vrRoot.position += vrRoot.TransformDirection((dataA.positionOffset + dataB.positionOffset) / 2 /** moveAcceleration.Evaluate((dataA.velocity + dataB.velocity) / 2)*/ * vrRoot.localScale.x);

        var prevCenter = (a.transform.position + b.transform.position) / 2;
        
        var axis1 = vrRoot.InverseTransformPoint(b.transform.position) - vrRoot.InverseTransformPoint(a.transform.position);

        var axis2 = Vector3.Cross(axis1, vrRoot.InverseTransformDirection(b.transform.forward));
        var axis3 = Vector3.Cross(axis1, axis2);
        axis2 = Vector3.Cross(axis1, axis3);

        Debug.DrawRay(b.transform.position, axis1, Color.blue);
        Debug.DrawRay(b.transform.position, axis2, Color.green);
        Debug.DrawRay(b.transform.position, axis3, Color.red);
        
        if (horizonLock)
        {
            axis1.y = 0;
            lastAxis1.y = 0;

            axis2.y = 0;
            lastAxis2.y = 0;

            axis3.y = 0;
            lastAxis3.y = 0;
        }

        var rotOffset1 = Quaternion.FromToRotation(axis1, lastAxis1);
        var rotOffset2 = Quaternion.FromToRotation(axis2, lastAxis2);
        var rotOffset3 = Quaternion.FromToRotation(axis3, lastAxis3);
        
        // rotOffset = Quaternion.Inverse(q1) * q2;
        //var rotOffset = Quaternion.RotateTowards(q1, q2, 10f);

        if (!isFirstFrame)
        {
            // find "shortest" rotation
            float _angle1 = 0f, _angle2 = 0f, _angle3 = 0f;
            Vector3 _axis1 = Vector3.zero, _axis2 = Vector3.zero, _axis3 = Vector3.zero;

            rotOffset1.ToAngleAxis(out _angle1, out _axis1);
            rotOffset2.ToAngleAxis(out _angle2, out _axis2);
            rotOffset3.ToAngleAxis(out _angle3, out _axis3);

            //__angle1 = _angle1;
            //__angle2 = _angle2;
            //__angle3 = _angle3;

            // Quaternion minRotOffset;
            /*
            if(_angle1 < _angle2)
            {
                if(_angle1 < _angle3)
                {
                    minRotOffset = rotOffset1;
                }
                else
                {
                    minRotOffset = rotOffset3;
                }
            }
            else
            {
                if(_angle2 < _angle3)
                {
                    minRotOffset = rotOffset2;
                }
                else
                {
                    minRotOffset = rotOffset3;
                }
            }
            */

            if(_angle1 < 90)
                vrRoot.rotation *= rotOffset1;

            var scale = (lastAxis1.magnitude / axis1.magnitude);
            vrRoot.localScale *= scale;

            var postCenter = (a.transform.position + b.transform.position) / 2;
            vrRoot.position += prevCenter - postCenter;
        }

        lastAxis1 = axis1;
        lastAxis2 = axis2;
        lastAxis3 = axis3;
    }

    // public float __angle1, __angle2, __angle3;

    Vector3 lastAxis1, lastAxis2, lastAxis3;
}

static class Helpers
{
    /// <summary>
    /// Transforms rotation from local space to world space.
    /// </summary>
    public static Quaternion TransformRotation(this Transform Target, Quaternion localRotation)
    {
        return Target.rotation * localRotation;
    }

    /// <summary>
    /// Transforms rotation from world space to local space.
    /// </summary>
    public static Quaternion InverseTransformRotation(this Transform Target, Quaternion worldRotation)
    {
        return Quaternion.Inverse(Target.transform.rotation) * worldRotation;
    }
}
