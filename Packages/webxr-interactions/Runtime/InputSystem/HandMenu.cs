#if !UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER || !HAS_XR_INTERACTION_TOOLKIT
using UnityEngine;
#endif


namespace WebXR.InputSystem
{
#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER && HAS_XR_INTERACTION_TOOLKIT
    public class HandMenu : UnityEngine.XR.Interaction.Toolkit.UI.BodyUI.HandMenu
#else
    public class HandMenu : MonoBehaviour
#endif
    {
    }
}