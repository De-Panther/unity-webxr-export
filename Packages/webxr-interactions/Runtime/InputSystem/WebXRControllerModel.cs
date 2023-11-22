using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif
#if HAS_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif
#if WEBXR_INPUT_PROFILES
using WebXRInputProfile;
#endif

namespace WebXR.InputSystem
{
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
  using InputSystem = UnityEngine.InputSystem.InputSystem;
#endif

  public class WebXRControllerModel : MonoBehaviour
  {
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
    /// <summary>
    /// Options for which <see cref="Transform"/> properties to update.
    /// </summary>
    /// <seealso cref="trackingType"/>
    public enum TrackingType
    {
      /// <summary>
      /// Update both rotation and position.
      /// </summary>
      RotationAndPosition,

      /// <summary>
      /// Update rotation only.
      /// </summary>
      RotationOnly,

      /// <summary>
      /// Update position only.
      /// </summary>
      PositionOnly,
    }

    /// <summary>
    /// These bit flags correspond with <c>UnityEngine.XR.InputTrackingState</c>
    /// but that enum is not used to avoid adding a dependency to the XR module.
    /// Only the Position and Rotation flags are used by this class, so velocity and acceleration flags are not duplicated here.
    /// </summary>
    [Flags]
    enum TrackingStates
    {
      /// <summary>
      /// Position and rotation are not valid.
      /// </summary>
      None,

      /// <summary>
      /// Position is valid.
      /// See <c>InputTrackingState.Position</c>.
      /// </summary>
      Position = 1 << 0,

      /// <summary>
      /// Rotation is valid.
      /// See <c>InputTrackingState.Rotation</c>.
      /// </summary>
      Rotation = 1 << 1,
    }

    [SerializeField, Tooltip("Which Transform properties to update.")]
    TrackingType m_TrackingType;

    /// <summary>
    /// The tracking type being used by the Tracked Pose Driver
    /// to control which <see cref="Transform"/> properties to update.
    /// </summary>
    /// <seealso cref="TrackingType"/>
    public TrackingType trackingType
    {
      get => m_TrackingType;
      set => m_TrackingType = value;
    }

    /// <summary>
    /// Options for which phases of the player loop will update <see cref="Transform"/> properties.
    /// </summary>
    /// <seealso cref="updateType"/>
    /// <seealso cref="InputSystem.onAfterUpdate"/>
    public enum UpdateType
    {
      /// <summary>
      /// Update after the Input System has completed an update and right before rendering.
      /// This is the recommended and default option to minimize lag for XR tracked devices.
      /// </summary>
      /// <seealso cref="InputUpdateType.BeforeRender"/>
      UpdateAndBeforeRender,

      /// <summary>
      /// Update after the Input System has completed an update except right before rendering.
      /// </summary>
      /// <remarks>
      /// This may be dynamic update, fixed update, or a manual update depending on the Update Mode
      /// project setting for Input System.
      /// </remarks>
      Update,

      /// <summary>
      /// Update after the Input System has completed an update right before rendering.
      /// </summary>
      /// <remarks>
      /// Note that this update mode may not trigger if there are no XR devices added which use before render timing.
      /// </remarks>
      /// <seealso cref="InputUpdateType.BeforeRender"/>
      /// <seealso cref="InputDevice.updateBeforeRender"/>
      BeforeRender,
    }

    [SerializeField, Tooltip("Updates the Transform properties after these phases of Input System event processing.")]
    UpdateType m_UpdateType = UpdateType.UpdateAndBeforeRender;

    /// <summary>
    /// The update type being used by the Tracked Pose Driver
    /// to control which phases of the player loop will update <see cref="Transform"/> properties.
    /// </summary>
    /// <seealso cref="UpdateType"/>
    public UpdateType updateType
    {
      get => m_UpdateType;
      set => m_UpdateType = value;
    }

    [SerializeField] private GameObject rigOrigin;

    [SerializeField] private WebXRControllerHand hand;
#if HAS_XR_INTERACTION_TOOLKIT
    [SerializeField] private XRBaseController xrController;
#endif
    [SerializeField] private InputActionProperty positionActionProperty;
    [SerializeField] private InputActionProperty rotationActionProperty;
    [SerializeField] private InputActionProperty trackingStateActionProperty;
    [SerializeField] private InputActionProperty triggerActionProperty;
    [SerializeField] private InputActionProperty gripActionProperty;
    [SerializeField] private InputActionProperty touchpadActionProperty;
    [SerializeField] private InputActionProperty touchpadPressedActionProperty;
    [SerializeField] private InputActionProperty thumbstickActionProperty;
    [SerializeField] private InputActionProperty thumbstickPressedActionProperty;
    [SerializeField] private InputActionProperty buttonAActionProperty;
    [SerializeField] private InputActionProperty buttonBActionProperty;

#if WEBXR_INPUT_PROFILES
    private InputProfileLoader inputProfileLoader;
    private InputProfileModel inputProfileModel;
#endif
    private bool startedLoading = false;
    private bool hasProfileList = false;
    private string loadedProfile = null;
    private bool loadedModel = false;
    private static Quaternion quat180 = Quaternion.Euler(0, 180, 0);
    private InputAction positionAction;
    private InputAction rotationAction;
    private InputAction trackingStateAction;
    private InputAction[] buttonActions;// = new InputAction[6];
    private InputAction[] axisActions;// = new InputAction[2];

