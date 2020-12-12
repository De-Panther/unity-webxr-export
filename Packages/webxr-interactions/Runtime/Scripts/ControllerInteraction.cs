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

    public GameObject[] controllerVisuals;

    public Transform handJointPrefab;

    private GameObject[] handJointsVisuals = new GameObject[25];
    private Dictionary<int, Transform> handJoints = new Dictionary<int, Transform>();

    void Awake()
    {
      attachJoint = GetComponent<FixedJoint>();
    }

    void OnEnable()
    {
      controller.OnControllerActive += SetControllerVisible;
      controller.OnHandActive += SetHandJointsVisible;
      controller.OnHandUpdate += OnHandUpdate;
    }

    void OnDisabled()
    {
      controller.OnControllerActive -= SetControllerVisible;
      controller.OnHandActive -= SetHandJointsVisible;
      controller.OnHandUpdate -= OnHandUpdate;
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

    void SetControllerVisible(bool visible)
    {
      foreach (var visual in controllerVisuals)
      {
        visual.SetActive(visible);
      }
    }

    void SetHandJointsVisible(bool visible)
    {
      foreach (var visual in handJointsVisuals)
      {
        visual?.SetActive(visible);
      }
    }

    private void OnHandUpdate(WebXRHandData handData)
    {
      Quaternion rotationOffset = Quaternion.Inverse(handData.joints[0].rotation);

      for (int i = 0; i <= WebXRHandData.LITTLE_PHALANX_TIP; i++)
      {
        if (handData.joints[i].enabled)
        {
          if (handJoints.ContainsKey(i))
          {
            handJoints[i].localPosition = rotationOffset * (handData.joints[i].position - handData.joints[0].position);
            handJoints[i].localRotation = rotationOffset * handData.joints[i].rotation;
          }
          else
          {
            var clone = Instantiate(handJointPrefab,
                                    rotationOffset * (handData.joints[i].position - handData.joints[0].position),
                                    rotationOffset * handData.joints[i].rotation,
                                    transform);
            if (handData.joints[i].radius > 0f)
            {
              clone.localScale = new Vector3(handData.joints[i].radius, handData.joints[i].radius, handData.joints[i].radius);
            }
            else
            {
              clone.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            }
            handJoints.Add(i, clone);
            handJointsVisuals[i] = clone.gameObject;
          }
        }
      }
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
