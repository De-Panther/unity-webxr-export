using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FinishLoading();

	Camera cameraMain, cameraL, cameraR;

    Quaternion cq;
    Quaternion lhq;
    Quaternion rhq;
    Vector3 cp;
    Vector3 lhp;
    Vector3 rhp;

    bool active = false;
    private Vector3 rotation;

    public GameObject leftHandObj;
    public GameObject rightHandObj;

    //orientation of HMD, sent via SendMessage from webvr.js
    public void HMDTiltW(float w) { cq.w = w; }
    public void HMDTiltX(float x) { cq.x = x; }
    public void HMDTiltY(float y) { cq.y = y; }
    public void HMDTiltZ(float z) { cq.z = z; }

    //position of HMD, sent via SendMessage from webvr.js
    public void HMDPosX(float x) { cp.x = x; }
    public void HMDPosY(float y) { cp.y = y; }
    public void HMDPosZ(float z) { cp.z = z; }

    //orientation of left hand, sent via SendMessage from webvr.js
    public void LHTiltW(float w) { lhq.w = w; }
    public void LHTiltX(float x) { lhq.x = x; }
    public void LHTiltY(float y) { lhq.y = y; }
    public void LHTiltZ(float z) { lhq.z = z; }

    //position of left hand, sent via SendMessage from webvr.js
    public void LHPosX(float x) { lhp.x = x; }
    public void LHPosY(float y) { lhp.y = y; }
    public void LHPosZ(float z) { lhp.z = z; }

    //orientation of right hand, sent via SendMessage from webvr.js
    public void RHTiltW(float w) { rhq.w = w; }
    public void RHTiltX(float x) { rhq.x = x; }
    public void RHTiltY(float y) { rhq.y = y; }
    public void RHTiltZ(float z) { rhq.z = z; }

    //position of right hand, sent via SendMessage from webvr.js
    public void RHPosX(float x) { rhp.x = x; }
    public void RHPosY(float y) { rhp.y = y; }
    public void RHPosZ(float z) { rhp.z = z; }

    public void Begin()
    {
		changeMode("vr");
        active = true;
    }

	void changeMode(string mode)
	{
		Debug.Log("Switching to " + mode);
		switch (mode)
		{
		case "normal":
			cameraMain.GetComponent<Camera>().enabled = true;
			cameraL.GetComponent<Camera>().enabled = false;
			cameraR.GetComponent<Camera>().enabled = false;
			break;
		case "vr":
			cameraMain.GetComponent<Camera>().enabled = false;
			cameraL.GetComponent<Camera>().enabled = true;
			cameraR.GetComponent<Camera>().enabled = true;
			break;
		}
	}

    void Start()
    {
		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

		changeMode("vr");

       	FinishLoading();
    }

    void Update()
    {

        if (active == true)
        {
			transform.rotation = cq;
			transform.localPosition = cp;

			Debug.Log ("Testing");

            leftHandObj.transform.rotation = lhq;
            leftHandObj.transform.position = lhp;

            rightHandObj.transform.rotation = rhq;
            rightHandObj.transform.position = rhp;
        }
    }
}