using UnityEngine;
using System.Linq;
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
	Matrix4x4 clp = new Matrix4x4();
	Matrix4x4 clv = new Matrix4x4();
	Matrix4x4 crp = new Matrix4x4();
	Matrix4x4 crv = new Matrix4x4();


    bool active = false;
    private Vector3 rotation;

    public GameObject leftHandObj;
    public GameObject rightHandObj;

	// view and projection matrix, sent via SendMessage from webvr.js
	public void HMDLeftProjection(string numbersStr) {
		float[] array = numbersStr.Split(',').Select(float.Parse).ToArray();

		clp [0, 0] = array[0];
		clp [0, 1] = array[4];
		clp [0, 2] = array[8];
		clp [0, 3] = array[12];
		clp [1, 0] = array[1];
		clp [1, 1] = array[5];
		clp [1, 2] = array[9];
		clp [1, 3] = array[13];
		clp [2, 0] = array[2];
		clp [2, 1] = array[6];
		clp [2, 2] = array[10];
		clp [2, 3] = array[14];
		clp [3, 0] = array[3];
		clp [3, 1] = array[7];
		clp [3, 2] = array[11];
		clp [3, 3] = array[15];
	}

	public void HMDRightProjection(string numbersStr) {
		float[] array = numbersStr.Split(',').Select(float.Parse).ToArray();

		crp [0, 0] = array[0];
		crp [0, 1] = array[4];
		crp [0, 2] = array[8];
		crp [0, 3] = array[12];
		crp [1, 0] = array[1];
		crp [1, 1] = array[5];
		crp [1, 2] = array[9];
		crp [1, 3] = array[13];
		crp [2, 0] = array[2];
		crp [2, 1] = array[6];
		crp [2, 2] = array[10];
		crp [2, 3] = array[14];
		crp [3, 0] = array[3];
		crp [3, 1] = array[7];
		crp [3, 2] = array[11];
		crp [3, 3] = array[15];
	}

	public void HMDLeftView(string numbersStr) {
		float[] array = numbersStr.Split(',').Select(float.Parse).ToArray();

		clv [0, 0] = array[0];
		clv [0, 1] = array[4];
		clv [0, 2] = array[8];
		clv [0, 3] = array[12];
		clv [1, 0] = array[1];
		clv [1, 1] = array[5];
		clv [1, 2] = array[9];
		clv [1, 3] = array[13];
		clv [2, 0] = array[2];
		clv [2, 1] = array[6];
		clv [2, 2] = array[10];
		clv [2, 3] = array[14];
		clv [3, 0] = array[3];
		clv [3, 1] = array[7];
		clv [3, 2] = array[11];
		clv [3, 3] = array[15];
	}

	public void HMDRightView(string numbersStr) {
		float[] array = numbersStr.Split(',').Select(float.Parse).ToArray();

		crv [0, 0] = array[0];
		crv [0, 1] = array[4];
		crv [0, 2] = array[8];
		crv [0, 3] = array[12];
		crv [1, 0] = array[1];
		crv [1, 1] = array[5];
		crv [1, 2] = array[9];
		crv [1, 3] = array[13];
		crv [2, 0] = array[2];
		crv [2, 1] = array[6];
		crv [2, 2] = array[10];
		crv [2, 3] = array[14];
		crv [3, 0] = array[3];
		crv [3, 1] = array[7];
		crv [3, 2] = array[11];
		crv [3, 3] = array[15];
	}

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

	void toggleMode() {
		active = active == true ? false : true;
		string mode = active == true ? "vr" : "normal";
		changeMode (mode);
	}

	void changeMode(string mode)
	{
		Debug.Log("Switching to " + mode);
		switch (mode)
		{
		case "normal":
			Debug.Log (cameraMain.GetComponent<Camera> ().projectionMatrix);
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

		changeMode("normal");

		cameraMain.projectionMatrix = cameraMain.projectionMatrix * Matrix4x4.Scale(new Vector3 (1, 1, 1));

       	FinishLoading();
    }

    void Update()
    {
		if (Input.GetKeyDown("space")) {
			toggleMode ();
		}


        if (active == true)
        {
			

            leftHandObj.transform.rotation = lhq;
            leftHandObj.transform.position = lhp;

            rightHandObj.transform.rotation = rhq;
            rightHandObj.transform.position = rhp;

			cameraL.worldToCameraMatrix = clv;
			cameraL.projectionMatrix = clp;

			cameraR.worldToCameraMatrix = crv;
			cameraR.projectionMatrix = crp;
        }
    }
}