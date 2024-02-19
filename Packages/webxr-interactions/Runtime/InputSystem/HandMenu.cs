#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER && HAS_XR_INTERACTION_TOOLKIT
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;
#endif
using UnityEngine;


namespace WebXR.InputSystem
{
    /// <summary>
    /// Makes a GameObject follow a tracked hand or motion controller with logic for setting visibility
    /// of the menu based on the palm orientation. This can be used, for example, to show a preferences
    /// menu when the user is looking at their palm.
    /// </summary>
    /// <remarks>
    /// This class makes the assumption that the tracked offset has the following orientation:
    /// When the user's palm is facing down with fingers pointing away from the user,
    /// y-axis is up, z-axis is forward, x-axis is right according to OpenXR.
    ///
    /// Using controllers you will need different offsets.
    /// TODO: Disable GameObject automatically when hand tracking is lost.
    /// </remarks>
    public class HandMenu : MonoBehaviour
    {
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER && HAS_XR_INTERACTION_TOOLKIT
        /// <summary>
        /// Enum dictating the up direction used in hand menu calculations.
        /// </summary>
        /// <seealso cref="handMenuUpDirection"/>
        public enum UpDirection
        {
            /// <summary>
            /// Use the global world up direction (<see cref="Vector3.up"/>).
            /// </summary>
            WorldUp,

            /// <summary>
            /// Use this GameObject's world up direction (<see cref="Transform.up"/>).
            /// Useful if this component is on a child GameObject of the XR Origin and the user can teleport to walls.
            /// </summary>
            TransformUp,

            /// <summary>
            /// Use the main camera up direction.
            /// The menu will stay oriented with the head when the user tilts their head left or right.
            /// </summary>
            CameraUp,
        }

        /// <summary>
        /// Enum determining which hand the hand menu will follow.
        /// </summary>
        /// <seealso cref="menuHandedness"/>
        public enum MenuHandedness
        {
            /// <summary>
            /// Make the menu not follow either hand. Effectively disables the hand menu.
            /// </summary>
            None,

            /// <summary>
            /// Make the menu follow the left hand.
            /// </summary>
            Left,

            /// <summary>
            /// Make the menu follow the right hand.
            /// </summary>
            Right,

            /// <summary>
            /// Make the menu follow either hand, choosing the first hand that satisfies requirements.
            /// </summary>
            Either,
        }

        [SerializeField]
        [Tooltip("Child GameObject used to hold the hand menu UI. This is the transform that moves each frame.")]
        GameObject m_HandMenuUIGameObject;

        /// <summary>
        /// Child GameObject used to hold the hand menu UI. This is the transform that moves each frame.
        /// </summary>
        public GameObject handMenuUIGameObject
        {
            get => m_HandMenuUIGameObject;
            set => m_HandMenuUIGameObject = value;
        }

        [Header("Hand alignment")]
        [SerializeField]
        [Tooltip("Which hand should the menu anchor to. None will disable the hand menu. Either will try to follow the first hand to meet requirements.")]
        MenuHandedness m_MenuHandedness = MenuHandedness.Either;

        /// <summary>
        /// Which hand should the menu anchor to.
        /// </summary>
        /// <remarks>
        /// <see cref="MenuHandedness.None"/> will disable the hand menu.
        /// <see cref="MenuHandedness.Either"/> will try to follow the first hand to meet requirements.
        /// </remarks>
        /// <seealso cref="MenuHandedness"/>
        public MenuHandedness menuHandedness
        {
            get => m_MenuHandedness;
            set => m_MenuHandedness = value;
        }
        
        [SerializeField]
        [Tooltip("Determines the up direction of the menu when the hand menu is looking at the camera.")]
        UpDirection m_HandMenuUpDirection = UpDirection.TransformUp;

        /// <summary>
        /// Determines the up direction of the menu when the hand menu is looking at the camera.
        /// </summary>
        /// <seealso cref="UpDirection"/>
        public UpDirection handMenuUpDirection
        {
            get => m_HandMenuUpDirection;
            set => m_HandMenuUpDirection = value;
        }
        
        [Header("Palm anchor")]
        [SerializeField]
        [Tooltip("Anchor associated with the left palm pose for the hand.")]
        Transform m_LeftPalmAnchor;

