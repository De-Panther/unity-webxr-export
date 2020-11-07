using UnityEngine;
using System.Collections.Generic;

namespace WebXR.Interactions
{
  public class ControllerInteraction : MonoBehaviour
  {
    private FixedJoint attachJoint = null;
    private Rigidbody currentRigidBody = null;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody>();

    private Animator anim;
    private WebXRController controller;

    void Awake()
    {
      attachJoint = GetComponent<FixedJoint>();
    }

    void Start()
    {
      anim = gameObject.GetComponent<Animator>();
      controller = gameObject.GetComponent<WebXRController>();
    }

    void Update()
    {
      controller.TryUpdateButtons();

      // Get button A(0 or 1), or Axis Trigger/Grip (0 to 1), the larger between them all, by that order
      float normalizedTime = controller.GetButton(WebXRController.ButtonTypes.ButtonA) ? 1 :
                              Mathf.Max(controller.GetAxis(WebXRController.AxisTypes.Trigger),
                              controller.GetAxis(WebXRController.AxisTypes.Grip));

      if (controller.GetButtonDown(WebXRController.ButtonTypes.Trigger)
          || controller.GetButtonDown(WebXRController.ButtonTypes.Grip)
          || controller.GetButtonDown(WebXRController.ButtonTypes.ButtonA))
      {
        Pickup();
      }

      if (controller.GetButtonUp(WebXRController.ButtonTypes.Trigger)
          || controller.GetButtonUp(WebXRController.ButtonTypes.Grip)
          || controller.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
      {
        Drop();
      }

      // Use the controller button or axis position to manipulate the playback time for hand model.
      anim.Play("Take", -1, normalizedTime);
    }

    void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.tag != "Interactable")
        return;

      contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody>());
      controller.Pulse(0.5f, 250);
    }

    void OnTriggerExit(Collider other)
    {
      if (other.gameObject.tag != "Interactable")
        return;

      contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody>());
    }

    public void Pickup()
    {
      currentRigidBody = GetNearestRigidBody();

      if (!currentRigidBody)
        return;

      currentRigidBody.MovePosition(transform.position);
      attachJoint.connectedBody = currentRigidBody;
    }

    public void Drop()
    {
      if (!currentRigidBody)
        return;

      attachJoint.connectedBody = null;
      currentRigidBody = null;
    }

    private Rigidbody GetNearestRigidBody()
    {
      Rigidbody nearestRigidBody = null;
      float minDistance = float.MaxValue;
      float distance = 0.0f;

      foreach (Rigidbody contactBody in contactRigidBodies)
      {
        distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

        if (distance < minDistance)
        {
          minDistance = distance;
          nearestRigidBody = contactBody;
        }
      }

      return nearestRigidBody;
    }
  }
}
