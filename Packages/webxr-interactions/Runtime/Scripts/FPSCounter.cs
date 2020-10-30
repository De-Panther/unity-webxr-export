using UnityEngine;

public class FPSCounter : MonoBehaviour
{
  public TextMesh text;
  private float fps = 0;
  private float framesCount = 0;
  private float lastCheck = 0;
  private float rate = 0.5f;

  void Update()
  {
    framesCount++;
    if (Time.time >= lastCheck+rate)
    {
      fps = framesCount / (Time.time-lastCheck);
      lastCheck = Time.time;
      framesCount = 0;
      text.text = fps.ToString("F0");
    }
  }
}
