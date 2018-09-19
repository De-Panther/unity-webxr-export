using UnityEngine;

// WebVR data class
[System.Serializable]
class WebVRData
{
	public WebVRControllerData[] controllers = new WebVRControllerData[0];
	public static WebVRData CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<WebVRData> (jsonString);
	}
}