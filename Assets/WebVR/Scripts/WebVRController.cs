using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRController : MonoBehaviour
{
	WebVRManager webVRManager;

	bool vrActive = false;

	void Awake() {
		webVRManager = WebVRManager.Instance;
	}

	void Start()
	{
		WebVRManager.OnVrStateChange += handleVrStateChange;
	}

	private void handleVrStateChange()
	{
		vrActive = webVRManager.vrState == WebVRManager.VrState.ENABLED;
	}

	void Update()
	{
		if (vrActive) {
			Debug.Log("Controller count: " + webVRManager.controllers.Count);
		} 
		
		// #if UNITY_EDITOR
		// if (leftHandObject) {
		// 	leftHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand);
		// 	leftHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
		// }
		// if (rightHandObject) {
		// 	rightHandObject.transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
		// 	rightHandObject.transform.position = transform.position + UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
		// }
		// #endif

		#if !UNITY_EDITOR && UNITY_WEBGL
		// if (leftHandObject) {
		// 	leftHandObject.transform.rotation = lhr;
		// 	leftHandObject.transform.position = lhp + transform.position;
		// }
		// if (rightHandObject) {
		// 	rightHandObject.transform.rotation = rhr;
		// 	rightHandObject.transform.position = rhp + transform.position;
		// }

		#endif
	}
}