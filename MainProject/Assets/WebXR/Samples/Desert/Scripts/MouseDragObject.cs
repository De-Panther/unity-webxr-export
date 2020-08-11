using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MouseDragObject : MonoBehaviour
{
  private Camera currentCamera;
  private new Rigidbody rigidbody;
  private Vector3 screenPoint;
  private Vector3 offset;

  void Awake()
  {
    rigidbody = GetComponent<Rigidbody>();
  }

  void OnMouseDown()
  {
    currentCamera = FindCamera();
    if (currentCamera != null)
    {
      screenPoint = currentCamera.WorldToScreenPoint(gameObject.transform.position);
      offset = gameObject.transform.position - currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(screenPoint.z));
    }
  }

  void OnMouseUp()
  {
    currentCamera = null;
  }

  void FixedUpdate()
  {
    if (currentCamera != null)
    {
      Vector3 currentScreenPoint = GetMousePosWithScreenZ(screenPoint.z);
      rigidbody.velocity = Vector3.zero;
      rigidbody.MovePosition(currentCamera.ScreenToWorldPoint(currentScreenPoint) + offset);
    }
  }

  Vector3 GetMousePosWithScreenZ(float screenZ)
  {
    return new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ);
  }

  Camera FindCamera()
  {
    Camera[] cameras = FindObjectsOfType<Camera>();
    Camera result = null;
    int camerasSum = 0;
    foreach (var camera in cameras)
    {
      if (camera.enabled)
      {
        result = camera;
        camerasSum++;
      }
    }
    if (camerasSum > 1)
    {
      result = null;
    }
    return result;
  }
}