        /// <summary>
        /// Anchor associated with the left palm pose for the hand.
        /// </summary>
        public Transform leftPalmAnchor
        {
            get => m_LeftPalmAnchor;
            set => m_LeftPalmAnchor = value;
        }

        [SerializeField]
        [Tooltip("Anchor associated with the right palm pose for the hand.")]
        Transform m_RightPalmAnchor;

        /// <summary>
        /// Anchor associated with the right palm pose for the hand.
        /// </summary>
        public Transform rightPalmAnchor
        {
            get => m_RightPalmAnchor;
            set => m_RightPalmAnchor = value;
        }

        [Header("Position follow config.")]
        [SerializeField]
        [Tooltip("Minimum distance in meters from target before which tween starts.")]
        float m_MinFollowDistance = 0.005f;

        /// <summary>
        /// Minimum distance in meters from target before which tween starts.
        /// </summary>
        public float minFollowDistance
        {
            get => m_MinFollowDistance;
            set
            {
                m_MinFollowDistance = value;
                m_HandAnchorSmartFollow.minDistanceAllowed = value;
            }
        }

        [SerializeField]
        [Tooltip("Maximum distance in meters from target before tween targets, when time threshold is reached.")]
        float m_MaxFollowDistance = 0.03f;

        /// <summary>
        /// Maximum distance in meters from target before tween targets, when time threshold is reached.
        /// </summary>
        public float maxFollowDistance
        {
            get => m_MaxFollowDistance;
            set
            {
                m_MaxFollowDistance = value;
                m_HandAnchorSmartFollow.maxDistanceAllowed = value;
            }
        }

        [SerializeField]
        [Tooltip("Time required to elapse before the max distance allowed goes from the min distance to the max.")]
        float m_MinToMaxDelaySeconds = 1f;

        /// <summary>
        /// Time required to elapse before the max distance allowed goes from the min distance to the max.
        /// </summary>
        public float minToMaxDelaySeconds
        {
            get => m_MinToMaxDelaySeconds;
            set
            {
                m_MinToMaxDelaySeconds = value;
                m_HandAnchorSmartFollow.minToMaxDelaySeconds = value;
            }
        }

        [Header("Gaze Alignment Config")]
        [SerializeField]
        [Tooltip("If true, menu will hide when gaze to menu origin's divergence angle is above the threshold. In other words, the menu will only show if looking roughly in it's direction.")]
        bool m_HideMenuWhenGazeDiverges = true;
        
        /// <summary>
        /// If true, menu will hide when gaze to menu origin's divergence angle is above the threshold. In other words, the menu will only show if looking roughly in it's direction.
        /// </summary>
        public bool hideMenuWhenGazeDiverges
        {
            get => m_HideMenuWhenGazeDiverges;
            set => m_HideMenuWhenGazeDiverges = value;
        }
        
        [SerializeField]
        [Tooltip("Only show menu if gaze to menu origin's divergence angle is below this value.")]
        float m_MenuVisibleGazeAngleDivergenceThreshold = 35f;

        float m_MenuVisibilityDotThreshold;

        /// <summary>
        /// Only show menu if gaze to menu origin's divergence angle is below this value.
        /// </summary>
        public float menuVisibleGazeDivergenceThreshold
        {
            get => m_MenuVisibleGazeAngleDivergenceThreshold;
            set
            {
                m_MenuVisibleGazeAngleDivergenceThreshold = value;
                m_MenuVisibilityDotThreshold = AngleToDot(value);
            }
        }

        readonly SmartFollowVector3TweenableVariable m_HandAnchorSmartFollow = new SmartFollowVector3TweenableVariable();
        readonly QuaternionTweenableVariable m_RotTweenFollow = new QuaternionTweenableVariable();
        readonly Vector3TweenableVariable m_MenuScaleTweenable = new Vector3TweenableVariable();

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        [SerializeField]
        Transform m_CameraTransform;

        public Transform cameraTransform
        {
            get => m_CameraTransform;
            set => m_CameraTransform = value;
        }
        
        bool m_WasMenuHiddenLastFrame = true;

        MenuHandedness m_LastHandThatMetRequirements = MenuHandedness.Left;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Should the menu animate when it is revealed or hidden.")]
        bool m_AnimateMenuHideAndReveal = true;
        
