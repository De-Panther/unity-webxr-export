using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WebVRControllerInteraction : MonoBehaviour
{
    [Tooltip("Map GameObject to controller hand name.")]
    public WebVRControllerHand hand = WebVRControllerHand.NONE;

    private WebVRControllerManager controllerManager;

    private FixedJoint attachJoint = null;
    private Rigidbody currentRigidBody = null;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody> ();

    void Awake()
    {
        controllerManager = WebVRControllerManager.Instance;
        attachJoint = GetComponent<FixedJoint> ();
    }

    void Update()
    {
        WebVRController controller = controllerManager.GetController(gameObject, hand);

        if (controller != null)
        {
            // Apply controller orientation and position.
            Matrix4x4 sitStand = controller.sitStand;
            Quaternion sitStandRotation = Quaternion.LookRotation (
                sitStand.GetColumn (2),
                sitStand.GetColumn (1)
            );
            transform.rotation = sitStandRotation * controller.rotation;
            transform.position = sitStand.MultiplyPoint(controller.position);

            // Button interactions
            if (controller.GetButtonDown(WebVRInputAction.Trigger) ||
                controller.GetButtonDown(WebVRInputAction.Grip))
            {
                Pickup();
            }

            if (controller.GetButtonUp(WebVRInputAction.Trigger) ||
                controller.GetButtonUp(WebVRInputAction.Grip))
            {
                Drop();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Interactable")
            return;

        contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody> ());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Interactable")
            return;

        contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody> ());
    }

    public void Pickup() {
        currentRigidBody = GetNearestRigidBody ();

        if (!currentRigidBody)
            return;

        currentRigidBody.MovePosition(transform.position);
        attachJoint.connectedBody = currentRigidBody;
    }

    public void Drop() {
        if (!currentRigidBody)
            return;

        attachJoint.connectedBody = null;
        currentRigidBody = null;
    }

    private Rigidbody GetNearestRigidBody() {
        Rigidbody nearestRigidBody = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (Rigidbody contactBody in contactRigidBodies) {
            distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

            if (distance < minDistance) {
                minDistance = distance;
                nearestRigidBody = contactBody;
            }
        }

        return nearestRigidBody;
    }
}