    Vector3 m_CurrentPosition = Vector3.zero;
    Quaternion m_CurrentRotation = Quaternion.identity;
    TrackingStates m_CurrentTrackingState = TrackingStates.None;
    bool m_RotationBound;
    bool m_PositionBound;
    bool m_TrackingStateBound;
    private bool visualActionsBound;
    private bool visualActionsUpdated;
    bool m_IsFirstUpdate = true;

    private Transform xrControllerOldModel;

#if WEBXR_INPUT_PROFILES
    void BindActions()
    {
      BindPosition();
      BindRotation();
      BindTrackingState();
      BindVisualActions();
    }

    void UnbindActions()
    {
      UnbindPosition();
      UnbindRotation();
      UnbindTrackingState();
      UnbindVisualActions();
    }

    void BindPosition()
    {
      if (m_PositionBound)
        return;

      positionAction = positionActionProperty.action;
      if (positionAction == null)
        return;

      positionAction.performed += OnPositionPerformed;
      positionAction.canceled += OnPositionCanceled;
      m_PositionBound = true;
    }

    void BindRotation()
    {
      if (m_RotationBound)
        return;

      rotationAction = rotationActionProperty.action;
      if (rotationAction == null)
        return;

      rotationAction.performed += OnRotationPerformed;
      rotationAction.canceled += OnRotationCanceled;
      m_RotationBound = true;
    }

    void BindTrackingState()
    {
      if (m_TrackingStateBound)
        return;

      trackingStateAction = trackingStateActionProperty.action;
      if (trackingStateAction == null)
        return;

      trackingStateAction.performed += OnTrackingStatePerformed;
      trackingStateAction.canceled += OnTrackingStateCanceled;
      m_TrackingStateBound = true;
    }

    void BindVisualActions()
    {
      if (visualActionsBound)
        return;

      buttonActions = new InputAction[6];
      axisActions = new InputAction[2];
      buttonActions[0] = triggerActionProperty.action;
      buttonActions[1] = gripActionProperty.action;
      buttonActions[2] = touchpadPressedActionProperty.action;
      buttonActions[3] = thumbstickPressedActionProperty.action;
      buttonActions[4] = buttonAActionProperty.action;
      buttonActions[5] = buttonBActionProperty.action;
      axisActions[0] = touchpadActionProperty.action;
      axisActions[1] = thumbstickActionProperty.action;

      for (int i = 0; i < buttonActions.Length; i++)
      {
        buttonActions[i].performed += OnVisualActionsPerformed;
        buttonActions[i].canceled += OnVisualActionsCanceled;
      }
      for (int i = 0; i < axisActions.Length; i++)
      {
        axisActions[i].performed += OnVisualActionsPerformed;
        axisActions[i].canceled += OnVisualActionsCanceled;
      }

      visualActionsBound = true;
    }

    void UnbindPosition()
    {
      if (!m_PositionBound)
        return;

      if (positionAction == null)
        return;

      positionAction.performed -= OnPositionPerformed;
      positionAction.canceled -= OnPositionCanceled;
      m_PositionBound = false;
    }

    void UnbindRotation()
    {
      if (!m_RotationBound)
        return;

      if (rotationAction == null)
        return;

      rotationAction.performed -= OnRotationPerformed;
      rotationAction.canceled -= OnRotationCanceled;
      m_RotationBound = false;
    }

    void UnbindTrackingState()
    {
      if (!m_TrackingStateBound)
        return;

      if (trackingStateAction == null)
        return;

      trackingStateAction.performed -= OnTrackingStatePerformed;
      trackingStateAction.canceled -= OnTrackingStateCanceled;
      m_TrackingStateBound = false;
    }

    void UnbindVisualActions()
    {
      if (!visualActionsBound)
        return;

      for (int i = 0; i < buttonActions.Length; i++)
      {
        buttonActions[i].performed -= OnVisualActionsPerformed;
        buttonActions[i].canceled -= OnVisualActionsCanceled;
      }
      for (int i = 0; i < axisActions.Length; i++)
      {
        axisActions[i].performed -= OnVisualActionsPerformed;
        axisActions[i].canceled -= OnVisualActionsCanceled;
      }

      visualActionsBound = false;
    }