        /// <summary>
        /// Duration of the reveal/hide animation in seconds.
        /// </summary>
        public bool animateMenuHideAndRevel
        {
            get => m_AnimateMenuHideAndReveal;
            set => m_AnimateMenuHideAndReveal = value;
        }
        
        [SerializeField]
        [Tooltip("Duration of the reveal/hide animation in seconds.")]
        float m_RevealHideAnimationDuration = 0.15f;

        /// <summary>
        /// Duration of the reveal/hide animation in seconds.
        /// </summary>
        public float revealHideAnimationDuration
        {
            get => m_RevealHideAnimationDuration;
            set => m_RevealHideAnimationDuration = value;
        }
        
        [Header("Follow presets")]
        
        [SerializeField]
        FollowPresetDatumProperty m_HandTrackingFollowPreset;

        [SerializeField]
        FollowPresetDatumProperty m_ControllerFollowPreset;

        XRInputModalityManager.InputMode m_CurrentInputMode = XRInputModalityManager.InputMode.None;

        Transform m_LeftOffsetRoot = null;
        Transform m_RightOffsetRoot = null;
        Coroutine m_HideCoroutine = null;
        Coroutine m_ShowCoroutine = null;

        Transform m_LastValidCameraTransform = null;
        Transform m_LastValidPalmAnchor = null;
        Transform m_LastValidPalmAnchorOffset = null;
        
        Vector3 m_InitialMenuLocalScale = Vector3.one;
        readonly BindableVariable<bool> m_MenuVisibleBindableVariable = new BindableVariable<bool>(false);
        float m_LastValidTrackingTime = 0f;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_HandAnchorSmartFollow.minDistanceAllowed = m_MinFollowDistance;
            m_HandAnchorSmartFollow.maxDistanceAllowed = m_MaxFollowDistance;
            m_HandAnchorSmartFollow.minToMaxDelaySeconds = m_MinToMaxDelaySeconds;
            
            // Initialize anchors
            m_RightOffsetRoot = new GameObject("Right Offset Root").transform;
            m_RightOffsetRoot.transform.SetParent(m_RightPalmAnchor);
            
            m_LeftOffsetRoot = new GameObject("Left Offset Root").transform;
            m_LeftOffsetRoot.transform.SetParent(m_LeftPalmAnchor);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_LeftPalmAnchor == null || m_RightPalmAnchor == null)
            {
                Debug.LogError($"Missing palm anchor transform reference. Disabling {this} component.", this);
                enabled = false;
                return;
            }

            if (m_HandMenuUIGameObject == null)
            {
                Debug.LogError($"Missing Hand Menu UI GameObject reference. Disabling {this} component.", this);
                enabled = false;
                return;
            }

            if (m_ControllerFollowPreset == null || m_HandTrackingFollowPreset == null)
            {
                Debug.LogError($"Missing Follow Preset reference. Disabling {this} component.", this);
                enabled = false;
                return;
            }

            m_HandAnchorSmartFollow.Value = m_HandMenuUIGameObject.transform.position;
            m_BindingsGroup.AddBinding(m_HandAnchorSmartFollow.Subscribe(newPosition => m_HandMenuUIGameObject.transform.position = newPosition));

            m_RotTweenFollow.Value = m_HandMenuUIGameObject.transform.rotation;
            m_BindingsGroup.AddBinding(m_RotTweenFollow.Subscribe(newRot => m_HandMenuUIGameObject.transform.rotation = newRot));

            m_InitialMenuLocalScale = m_HandMenuUIGameObject.transform.localScale;
            m_MenuScaleTweenable.Value = m_InitialMenuLocalScale;
            m_BindingsGroup.AddBinding(m_MenuScaleTweenable.Subscribe(value => m_HandMenuUIGameObject.transform.localScale = value));
            
            m_BindingsGroup.AddBinding(XRInputModalityManager.currentInputMode.SubscribeAndUpdate(OnInputModeChanged));

            m_MenuVisibleBindableVariable.Value = false;
            m_BindingsGroup.AddBinding(m_MenuVisibleBindableVariable.SubscribeAndUpdate(value =>
            {
                if (value)
                    ShowMenu();
                else
                    HideMenu();
            }));
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            if (m_ShowCoroutine != null)
            {
                StopCoroutine(m_ShowCoroutine);
                m_ShowCoroutine = null;
            }
            if (m_HideCoroutine != null)
            {
                StopCoroutine(m_HideCoroutine);
                m_HideCoroutine = null;
            }
            m_BindingsGroup.Clear();

