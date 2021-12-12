using UnityEngine;

namespace WebXR.Interactions
{
  [RequireComponent(typeof(Rigidbody))]
  public class MouseDragObject : MonoBehaviour
  {
    private Camera m_currentCamera;
    private Rigidbody m_rigidbody;
    private Vector3 m_screenPoint;
    private Vector3 m_offset;
    private Vector3 m_currentVelocity;
    private Vector3 m_previousPos;

    void Awake()
    {
      m_rigidbody = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
      m_currentCamera = FindCamera();
      if (m_currentCamera != null)
      {
        m_screenPoint = m_currentCamera.WorldToScreenPoint(gameObject.transform.position);
        m_offset = gameObject.transform.position - m_currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(m_screenPoint.z));
      }
    }

    void OnMouseUp()
    {
      m_rigidbody.velocity = m_currentVelocity;
      m_currentCamera = null;
    }

    void FixedUpdate()
    {
      if (m_currentCamera != null)
      {
        Vector3 currentScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
        m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
        m_previousPos = transform.position;
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
}
