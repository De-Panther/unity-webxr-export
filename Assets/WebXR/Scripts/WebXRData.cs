using UnityEngine;

// WebVR data class
[System.Serializable]
class WebXRData
{
	public WebXRControllerData[] controllers = new WebXRControllerData[0];
	public static WebXRData CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<WebXRData> (jsonString);
	}
}