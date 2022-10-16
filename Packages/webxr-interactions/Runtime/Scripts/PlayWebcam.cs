using System.Collections;
using UnityEngine;

public class PlayWebcam : MonoBehaviour
{
  private WebCamTexture webcamTexture;
  private Renderer _renderer;
  private bool started = false;

  private int defaultWidth = 1280;
  private int defaultHeight = 720;

  void Awake()
  {
    _renderer = GetComponent<Renderer>();
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
      _renderer.material.mainTexture = webcamTexture;
    }
    webcamTexture.Play();
    StartCoroutine(SetScale());
  }

  IEnumerator SetScale()
  {
    while (!webcamTexture.isPlaying || webcamTexture.height == 16)
    {
      yield return null;
    }
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
}
