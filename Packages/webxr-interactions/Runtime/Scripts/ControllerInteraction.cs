using UnityEngine;
using System.Collections.Generic;
#if WEBXR_INPUT_PROFILES
using WebXRInputProfile;
#endif

namespace WebXR.Interactions
{
  public class ControllerInteraction : MonoBehaviour
  {
    private FixedJoint attachJoint = null;
    private Rigidbody currentRigidBody = null;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody>();

    [SerializeField] private Animator animator = null;
    [SerializeField] private string animationStateName = "Take";
    private WebXRController controller;
    private bool hasAnimator = false;
    private bool controllerVisible = false;

    public GameObject[] controllerVisuals;

    public Transform handJointPrefab;
    private bool handJointsVisible = false;

    public bool useInputProfile = true;

    public GameObject inputProfileObject;
    public GameObject inputProfileModelParent;

    private GameObject[] handJointsVisuals = new GameObject[25];
    private Dictionary<int, Transform> handJoints = new Dictionary<int, Transform>();

#if WEBXR_INPUT_PROFILES
    private InputProfileLoader inputProfileLoader;
    private InputProfileModel inputProfileModel;
    private bool hasProfileList = false;
    private bool loadedModel = false;
    private string loadedProfile = null;
#endif

    void Awake()
    {
      attachJoint = GetComponent<FixedJoint>();
      hasAnimator = animator != null;
      controller = gameObject.GetComponent<WebXRController>();
#if WEBXR_INPUT_PROFILES
      if (inputProfileObject != null)
      {
        inputProfileLoader = inputProfileObject.GetComponent<InputProfileLoader>();
        if (inputProfileLoader == null)
        {
          inputProfileLoader = inputProfileObject.AddComponent<InputProfileLoader>();
        }
        if (InputProfileLoader.ProfilesPaths == null || InputProfileLoader.ProfilesPaths.Count == 0)
        {

          inputProfileLoader.LoadProfilesList(HandleProfilesList);
        }
        else
        {
          HandleProfilesList(InputProfileLoader.ProfilesPaths);
        }
      }
#endif
      SetControllerVisible(false);
      SetHandJointsVisible(false);
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

    void Update()
    {
      if (!controllerVisible && !handJointsVisible)
      {
        return;
      }

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

#if WEBXR_INPUT_PROFILES
      if (loadedModel)
      {
        UpdateModelInput();
        return;
      }
#endif

      // Use the controller button or axis position to manipulate the playback time for hand model.
      if (hasAnimator)
      {
        animator.Play(animationStateName, -1, normalizedTime);
      }
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
      controllerVisible = visible;
      Drop();
#if WEBXR_INPUT_PROFILES
      if (visible && useInputProfile)
      {
        if (inputProfileModel != null)
        {
          inputProfileModelParent.SetActive(true);
          loadedModel = true;
          return;
        }
        LoadInputProfile();
      }
      else
      {
        inputProfileModelParent.SetActive(false);
      }
#endif
      foreach (var visual in controllerVisuals)
      {
        visual.SetActive(visible);
      }
    }

    void SetHandJointsVisible(bool visible)
    {
      handJointsVisible = visible;
      Drop();
      foreach (var visual in handJointsVisuals)
      {
        visual?.SetActive(visible);
      }
    }

    private void OnHandUpdate(WebXRHandData handData)
    {
      if (handJointPrefab == null)
      {
        return;
      }
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

#if WEBXR_INPUT_PROFILES
    void HandleProfilesList(Dictionary<string, string> profilesList)
    {
      if (profilesList == null || profilesList.Count == 0)
      {
        return;
      }
      hasProfileList = true;
    }

    void LoadInputProfile()
    {
      var profiles = controller.GetProfiles();
      if (hasProfileList && profiles != null && profiles.Length > 0)
      {
        loadedProfile = profiles[0];
        inputProfileLoader.LoadProfile(profiles, OnProfileLoaded);
      }
    }

    private void OnProfileLoaded(bool success)
    {
      if (success)
      {
        LoadInputModel();
      }
    }

    void LoadInputModel()
    {
      inputProfileModel = inputProfileLoader.LoadModelForHand(loadedProfile, (InputProfileLoader.Handedness)controller.hand, HandleModelLoaded);
      if (inputProfileModel != null)
      {
        UpdateModelInput();
      }
    }

    void HandleModelLoaded(bool success)
    {
      loadedModel = success;
      if (loadedModel)
      {
        var inputProfileModelTransform = inputProfileModel.transform;
        inputProfileModelTransform.SetParent(inputProfileModelParent.transform);
        inputProfileModelTransform.localPosition = Vector3.zero;
        inputProfileModelTransform.localRotation = Quaternion.identity;
        inputProfileModelTransform.localScale = Vector3.one;
        if (controllerVisible)
        {
          inputProfileModelParent.SetActive(true);
          foreach (var visual in controllerVisuals)
          {
            visual.SetActive(false);
          }
        }
      }
      else
      {
        Destroy(inputProfileModel.gameObject);
      }
    }

    void UpdateModelInput()
    {
      for (int i = 0; i < 6; i++)
      {
        SetButtonValue(i);
      }
      for (int i = 0; i < 4; i++)
      {
        SetAxisValue(i);
      }
    }

    void SetButtonValue(int index)
    {
      inputProfileModel.SetButtonValue(index, controller.GetButtonIndexValue(index));
    }

    public void SetAxisValue(int index)
    {
      inputProfileModel.SetAxisValue(index, controller.GetAxisIndexValue(index));
    }
#endif

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
