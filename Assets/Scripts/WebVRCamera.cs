using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;

public class WebVRCamera : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void FinishLoading();

	[DllImport("__Internal")]
	private static extern void TestTimeReturn();

	[DllImport("__Internal")]
	private static extern void PostRender();

	Camera cameraMain, cameraL, cameraR;

    Quaternion cq;
    Quaternion lhq;
    Quaternion rhq;
    Vector3 cp;
	Matrix4x4 clp = Matrix4x4.identity;
	Matrix4x4 clv = Matrix4x4.identity;
	Matrix4x4 crp = Matrix4x4.identity;
	Matrix4x4 crv = Matrix4x4.identity;
	Matrix4x4 sitStand = Matrix4x4.identity;

	float deltaTime = 0.0f;


    bool active = false;
    private Vector3 rotation;

    public GameObject leftHandObj;
    public GameObject rightHandObj;

	private Matrix4x4 numbersToMatrix(float[] array) {
		var mat = new Matrix4x4 ();
		mat.m00 = array[0];
		mat.m01 = array[1];
		mat.m02 = array[2];
		mat.m03 = array[3];
		mat.m10 = array[4];
		mat.m11 = array[5];
		mat.m12 = array[6];
		mat.m13 = array[7];
		mat.m20 = array[8];
		mat.m21 = array[9];
		mat.m22 = array[10];
		mat.m23 = array[11];
		mat.m30 = array[12];
		mat.m31 = array[13];
		mat.m32 = array[14];
		mat.m33 = array[15];
		return mat;
	}
		

	// time tester
	void TestTime() {
		Debug.Log ("Time tester received in Unity");
		TestTimeReturn ();
	}
		

	// hmd start frame.
	public void startFrame() {
	}

	// Send post render update so we can submitFrame to vrDisplay.
	private IEnumerator endOfFrame()
	{
		yield return new WaitForEndOfFrame();
		PostRender ();
	}

	public void HMDViewProjection (string viewProjectionNumbersStr) {
		float[] array = viewProjectionNumbersStr.Split(',').Select(float.Parse).ToArray();

		clp = numbersToMatrix(array.Skip (16 * 0).Take (16).ToArray ());
		clv = numbersToMatrix(array.Skip(16 * 1).Take (16).ToArray ());
		crp = numbersToMatrix(array.Skip(16 * 2).Take (16).ToArray ());
		crv = numbersToMatrix(array.Skip(16 * 3).Take (16).ToArray ());
	}

	public void HMDSittingToStandingTransform (string sitStandStr) {
		float[] array = sitStandStr.Split(',').Select(float.Parse).ToArray();
		Debug.Log("received sit stand transform");
		sitStand = numbersToMatrix (array);
		sitStand = sitStand.inverse;
	}

    public void Begin()
    {
		changeMode("vr");
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
			cameraMain.GetComponent<Camera> ().enabled = true;
			cameraL.GetComponent<Camera> ().enabled = false;
			cameraR.GetComponent<Camera> ().enabled = false;
			active = false;
			break;
		case "vr":
			cameraMain.GetComponent<Camera>().enabled = false;
			cameraL.GetComponent<Camera>().enabled = true;
			cameraR.GetComponent<Camera>().enabled = true;
			active = true;
			break;
		}
	}
		
    void Start()
    {
		cameraMain = GameObject.Find("CameraMain").GetComponent<Camera>();
		cameraL = GameObject.Find("CameraL").GetComponent<Camera>();
		cameraR = GameObject.Find("CameraR").GetComponent<Camera>();

		changeMode("normal");

       	FinishLoading();
    }



    void Update()
    {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		if (Input.GetKeyDown("space")) 
		{
			toggleMode ();
		}


        if (active == true)
        {
//			leftHandObj.transform.rotation = lhq;
//            leftHandObj.transform.position = lhp;
//
//            rightHandObj.transform.rotation = rhq;
//            rightHandObj.transform.position = rhp;


			if (!sitStand.isIdentity) {
				clv *= sitStand;
				crv *= sitStand;
			}

			if (!clv.isIdentity || !clp.isIdentity || !crv.isIdentity || !crp.isIdentity) {
				cameraL.worldToCameraMatrix = clv;
				cameraL.projectionMatrix = clp;
				cameraR.worldToCameraMatrix = crv;
				cameraR.projectionMatrix = crp;
			}
        }
		StartCoroutine(endOfFrame());
    }



	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(w / 4, h / 2, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}