    void OnPositionPerformed(InputAction.CallbackContext context)
    {
      m_CurrentPosition = context.ReadValue<Vector3>();
    }

    void OnPositionCanceled(InputAction.CallbackContext context)
    {
      m_CurrentPosition = Vector3.zero;
    }

    void OnRotationPerformed(InputAction.CallbackContext context)
    {
      m_CurrentRotation = context.ReadValue<Quaternion>();
    }

    void OnRotationCanceled(InputAction.CallbackContext context)
    {
      m_CurrentRotation = Quaternion.identity;
    }

    void OnTrackingStatePerformed(InputAction.CallbackContext context)
    {
      m_CurrentTrackingState = (TrackingStates)context.ReadValue<int>();
      if (loadedModel)
      {
        inputProfileModel.gameObject.SetActive(m_CurrentTrackingState != TrackingStates.None);
      }
    }

    void OnTrackingStateCanceled(InputAction.CallbackContext context)
    {
      m_CurrentTrackingState = TrackingStates.None;
      if (loadedModel)
      {
        inputProfileModel.gameObject.SetActive(false);
      }
    }

    void OnVisualActionsPerformed(InputAction.CallbackContext context)
    {
      visualActionsUpdated = true;
    }

    void OnVisualActionsCanceled(InputAction.CallbackContext context)
    {
      visualActionsUpdated = true;
    }