            m_HandMenuUIGameObject.transform.localScale = m_InitialMenuLocalScale;
            m_HandMenuUIGameObject.SetActive(true);
            OnMenuVisible();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            m_HandAnchorSmartFollow.Dispose();
        }
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            m_HandAnchorSmartFollow.minDistanceAllowed = m_MinFollowDistance;
            m_HandAnchorSmartFollow.maxDistanceAllowed = m_MaxFollowDistance;
            m_HandAnchorSmartFollow.minToMaxDelaySeconds = m_MinToMaxDelaySeconds;
            m_MenuVisibilityDotThreshold = AngleToDot(m_MenuVisibleGazeAngleDivergenceThreshold);
        }
        
        /// <summary>
        /// This method is called when the input mode changes in the XRInputModalityManager.
        /// It updates the current preset and applies it to the left and right offset roots
        /// based on the new input mode (MotionController or TrackedHand).
        /// </summary>
        /// <param name="newInputMode">The new input mode of the XRInputModalityManager.</param>
        void OnInputModeChanged(XRInputModalityManager.InputMode newInputMode)
        {
            m_CurrentInputMode = newInputMode;
            GetCurrentPreset()?.ApplyPreset(m_LeftOffsetRoot, m_RightOffsetRoot);
        }

        FollowPreset GetCurrentPreset()
        {
            if (m_CurrentInputMode == XRInputModalityManager.InputMode.MotionController)
                return m_ControllerFollowPreset.Value;
            return m_HandTrackingFollowPreset.Value;
        }

        void ShowMenu()
        {
            if (m_HideCoroutine != null)
            {
                StopCoroutine(m_HideCoroutine);
                m_HideCoroutine = null;
            }
            
            m_HandMenuUIGameObject.SetActive(true);
            if(m_AnimateMenuHideAndReveal && m_ShowCoroutine == null)
                m_ShowCoroutine = StartCoroutine(m_MenuScaleTweenable.PlaySequence(m_MenuScaleTweenable.Value, m_InitialMenuLocalScale, m_RevealHideAnimationDuration, OnMenuVisible));
            else
                OnMenuVisible();
        }

        void OnMenuVisible()
        {
            m_ShowCoroutine = null;
            m_WasMenuHiddenLastFrame = false;
        }

        void HideMenu()
        {
            if (m_ShowCoroutine != null)
            {
                StopCoroutine(m_ShowCoroutine);
                m_ShowCoroutine = null;
            }
            
            if(m_AnimateMenuHideAndReveal && m_HideCoroutine == null)
                m_HideCoroutine = StartCoroutine(m_MenuScaleTweenable.PlaySequence(m_MenuScaleTweenable.Value, Vector3.zero, m_RevealHideAnimationDuration, OnMenuHidden));
            else
                OnMenuHidden();
        }

        void OnMenuHidden()
        {
            m_HandMenuUIGameObject.SetActive(false);
            m_WasMenuHiddenLastFrame = true;
            m_HideCoroutine = null;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (m_CurrentInputMode == XRInputModalityManager.InputMode.None)
            {
                m_MenuVisibleBindableVariable.Value = false;
                return;
            }
            
            bool showMenu = false;
            var currentPresent = GetCurrentPreset();
            if(TryGetTrackedAnchors(m_MenuHandedness, currentPresent, out var targetHandedness, out var cameraTransform, out var palmAnchor, out var palmAnchorOffset))
            {
                m_LastValidCameraTransform = cameraTransform;
                m_LastValidPalmAnchor = palmAnchor;
                m_LastValidPalmAnchorOffset = palmAnchorOffset;
                m_LastValidTrackingTime = Time.unscaledTime;
                showMenu = true;
            }
            
            // If trying to hide menu, but game object is still visible, we want to continue tracking during animation.
            if (!showMenu)
            {
                var timeSinceLastValidTracking = Time.unscaledTime - m_LastValidTrackingTime;
                
                // If within hide delay period, keep menu visible.
                if (timeSinceLastValidTracking > currentPresent.hideDelaySeconds)
                    m_MenuVisibleBindableVariable.Value = false;
                
                // If any associated transforms are invalid - return.
                if(m_LastValidCameraTransform == null || m_LastValidPalmAnchor == null || m_LastValidPalmAnchorOffset == null)
                    return;
            }
            
            var gazeToObject = (m_LastValidPalmAnchorOffset.position - m_LastValidCameraTransform.position).normalized;
            if (showMenu)
            {
                // Add extra gaze divergence validation
                if (m_HideMenuWhenGazeDiverges)
                {
                    var gazeDirection = m_LastValidCameraTransform.forward;
                    showMenu = Vector3.Dot(gazeToObject, gazeDirection) > m_MenuVisibilityDotThreshold;
                }
                m_MenuVisibleBindableVariable.Value = showMenu;
            }

            // Stop tracking if menu is not visible
            bool menuVisible = m_HandMenuUIGameObject.activeSelf;
            if(!menuVisible)
                return;

            var targetPos = m_LastValidPalmAnchorOffset.position;
            var targetRot = m_LastValidPalmAnchorOffset.rotation;
            
            // Check if head gaze is looking at palm
            if (targetHandedness == MenuHandedness.Left || targetHandedness == MenuHandedness.Right)
            {
                var referenceAxis = currentPresent.GetReferenceAxisForTrackingAnchor(m_LastValidPalmAnchor, targetHandedness == MenuHandedness.Right);
                var objectToGaze = -gazeToObject;
                
                // Gaze aligned with reference axis
                if (currentPresent.snapToGaze && Vector3.Dot(referenceAxis, objectToGaze) > currentPresent.snapToGazeDotThreshold)
                {
                    var referenceUpDirection = GetReferenceUpDirection(m_LastValidCameraTransform);
                    BurstMathUtility.OrthogonalLookRotation(gazeToObject, referenceUpDirection, out targetRot);
                }
            }

            m_HandAnchorSmartFollow.target = targetPos;
            m_RotTweenFollow.target = targetRot;

            // If the menu was previously hidden, we want to snap to the correct target
            if (m_WasMenuHiddenLastFrame || !currentPresent.allowSmoothing)
            {
                m_HandAnchorSmartFollow.HandleTween(1f);
                
                // If we allow smoothing, do not snap rotation as it looks jarring
                if(currentPresent.allowSmoothing) 
                    m_RotTweenFollow.HandleTween(Time.deltaTime * currentPresent.followLowerSmoothingValue);
                else
                    m_RotTweenFollow.HandleTween(1f);
            }
            else
            {
                m_HandAnchorSmartFollow.HandleSmartTween(Time.deltaTime, currentPresent.followLowerSmoothingValue, currentPresent.followUpperSmoothingValue);
                m_RotTweenFollow.HandleTween(Time.deltaTime * currentPresent.followLowerSmoothingValue);
            }
        }

        bool TryGetTrackedAnchors(MenuHandedness desiredHandedness, in FollowPreset currentPreset, out MenuHandedness targetHandedness, out Transform cameraTransform, out Transform palmAnchor, out Transform palmAnchorOffset)
        {
            palmAnchor = null;
            palmAnchorOffset = null;
            targetHandedness = MenuHandedness.None;
            
            if (!TryGetCamera(out cameraTransform) || desiredHandedness == MenuHandedness.None)
            {
                return false;
            }

            // Check if each palm meets requirements. We expect the up vector to be aligned with the world up when the users palm is facing the ground.
            var leftMeetsRequirements = PalmMeetsRequirements(cameraTransform, m_LeftPalmAnchor, false, currentPreset);
            var rightMeetsRequirements = PalmMeetsRequirements(cameraTransform, m_RightPalmAnchor, true, currentPreset);
            
            if (!leftMeetsRequirements && !rightMeetsRequirements)
            {
                return false;
            }

            if (desiredHandedness == MenuHandedness.Either)
            {
                // Check last hand to meet requirements
                if (leftMeetsRequirements && rightMeetsRequirements)
                {
                    var handToTry = m_LastHandThatMetRequirements == MenuHandedness.Right ? MenuHandedness.Right : MenuHandedness.Left;
                    GetTransformAnchorsForHandedness(handToTry, out palmAnchor, out palmAnchorOffset);
                    targetHandedness = handToTry;
                    return true;
                }

                if (leftMeetsRequirements)
                {
                    GetTransformAnchorsForHandedness(MenuHandedness.Left, out palmAnchor, out palmAnchorOffset);
                    m_LastHandThatMetRequirements = MenuHandedness.Left;
                    targetHandedness = MenuHandedness.Left;
                    return true;
                }
                else
                {
                    GetTransformAnchorsForHandedness(MenuHandedness.Right, out palmAnchor, out palmAnchorOffset);
                    m_LastHandThatMetRequirements = MenuHandedness.Right;
                    targetHandedness = MenuHandedness.Right;
                    return true;
                }
            }

            if (desiredHandedness == MenuHandedness.Left)
            {
                if (leftMeetsRequirements)
                {
                    GetTransformAnchorsForHandedness(MenuHandedness.Left, out palmAnchor, out palmAnchorOffset);
                    m_LastHandThatMetRequirements = MenuHandedness.Left;
                    targetHandedness = MenuHandedness.Left;
                    return true;
                }

                palmAnchor = null;
                palmAnchorOffset = null;
                return false;
            }

            if (desiredHandedness == MenuHandedness.Right)
            {
                if (rightMeetsRequirements)
                {
                    GetTransformAnchorsForHandedness(MenuHandedness.Right, out palmAnchor, out palmAnchorOffset);
                    m_LastHandThatMetRequirements = MenuHandedness.Right;
                    targetHandedness = MenuHandedness.Right;
                    return true;
                }

                palmAnchor = null;
                palmAnchorOffset = null;
                return false;
            }

            return false;
        }

        void GetTransformAnchorsForHandedness(MenuHandedness handedness, out Transform palmAnchor, out Transform palmAnchorOffset)
        {
            if (handedness == MenuHandedness.Left)
            {
                palmAnchor = m_LeftPalmAnchor;
                palmAnchorOffset = m_LeftOffsetRoot;
            }
            else if (handedness == MenuHandedness.Right)
            {
                palmAnchor = m_RightPalmAnchor;
                palmAnchorOffset = m_RightOffsetRoot;
            }
            else
            {
                palmAnchor = null;
                palmAnchorOffset = null;
            }
        }

        Vector3 GetReferenceUpDirection(Transform cameraTransform)
        {
            switch (m_HandMenuUpDirection)
            {
                case UpDirection.WorldUp:
                    return Vector3.up;

                case UpDirection.TransformUp:
                    return transform.up;

                case UpDirection.CameraUp:
                    return cameraTransform.up;

                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(UpDirection)}={m_HandMenuUpDirection}.");
                    goto case UpDirection.TransformUp;
            }
        }

        bool PalmMeetsRequirements(Transform cameraTransform, Transform palmAnchor, bool isRightHand, in FollowPreset currentPresent)
        {
            if (currentPresent == null)
                return false;

            var palmAnchorUp = currentPresent.GetReferenceAxisForTrackingAnchor(palmAnchor, isRightHand);
            var referenceUpDirection = GetReferenceUpDirection(cameraTransform);
            
            // With hand tracking, palm faces the world up direction when the hand is lying down flat.
            // With controllers, we typically look for the right vector on the left controller, and the left vector on the right controller to fill this role.
            // Check if palm is looking at the camera and whether the palm is flipped over towards the sky
            bool meetsPalmFacingUserThreshold = !currentPresent.requirePalmFacingUser || Vector3.Dot(palmAnchorUp, -cameraTransform.forward) > currentPresent.palmFacingUserDotThreshold; 
            bool meetsPalmFacingUpThreshold = !currentPresent.requirePalmFacingUp || Vector3.Dot(palmAnchorUp, referenceUpDirection) > currentPresent.palmFacingUpDotThreshold;
            
            return meetsPalmFacingUserThreshold && meetsPalmFacingUpThreshold;
        }

        // TODO: Handle the Camera becoming disabled and retry Camera.main
        bool TryGetCamera(out Transform cameraTransform)
        {
            if (m_CameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    cameraTransform = null;
                    return false;
                }
                m_CameraTransform = mainCamera.transform;
            }
            cameraTransform = m_CameraTransform;
            return true;
        }
        
        static float AngleToDot(float angleDeg)
        {
            return Mathf.Cos(Mathf.Deg2Rad * angleDeg);
        }
#endif
    }
}