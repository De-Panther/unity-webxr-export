#if XR_HANDS_1_2_OR_NEWER
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands
{
    /// <summary>
    /// A class that follows the pinch point between the thumb and index finger using XR Hand Tracking. 
    /// It updates its position to the midpoint between the thumb and index tip while optionally adjusting its rotation 
    /// to look at a specified target. The rotation towards the target can also be smoothly interpolated over time.
    /// </summary>
    public class PinchPointFollow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The XR Hand Tracking Events component that will be used to subscribe to hand tracking events.")]
#if XR_HANDS_1_2_OR_NEWER
        XRHandTrackingEvents m_XRHandTrackingEvents;
#else
        Object m_XRHandTrackingEvents;
#endif

        [SerializeField]
        [Tooltip("The transform to match the rotation of.")]
        Transform m_TargetRotation;

        [SerializeField]
        [Tooltip("The transform will use the XRRayInteractor endpoint position to calculate the transform rotation.")]
        XRRayInteractor m_RayInteractor;

        [SerializeField]
        [Tooltip("How fast to match rotation (0 means no rotation smoothing.)")]
        [Range(0f, 32f)]
        float m_RotationSmoothingSpeed = 12f;

#if XR_HANDS_1_2_OR_NEWER
        bool m_HasRayInteractor;
        bool m_HasTargetRotationTransform;

        OneEuroFilterVector3 m_OneEuroFilterVector3;
        readonly QuaternionTweenableVariable m_QuaternionTweenableVariable = new QuaternionTweenableVariable();
        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();
#endif

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
#if XR_HANDS_1_2_OR_NEWER
            if (m_XRHandTrackingEvents != null)
                m_XRHandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

            m_OneEuroFilterVector3 = new OneEuroFilterVector3(transform.localPosition);
            m_HasRayInteractor = m_RayInteractor != null;
            m_HasTargetRotationTransform = m_TargetRotation != null;
            m_BindingsGroup.AddBinding(m_QuaternionTweenableVariable.Subscribe(newValue => transform.rotation = newValue));
#else
            Debug.LogWarning("PinchPointFollow requires XR Hands (com.unity.xr.hands) 1.2.0 or newer. Disabling component.", this);
            enabled = false;
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
#if XR_HANDS_1_2_OR_NEWER
            m_BindingsGroup.Clear();
            if (m_XRHandTrackingEvents != null)
                m_XRHandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
#endif
        }

#if XR_HANDS_1_2_OR_NEWER
        void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            var thumbTip = args.hand.GetJoint(XRHandJointID.ThumbTip);
            if (!thumbTip.TryGetPose(out var thumbTipPose))
                return;

            var indexTip = args.hand.GetJoint(XRHandJointID.IndexTip);
            if (!indexTip.TryGetPose(out var indexTipPose))
                return;

            var targetPos = Vector3.Lerp(thumbTipPose.position, indexTipPose.position, 0.5f);
            var filteredTargetPos = m_OneEuroFilterVector3.Filter(targetPos, Time.deltaTime);
            
            // Hand pose data is in local space relative to the xr origin.
            transform.localPosition = filteredTargetPos;

            if (m_HasTargetRotationTransform && m_HasRayInteractor)
            {
                // Given that the ray endpoint is in worldspace, we need to use the worldspace transform of this point to determine the target rotation.
                // This allows us to keep orientation consistent when moving the xr origin for locomotion.
                var targetDir = (m_RayInteractor.rayEndPoint - transform.position).normalized;
                var targetRot = Quaternion.LookRotation(targetDir);
                
                // If there aren't any major swings in rotation, follow the target rotation.
                if (Vector3.Dot(m_TargetRotation.forward, targetDir) > 0.5f)
                    m_QuaternionTweenableVariable.target = targetRot;
            }

            var tweenTarget = m_RotationSmoothingSpeed > 0f ? m_RotationSmoothingSpeed * Time.deltaTime : 1f;
            m_QuaternionTweenableVariable.HandleTween(tweenTarget);
        }
#endif
    }
}
