using UnityEngine;
using System.Collections.Generic;
using WebXR;

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
    float normalizedTime = controller.GetButton("Trigger") ? 1 : controller.GetAxis("Grip");

    if (controller.GetButtonDown("Trigger") || controller.GetButtonDown("Grip"))
      Pickup();

    if (controller.GetButtonUp("Trigger") || controller.GetButtonUp("Grip"))
      Drop();

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
