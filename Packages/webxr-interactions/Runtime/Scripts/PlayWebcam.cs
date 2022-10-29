using System;
using System.Collections;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

public class PlayWebcam : MonoBehaviour
{
  #if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]
  public static extern void JS_WebCamVideo_SetLatestTextureId(System.IntPtr textureId);

  [DllImport("__Internal")]
  public static extern void JS_WebCamVideo_RemoveWhereTextureId(System.IntPtr textureId);
  #endif

  [SerializeField]
  private string thresholdMinName = "_ThresholdMin";
  [SerializeField]
  private string thresholdMaxName = "_ThresholdMax";

  private WebCamTexture webcamTexture;
  private Renderer _renderer;
  private bool started = false;

  private Material material;
  private bool hasThresholdProperties = false;
  private int thresholdMinID = -1;
  private int thresholdMaxID = -1;
  private Color thresholdMinColor;
  private Color thresholdMaxColor;
  private IntPtr latestWebcamPtr = IntPtr.Zero;

  private int defaultWidth = 1280;
  private int defaultHeight = 720;

  void Awake()
  {
    TrySetupRenderer();
  }

  void TrySetupRenderer()
  {
    if (_renderer == null)
    {
      _renderer = GetComponent<Renderer>();
      material = Instantiate(_renderer.sharedMaterial);
      _renderer.sharedMaterial = material;
      hasThresholdProperties = false;
      thresholdMinID = material.shader.FindPropertyIndex(thresholdMinName);
      if (thresholdMinID == -1)
      {
        return;
      }
      thresholdMinID = material.shader.GetPropertyNameId(thresholdMinID);
      thresholdMaxID = material.shader.FindPropertyIndex(thresholdMaxName);
      if (thresholdMaxID == -1)
      {
        return;
      }
      thresholdMaxID = material.shader.GetPropertyNameId(thresholdMaxID);
      thresholdMinColor = material.GetColor(thresholdMinID);
      thresholdMaxColor = material.GetColor(thresholdMaxID);
      hasThresholdProperties = true;
    }
  }

  void Start()
  {
    started = true;
    Play();
  }

  void Play()
  {
    if (webcamTexture == null)
    {
      var devices = WebCamTexture.devices;
      var device = devices[0];
      var resolutions = device.availableResolutions;
      if (resolutions?.Length > 0)
      {
        webcamTexture = new WebCamTexture(device.name, resolutions[0].width, resolutions[0].height, resolutions[0].refreshRate);
      }
      else
      {
        webcamTexture = new WebCamTexture(device.name, defaultWidth, defaultHeight);
      }
      float ratio = (float)defaultWidth / (float)defaultHeight;
      transform.localScale = new Vector3(ratio, 1f, 1f);
      material.mainTexture = webcamTexture;
    }
    latestWebcamPtr = webcamTexture.GetNativeTexturePtr();
    #if UNITY_WEBGL && !UNITY_EDITOR
    JS_WebCamVideo_RemoveWhereTextureId(latestWebcamPtr);
    #endif
    webcamTexture.Play();
    StartCoroutine(SetScale());
  }

  IEnumerator SetScale()
  {
    while (webcamTexture.GetNativeTexturePtr() == latestWebcamPtr)
    {
      yield return null;
    }
    #if UNITY_WEBGL && !UNITY_EDITOR
    JS_WebCamVideo_SetLatestTextureId(webcamTexture.GetNativeTexturePtr());
    #endif
    float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
    transform.localScale = new Vector3(ratio, 1f, 1f);
  }

  void OnEnable()
  {
    if (started)
    {
      Play();
    }
  }

  void OnDisable()
  {
    StopAllCoroutines();
    webcamTexture.Stop();
  }

  private void TrySetColor(string value, ref Color color, int propertyID, int colorLetter)
  {
    if (hasThresholdProperties && int.TryParse(value, out int intValue))
    {
      switch (colorLetter)
      {
        case 0:
          color.r = (float)Mathf.Clamp(intValue, 0, 255) / 255f;
          break;
        case 1:
          color.g = (float)Mathf.Clamp(intValue, 0, 255) / 255f;
          break;
        case 2:
          color.b = (float)Mathf.Clamp(intValue, 0, 255) / 255f;
          break;
      }
      material.SetColor(propertyID, color);
    }
  }

  public void TrySetMinR(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMinColor, thresholdMinID, 0);
  }

  public void TrySetMinG(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMinColor, thresholdMinID, 1);
  }

  public void TrySetMinB(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMinColor, thresholdMinID, 2);
  }

  public void TrySetMaxR(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMaxColor, thresholdMaxID, 0);
  }

  public void TrySetMaxG(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMaxColor, thresholdMaxID, 1);
  }

  public void TrySetMaxB(string value)
  {
    TrySetupRenderer();
    TrySetColor(value, ref thresholdMaxColor, thresholdMaxID, 2);
  }
}