    private void Awake()
    {
      inputProfileLoader = rigOrigin.GetComponent<InputProfileLoader>();
      if (inputProfileLoader == null)
      {
        inputProfileLoader = rigOrigin.AddComponent<InputProfileLoader>();
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

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    protected void OnEnable()
    {
      BindActions();
      InputSystem.onAfterUpdate += UpdateCallback;

      // Read current input values when becoming enabled,
      // but wait until after the input update so the input is read at a consistent time
      m_IsFirstUpdate = true;
      switch (hand)
      {
        case WebXRControllerHand.LEFT:
          WebXRInputSystem.OnLeftControllerProfiles += HandleOnControllerProfiles;
          break;
        case WebXRControllerHand.RIGHT:
          WebXRInputSystem.OnRightControllerProfiles += HandleOnControllerProfiles;
          break;
      }
      HandleOnControllerProfiles();
    }

    /// <summary>
    /// This function is called when the object becomes disabled or inactive.
    /// </summary>
    protected void OnDisable()
    {
      UnbindActions();
      InputSystem.onAfterUpdate -= UpdateCallback;
      switch (hand)
      {
        case WebXRControllerHand.LEFT:
          WebXRInputSystem.OnLeftControllerProfiles -= HandleOnControllerProfiles;
          break;
        case WebXRControllerHand.RIGHT:
          WebXRInputSystem.OnRightControllerProfiles -= HandleOnControllerProfiles;
          break;
      }
#if HAS_XR_INTERACTION_TOOLKIT
      if (xrController != null && xrControllerOldModel != null)
      {
        xrController.model = xrControllerOldModel;
        xrController.hideControllerModel = false;
      }
#endif
    }

    /// <summary>
    /// The callback method called after the Input System has completed an update and processed all pending events.
    /// </summary>
    /// <seealso cref="InputSystem.onAfterUpdate"/>
    protected void UpdateCallback()
    {
      if (m_IsFirstUpdate)
      {
        // Update current input values if this is the first update since becoming enabled
        // since the performed callbacks may not have been executed
        if (positionAction != null)
          m_CurrentPosition = positionAction.ReadValue<Vector3>();

        if (rotationAction != null)
          m_CurrentRotation = rotationAction.ReadValue<Quaternion>();

        ReadTrackingState();

        m_IsFirstUpdate = false;
      }

      if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
        OnBeforeRender();
      else
        OnUpdate();
    }

    void ReadTrackingState()
    {
      if (trackingStateAction != null && !trackingStateAction.enabled)
      {
        // Treat a disabled action as the default None value for the ReadValue call
        m_CurrentTrackingState = TrackingStates.None;
        return;
      }

      if (trackingStateAction == null)
      {
        m_CurrentTrackingState = TrackingStates.None;
        return;
      }

      m_CurrentTrackingState = (TrackingStates)trackingStateAction.ReadValue<int>();
    }

    /// <summary>
    /// This method is called after the Input System has completed an update and processed all pending events
    /// when the type of update is not <see cref="InputUpdateType.BeforeRender"/>.
    /// </summary>
    protected virtual void OnUpdate()
    {
      if (m_UpdateType == UpdateType.Update ||
          m_UpdateType == UpdateType.UpdateAndBeforeRender)
      {
        PerformUpdate();
      }
    }

    /// <summary>
    /// This method is called after the Input System has completed an update and processed all pending events
    /// when the type of update is <see cref="InputUpdateType.BeforeRender"/>.
    /// </summary>
    protected virtual void OnBeforeRender()
    {
      if (m_UpdateType == UpdateType.BeforeRender ||
          m_UpdateType == UpdateType.UpdateAndBeforeRender)
      {
        PerformUpdate();
      }
    }

    /// <summary>
    /// Updates <see cref="Transform"/> properties with the current input pose values that have been read,
    /// constrained by tracking type and tracking state.
    /// </summary>
    /// <seealso cref="SetLocalTransform"/>
    protected virtual void PerformUpdate()
    {
      SetLocalTransform(m_CurrentPosition, m_CurrentRotation);
      if (loadedModel && visualActionsUpdated)
      {
        UpdateModelInput();
      }
    }

    /// <summary>
    /// Updates <see cref="Transform"/> properties, constrained by tracking type and tracking state.
    /// </summary>
    /// <param name="newPosition">The new local position to possibly set.</param>
    /// <param name="newRotation">The new local rotation to possibly set.</param>
    protected virtual void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
    {
      var positionValid = (m_CurrentTrackingState & TrackingStates.Position) != 0;
      var rotationValid = (m_CurrentTrackingState & TrackingStates.Rotation) != 0;

#if HAS_POSITION_AND_ROTATION
      if (m_TrackingType == TrackingType.RotationAndPosition && rotationValid && positionValid)
      {
        transform.SetLocalPositionAndRotation(newPosition, newRotation);
        return;
      }
#endif

      if (rotationValid &&
          (m_TrackingType == TrackingType.RotationAndPosition ||
           m_TrackingType == TrackingType.RotationOnly))
      {
        transform.localRotation = newRotation;
      }

      if (positionValid &&
          (m_TrackingType == TrackingType.RotationAndPosition ||
           m_TrackingType == TrackingType.PositionOnly))
      {
        transform.localPosition = newPosition;
      }
    }

    private void UpdateModelInput()
    {
      if (m_CurrentTrackingState == TrackingStates.None)
      {
        return;
      }
      visualActionsUpdated = false;
      for (int i = 0; i < 6; i++)
      {
        inputProfileModel.SetButtonValue(i, buttonActions[i].ReadValue<float>());
      }

      var axis = axisActions[0].ReadValue<Vector2>();
      inputProfileModel.SetAxisValue(0, axis.x);
      inputProfileModel.SetAxisValue(1, axis.y);
      axis = axisActions[1].ReadValue<Vector2>();
      inputProfileModel.SetAxisValue(2, axis.x);
      inputProfileModel.SetAxisValue(3, axis.y);
    }

    private void HandleProfilesList(Dictionary<string, string> profilesList)
    {
      if (profilesList == null || profilesList.Count == 0)
      {
        return;
      }

      hasProfileList = true;
    }

    public void HandleOnControllerProfiles()
    {
      if (loadedModel)
      {
        HandleModelLoaded(loadedModel);
      }
      if (startedLoading)
      {
        return;
      }

      string[] profiles = null;
      switch (hand)
      {
        case WebXRControllerHand.LEFT:
          profiles = WebXRInputSystem.GetLeftProfiles();
          break;
        case WebXRControllerHand.RIGHT:
          profiles = WebXRInputSystem.GetRightProfiles();
          break;
      }

      if (hasProfileList && profiles != null && profiles.Length > 0)
      {
        startedLoading = true;
        loadedProfile = profiles[0];
        inputProfileLoader.LoadProfile(profiles, OnProfileLoaded);
      }
    }

    private void OnProfileLoaded(bool success)
    {
      if (!success)
      {
        return;
      }

      LoadInputModel();
    }

    private void LoadInputModel()
    {
      inputProfileModel = inputProfileLoader.LoadModelForHand(
        loadedProfile,
        (InputProfileLoader.Handedness)hand,
        HandleModelLoaded);
      if (inputProfileModel != null)
      {
        // Update input state while still loading the model
        UpdateModelInput();
      }
    }

    private void HandleModelLoaded(bool success)
    {
      loadedModel = success;
      if (!loadedModel)
      {
        Destroy(inputProfileModel.gameObject);
        return;
      }

      var inputProfileModelTransform = inputProfileModel.transform;
      inputProfileModelTransform.SetParent(transform);
      inputProfileModelTransform.localPosition = Vector3.zero;
      inputProfileModelTransform.localRotation = Quaternion.identity;
      inputProfileModelTransform.localScale = Vector3.one;
#if HAS_XR_INTERACTION_TOOLKIT
      if (xrController != null && xrController.model != null)
      {
        xrControllerOldModel = xrController.model;
        xrController.hideControllerModel = true;
        xrController.model = null;
      }
#endif
      if (m_CurrentTrackingState == TrackingStates.None)
      {
        inputProfileModel.gameObject.SetActive(false);
      }
    }
#endif
#endif
  }
}