using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class WebUI {

	[DllImport("__Internal")]
	public static extern void ShowPanel(string panelId);
}
