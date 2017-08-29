using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class SharedArrayExample : MonoBehaviour {
	[DllImport("__Internal")]
	private static extern void InitJavaScriptSharedArray(float[] array, int length);
	[DllImport("__Internal")]
	private static extern void InitJavaScriptSharedArrayButtons();

	float[] sharedArray = {0, 0, 0};

	void Start () {
		InitJavaScriptSharedArray (sharedArray, sharedArray.Length);
		InitJavaScriptSharedArrayButtons ();
	}

	void OnGUI() {
		GUI.Label (new Rect (20, 20, 500, 100), sharedArray[0].ToString() + " " + sharedArray[1].ToString() + " " + sharedArray[2].ToString());
	}
}