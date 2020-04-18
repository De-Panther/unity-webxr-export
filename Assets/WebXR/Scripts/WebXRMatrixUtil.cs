using UnityEngine;

namespace WebXR
{
  public static class WebXRMatrixUtil
  {
    // According to https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html
    public static void SetTransformFromViewMatrix(Transform transform, Matrix4x4 webVRViewMatrix)
    {
      Matrix4x4 trs = TransformViewMatrixToTRS(webVRViewMatrix);
      transform.localPosition = trs.GetColumn(3);
      transform.localRotation = Quaternion.LookRotation(trs.GetColumn(2), trs.GetColumn(1));
      transform.localScale = new Vector3(
          trs.GetColumn(0).magnitude,
          trs.GetColumn(1).magnitude,
          trs.GetColumn(2).magnitude
      );
    }

    // According to https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/#post-2367177
    public static Matrix4x4 TransformViewMatrixToTRS(Matrix4x4 openGLViewMatrix)
    {
      openGLViewMatrix.m20 *= -1;
      openGLViewMatrix.m21 *= -1;
      openGLViewMatrix.m22 *= -1;
      openGLViewMatrix.m23 *= -1;
      return openGLViewMatrix.inverse;
    }

    public static Vector3 GetTranslationFromMatrix(Matrix4x4 mat)
    {
      return mat.GetColumn(3);
    }

    public static Quaternion GetRotationFromMatrix(Matrix4x4 mat)
    {
      return Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
    }

    // Converts float array to Matrix4x4
    public static Matrix4x4 NumbersToMatrix(float[] array)
    {
      var mat = new Matrix4x4();
      mat.m00 = array[0];
      mat.m01 = array[1];
      mat.m02 = array[2];
      mat.m03 = array[3];
      mat.m10 = array[4];
      mat.m11 = array[5];
      mat.m12 = array[6];
      mat.m13 = array[7];
      mat.m20 = array[8];
      mat.m21 = array[9];
      mat.m22 = array[10];
      mat.m23 = array[11];
      mat.m30 = array[12];
      mat.m31 = array[13];
      mat.m32 = array[14];
      mat.m33 = array[15];
      return mat;
    }
  }
}
