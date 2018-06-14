using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class FreeFlightController : MonoBehaviour {
    [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
    public bool rotationEnabled = true;

    [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
    public bool translationEnabled = true;

    private WebVRDisplayCapabilities capabilities;

    [Tooltip("Mouse sensitivity")]
    public float mouseSensitivity = 1f;

    [Tooltip("Straffe Speed")]
    public float straffeSpeed = 5f;

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -90f;
    private float maximumY = 90f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    Quaternion originalRotation;

    bool inDesktopLike {
        get {
            return capabilities.hasExternalDisplay;
        }
    }

    void Start()
    {
        WebVRManager.Instance.OnVRChange += onVRChange;
        WebVRManager.Instance.OnVRCapabilitiesUpdate += onVRCapabilitiesUpdate;
        originalRotation = transform.localRotation;
    }

    private void onVRChange(WebVRState state)
    {
        if (state == WebVRState.ENABLED)
        {
            DisableEverything();
        }
        else
        {
            EnableAccordingToPlatform();
        }
    }

    private void onVRCapabilitiesUpdate(WebVRDisplayCapabilities vrCapabilities)
    {
        capabilities = vrCapabilities;
        EnableAccordingToPlatform();
    }

    void Update() {
        if (translationEnabled)
        {
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * straffeSpeed;
            float z = Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;

            transform.Translate(x, 0, z);
        }

        if (rotationEnabled && Input.GetMouseButton(0))
        {

            rotationX += Input.GetAxis ("Mouse X") * mouseSensitivity;
            rotationY += Input.GetAxis ("Mouse Y") * mouseSensitivity;

            rotationX = ClampAngle (rotationX, minimumX, maximumX);
            rotationY = ClampAngle (rotationY, minimumY, maximumY);

            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);

            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
    }

    void DisableEverything()
    {
        translationEnabled = false;
        rotationEnabled = false;
    }

    /// Enables rotation and translation control for desktop environments.
    /// For mobile environments, it enables rotation or translation according to
    /// the device capabilities.
    void EnableAccordingToPlatform()
    {
        rotationEnabled = inDesktopLike || !capabilities.canPresent;
        translationEnabled = inDesktopLike || !capabilities.hasPosition;
    }

    public static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp (angle, min, max);
    }
}
