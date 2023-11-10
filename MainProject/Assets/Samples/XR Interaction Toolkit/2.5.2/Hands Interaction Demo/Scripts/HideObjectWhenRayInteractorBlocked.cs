namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands
{
    /// <summary>
    /// Hides the specified GameObject when the associated XRRayInteractor is blocked by an interaction within its group.
    /// </summary>
    public class HideObjectWhenRayInteractorBlocked : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The XRRayInteractor that this component monitors for blockages.")]
        XRRayInteractor m_Interactor;

        [SerializeField]
        [Tooltip("The GameObject to hide when the XRRayInteractor is blocked.")]
        GameObject m_ObjectToHide;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
            if (m_Interactor == null || m_ObjectToHide == null)
                enabled = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Update()
        {
            m_ObjectToHide.SetActive(m_Interactor.isActiveAndEnabled && !m_Interactor.IsBlockedByInteractionWithinGroup());
        }
    }
}