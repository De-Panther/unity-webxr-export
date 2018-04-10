using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WVRControllerInteraction : MonoBehaviour
{
    [Tooltip("Map GameObject to controller hand name.")]
    public Hand hand = Hand.NONE;

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
        WVRController controller = controllerManager.GetController(gameObject, hand);

        if (controller != null)
        {
            Matrix4x4 sitStand = controller.sitStand;
            Quaternion sitStandRotation = Quaternion.LookRotation (
                sitStand.GetColumn (2),
                sitStand.GetColumn (1)
            );
            transform.rotation = sitStandRotation * controller.rotation;
            transform.position = sitStand.MultiplyPoint(controller.position);

            if (controller.GetButton(InputAction.Trigger)) {
                // Debug.Log(hand + " trigger");
            }

            if (controller.GetButtonDown(InputAction.Trigger)) {
                // Debug.Log(hand + " trigger down");
                Pickup();
            }

            if (controller.GetButtonUp(InputAction.Trigger)) {
                // Debug.Log(hand + " trigger up");
                Drop();
            }

            if (controller.GetButton(InputAction.Grip)) {
                // Debug.Log(hand + " Grip");
            }

            if (controller.GetButtonDown(InputAction.Grip)) {
                // Debug.Log(hand + " Grip down");
            }

            if (controller.GetButtonUp(InputAction.Grip)) {
                // Debug.Log(hand + " Grip up");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag ("Interactable"))
            return;

        contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody> ());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag ("Interactable"))
            return;

        contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody> ());
    }

    public void Pickup() {
        currentRigidBody = GetNearestRigidBody ();

        if (!currentRigidBody)
            return;

        currentRigidBody.transform.position = transform.position;
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
