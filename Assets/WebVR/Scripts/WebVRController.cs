using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRController : MonoBehaviour
{
	public enum Hand { NONE, LEFT, RIGHT };

	[Tooltip("Map GameObject to controller hand name.")]
	public Hand hand = Hand.NONE;

	private WebVRManager webVRManager;
	private WebVRControllerManager controllerManager;

	private bool vrActive = false;

	void Awake() {
		webVRManager = WebVRManager.Instance;
		controllerManager = WebVRControllerManager.Instance;
	}

	void Start()
	{
		WebVRManager.OnVrStateChange += handleVrStateChange;
	}

	private void handleVrStateChange()
	{
		vrActive = webVRManager.vrState == VrState.ENABLED;
	}

	void Update()
	{

		if (vrActive) {
			Controller controller = controllerManager.registerController(gameObject);

			if (controller != null) {
				Matrix4x4 sitStand = webVRManager.stageParameters.SitStand;
				Quaternion sitStandRotation = Quaternion.LookRotation (
					sitStand.GetColumn (2),
					sitStand.GetColumn (1)
				);
				Vector3 p = sitStand.MultiplyPoint(controller.position);
				Quaternion r = sitStandRotation * controller.rotation;

				#if UNITY_EDITOR
				// if (leftHandObject) {
				// 	leftHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
				// 	leftHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
				// }
				// if (rightHandObject) {
				// 	rightHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
				// 	rightHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
				// }
				#endif
				#if !UNITY_EDITOR && UNITY_WEBGL
				gameObject.transform.rotation = r;
				gameObject.transform.position = p;
				#endif
			}
		}
	}
}