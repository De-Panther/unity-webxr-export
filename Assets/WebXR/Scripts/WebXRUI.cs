using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class WebXRUI {

	[DllImport("__Internal")]
	public static extern void displayElementId(string id);
}
