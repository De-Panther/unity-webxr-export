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
    [SerializeField] private bool useCollidersForHandJoints = true;

    [SerializeField] private bool useInputProfile = true;

    public GameObject inputProfileObject;
    public GameObject inputProfileModelParent;

    private GameObject[] handJointsVisuals = new GameObject[25];
    private Dictionary<int, Transform> handJoints = new Dictionary<int, Transform>();
    public GameObject inputProfileHandModelParent;

    [Header("Input Bindings")]
    [SerializeField] private WebXRController.ButtonTypes[] defaultPickupButtons = new WebXRController.ButtonTypes[] {
      WebXRController.ButtonTypes.Trigger,
      WebXRController.ButtonTypes.Grip,
      WebXRController.ButtonTypes.ButtonA
    };
    private WebXRController.ButtonTypes[] pickupButtons;

    private Vector3 currentVelocity;
    private Vector3 previousPos;

#if WEBXR_INPUT_PROFILES
    private InputProfileLoader inputProfileLoader;
    private InputProfileModel inputProfileModel;
    private bool hasProfileList = false;
    private bool loadedModel = false;
    private string loadedProfile = null;

    private InputProfileModel inputProfileHandModel;
    private bool loadedHandModel = false;
    private string loadedHandProfile = null;
    private Dictionary<int, Transform> handModelJoints = new Dictionary<int, Transform>();
    private static Quaternion quat180 = Quaternion.Euler(0, 180, 0);
#endif

    private void Awake()
    {
      attachJoint = GetComponent<FixedJoint>();
      hasAnimator = animator != null;
      controller = gameObject.GetComponent<WebXRController>();
      pickupButtons = defaultPickupButtons;
#if WEBXR_INPUT_PROFILES
      if (inputProfileObject != null)
      {
        inputProfileLoader = inputProfileObject.GetComponent<InputProfileLoader>();
        if (inputProfileLoader == null)
        {
          inputProfileLoader = inputProfileObject.AddComponent<InputProfileLoader>();
        }
        var profilesPaths = inputProfileLoader.GetProfilesPaths();
        if (profilesPaths == null || profilesPaths.Count == 0)
        {
          inputProfileLoader.LoadProfilesList(HandleProfilesList);
        }
        else
        {
          HandleProfilesList(profilesPaths);
        }
      }
#endif
      SetControllerVisible(false);
      SetHandJointsVisible(false);
    }

    private void OnEnable()
    {
      if (controller.isHandActive)
      {
        SetHandJointsVisible(true);
      }
      else if (controller.isControllerActive)
      {
        SetControllerVisible(true);
      }
      controller.OnControllerActive += SetControllerVisible;
      controller.OnHandActive += SetHandJointsVisible;
      controller.OnHandUpdate += OnHandUpdate;
      controller.OnAlwaysUseGripChanged += SetInputProfileModelPose;
    }

    private void OnDisable()
    {
      controller.OnControllerActive -= SetControllerVisible;
      controller.OnHandActive -= SetHandJointsVisible;
      controller.OnHandUpdate -= OnHandUpdate;
      controller.OnAlwaysUseGripChanged -= SetInputProfileModelPose;
    }

    private void Update()
    {
      if (!controllerVisible && !handJointsVisible)
      {
        return;
      }

      // Get button A(0 or 1), or Axis Trigger/Grip (0 to 1), the larger between them all, by that order
      float normalizedTime = controller.GetButton(WebXRController.ButtonTypes.ButtonA) ? 1 :
                              Mathf.Max(controller.GetAxis(WebXRController.AxisTypes.Trigger),
                              controller.GetAxis(WebXRController.AxisTypes.Grip));
      
      bool pickup = false;
      for (int i = 0; i < pickupButtons.Length; i++) {
        pickup = pickup || controller.GetButtonDown(pickupButtons[i]);
      }
      if (pickup)
      {
        Pickup();
      }

      bool drop = false;
      for (int i = 0; i < pickupButtons.Length; i++) {
        drop = drop || controller.GetButtonUp(pickupButtons[i]);
      }
      if (drop)
      {
        Drop();
      }

      currentVelocity = (transform.position - previousPos) / Time.deltaTime;
      previousPos = transform.position;

#if WEBXR_INPUT_PROFILES
      if (loadedModel && useInputProfile)
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

    private void OnTriggerEnter(Collider other)
    {
      if (!other.gameObject.CompareTag("Interactable"))
        return;

      contactRigidBodies.Add(other.gameObject.GetComponent<Rigidbody>());
      controller.Pulse(0.5f, 250);
    }

    private void OnTriggerExit(Collider other)
    {
      if (!other.gameObject.CompareTag("Interactable"))
        return;

      contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody>());
    }

    public void SetUseCollidersForHandJoints(bool value)
    {
      useCollidersForHandJoints = value;
      for (int i = 0; i <= (int)WebXRHandJoint.pinky_finger_tip; i++)
      {
        if (handJoints.ContainsKey(i))
        {
          if (handJoints[i].TryGetComponent<Collider>(out var collider))
          {
            collider.enabled = useCollidersForHandJoints;
          }
        }
#if WEBXR_INPUT_PROFILES
        if (handModelJoints.ContainsKey(i))
        {
          if (handModelJoints[i].TryGetComponent<Collider>(out var collider))
          {
            collider.enabled = useCollidersForHandJoints;
          }
        }
#endif
      }
    }

    public bool GetUseCollidersForHandJoints()
    {
      return useCollidersForHandJoints;
    }

    public void SetUseInputProfile(bool value)
    {
      useInputProfile = value;
    }

    public bool GetUseInputProfile()
    {
      return useInputProfile;
    }

    private void SetControllerVisible(bool visible)
    {
      if (controllerVisible != visible)
      {
        controllerVisible = visible;
        Drop();
      }
#if WEBXR_INPUT_PROFILES
      // We want to use WebXR Input Profiles
      if (visible && useInputProfile)
      {
        SetInputProfileModelPose(controller.GetAlwaysUseGrip());
        if (inputProfileModel != null && loadedModel)
        {
          // There's a loaded Input Profile Model
          inputProfileModelParent.SetActive(true);
          UpdateModelInput();
          return;
        }
        else if (inputProfileModel == null)
        {
          // There's no loaded Input Profile Model and it's not in loading process
          LoadInputProfile();
        }
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
      if (!visible)
      {
        contactRigidBodies.Clear();
      }
    }

    private void SetInputProfileModelPose(bool alwaysUseGrip)
    {
      if (alwaysUseGrip)
      {
#if HAS_POSITION_AND_ROTATION
        inputProfileModelParent.transform.SetLocalPositionAndRotation(Vector3.zero, 
          Quaternion.identity);
#else
        inputProfileModelParent.transform.localPosition = Vector3.zero;
        inputProfileModelParent.transform.localRotation = Quaternion.identity;
#endif
        return;
      }
      Quaternion rotationOffset = Quaternion.Inverse(transform.localRotation);
      var gripPosition = rotationOffset * (controller.gripPosition - transform.localPosition);
      var gripRotation = rotationOffset * controller.gripRotation;
#if HAS_POSITION_AND_ROTATION
      inputProfileModelParent.transform.SetLocalPositionAndRotation(gripPosition, 
        gripRotation);
#else
      inputProfileModelParent.transform.localPosition = gripPosition;
      inputProfileModelParent.transform.localRotation = gripRotation;
#endif
    }

    private void SetHandJointsVisible(bool visible)
    {
      if (handJointsVisible != visible)
      {
        handJointsVisible = visible;
        Drop();
      }
#if WEBXR_INPUT_PROFILES
      // We want to use WebXR Input Profiles
      if (visible && useInputProfile)
      {
        if (inputProfileHandModel != null && loadedHandModel)
        {
          // There's a loaded Input Profile Model
          inputProfileHandModelParent.SetActive(true);
          return;
        }
        else if (inputProfileHandModel == null)
        {
          // There's no loaded Input Profile Model and it's not in loading process
          LoadHandInputProfile();
        }
      }
      else
      {
        inputProfileHandModelParent.SetActive(false);
      }
#endif
      foreach (var visual in handJointsVisuals)
      {
        visual?.SetActive(visible);
      }
      if (!visible)
      {
        contactRigidBodies.Clear();
      }
    }

    private void OnHandUpdate(WebXRHandData handData)
    {
      if (handJointPrefab == null)
      {
        return;
      }
      Quaternion rotationOffset = Quaternion.Inverse(handData.joints[0].rotation);

#if WEBXR_INPUT_PROFILES
      if (useInputProfile && loadedHandModel)
      {
        for (int i = 0; i <= (int)WebXRHandJoint.pinky_finger_tip; i++)
        {
          if (handModelJoints.ContainsKey(i))
          {
            handModelJoints[i].localPosition = rotationOffset * (handData.joints[i].position - handData.joints[0].position);
            handModelJoints[i].localRotation = rotationOffset * handData.joints[i].rotation * quat180;
          }
        }
        return;
      }
#endif

      for (int i = 0; i <= (int)WebXRHandJoint.pinky_finger_tip; i++)
      {
        if (handJoints.ContainsKey(i))
        {
#if HAS_POSITION_AND_ROTATION
          handJoints[i].SetLocalPositionAndRotation(rotationOffset * (handData.joints[i].position - handData.joints[0].position), rotationOffset * handData.joints[i].rotation);
#else
          handJoints[i].localPosition = rotationOffset * (handData.joints[i].position - handData.joints[0].position);
          handJoints[i].localRotation = rotationOffset * handData.joints[i].rotation;
#endif
          if (handData.joints[i].radius != handJoints[i].localScale.x && handData.joints[i].radius > 0)
          {
            handJoints[i].localScale = new Vector3(handData.joints[i].radius, handData.joints[i].radius, handData.joints[i].radius);
          }
        }
        else
        {
          var clone = Instantiate(handJointPrefab, transform);
#if HAS_POSITION_AND_ROTATION
          clone.SetLocalPositionAndRotation(rotationOffset * (handData.joints[i].position - handData.joints[0].position), rotationOffset * handData.joints[i].rotation);
#else
          clone.localPosition = rotationOffset * (handData.joints[i].position - handData.joints[0].position);
          clone.localRotation = rotationOffset * handData.joints[i].rotation;
#endif
          if (handData.joints[i].radius > 0f)
          {
            clone.localScale = new Vector3(handData.joints[i].radius, handData.joints[i].radius, handData.joints[i].radius);
          }
          else
          {
            clone.localScale = new Vector3(0.005f, 0.005f, 0.005f);
          }
          var collider = clone.GetComponent<Collider>();
          if (collider != null)
          {
            collider.enabled = useCollidersForHandJoints;
          }
          handJoints.Add(i, clone);
          handJointsVisuals[i] = clone.gameObject;
        }
      }
    }

#if WEBXR_INPUT_PROFILES
    private void HandleProfilesList(Dictionary<string, string> profilesList)
    {
      if (profilesList == null || profilesList.Count == 0)
      {
        return;
      }
      hasProfileList = true;

      if (controllerVisible && useInputProfile)
      {
        SetControllerVisible(true);
      }
    }

    private void LoadInputProfile()
    {
      if (!string.IsNullOrEmpty(loadedProfile))
      {
        return;
      }
      // Start loading possible profiles for the controller
      var profiles = controller.GetProfiles();
      if (hasProfileList && profiles != null && profiles.Length > 0)
      {
        loadedProfile = profiles[0];
        inputProfileLoader.LoadProfile(profiles, OnProfileLoaded);
      }
    }

    private void LoadHandInputProfile()
    {
      // Start loading the generic hand profile
      loadedHandProfile = "generic-hand";
      inputProfileLoader.LoadProfile(new string[] {loadedHandProfile}, OnHandProfileLoaded);
    }

    private void OnProfileLoaded(bool success)
    {
      if (success)
      {
        LoadInputModel();
      }
      // Nothing to do if profile didn't load
    }

    private void OnHandProfileLoaded(bool success)
    {
      if (success)
      {
        LoadHandInputModel();
      }
      // Nothing to do if profile didn't load
    }

    private void LoadInputModel()
    {
      inputProfileModel = inputProfileLoader.LoadModelForHand(
                          loadedProfile,
                          (InputProfileLoader.Handedness)controller.hand,
                          HandleModelLoaded);
      if (inputProfileModel != null)
      {
        // Update input state while still loading the model
        UpdateModelInput();
      }
    }

    private void LoadHandInputModel()
    {
      inputProfileHandModel = inputProfileLoader.LoadModelForHand(
                              loadedHandProfile,
                              (InputProfileLoader.Handedness)controller.hand,
                              HandleHandModelLoaded);
    }

    private void HandleModelLoaded(bool success)
    {
      loadedModel = success;
      if (loadedModel)
      {
        // Set parent only after successful loading, to not interrupt loading in case of disabled object
        var inputProfileModelTransform = inputProfileModel.transform;
        inputProfileModelTransform.SetParent(inputProfileModelParent.transform);
        inputProfileModelTransform.localPosition = Vector3.zero;
        inputProfileModelTransform.localRotation = Quaternion.identity;
        inputProfileModelTransform.localScale = Vector3.one;
        if (controllerVisible)
        {
          contactRigidBodies.Clear();
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

    private void HandleHandModelLoaded(bool success)
    {
      loadedHandModel = success;
      if (loadedHandModel)
      {
        // Set parent only after successful loading, to not interupt loading in case of disabled object
        var inputProfileModelTransform = inputProfileHandModel.transform;
        inputProfileModelTransform.SetParent(inputProfileHandModelParent.transform);
        inputProfileModelTransform.localPosition = Vector3.zero;
        inputProfileModelTransform.localRotation = quat180;
        inputProfileModelTransform.localScale = Vector3.one;
        for (int i = 0; i <= (int)WebXRHandJoint.pinky_finger_tip; i++)
        {
          handModelJoints.Add(i, inputProfileHandModel.GetChildTransform(((WebXRHandJoint)i).ToString().Replace('_','-')));
          // It took at least one frame with hand data, there should be hand joint transform
          if (handJoints.ContainsKey(i))
          {
            handModelJoints[i].SetPositionAndRotation(handJoints[i].position, handJoints[i].rotation * quat180);
            if (useCollidersForHandJoints)
            {
              var collider = handModelJoints[i].gameObject.AddComponent<SphereCollider>();
              collider.radius = handJoints[i].localScale.x;
              collider.isTrigger = true;
            }
          }
        }
        if (handJointsVisible)
        {
          inputProfileHandModelParent.SetActive(true);
          foreach (var visual in handJointsVisuals)
          {
            visual?.SetActive(false);
          }
        }
      }
      else
      {
        Destroy(inputProfileHandModel.gameObject);
      }
    }

    private void UpdateModelInput()
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

    private void SetButtonValue(int index)
    {
      inputProfileModel.SetButtonValue(index, controller.GetButtonIndexValue(index));
    }

    private void SetAxisValue(int index)
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
      currentRigidBody.velocity = currentVelocity;
      attachJoint.connectedBody = null;
      currentRigidBody = null;
    }

    private Rigidbody GetNearestRigidBody()
    {
      Rigidbody nearestRigidBody = null;
      float minDistance = float.MaxValue;
      float distance = 0.0f;
      bool removeNulls = false;

      foreach (Rigidbody contactBody in contactRigidBodies)
      {
        if (contactBody == null)
        {
          removeNulls = true;
          continue;
        }
        distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

        if (distance < minDistance)
        {
          minDistance = distance;
          nearestRigidBody = contactBody;
        }
      }

      if (removeNulls)
      {
        contactRigidBodies.RemoveAll(x => x == null);
      }

      return nearestRigidBody;
    }

    public void SetPickupButtons(params WebXRController.ButtonTypes[] pickupButtons)
    {
      this.pickupButtons = pickupButtons != null ? pickupButtons : defaultPickupButtons;
    }
  }
}
