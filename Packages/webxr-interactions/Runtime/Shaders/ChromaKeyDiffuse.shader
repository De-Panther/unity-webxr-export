Shader "WebXR Mixed Reality Capture/Chroma Key Diffuse" {
  Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _ThresholdMin ("Threshold Min", Color) = (0,0.39,0,1)
    _ThresholdMax ("Threshold Max", Color) = (0.23,1,0.23,1)
  }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    LOD 200

    CGPROGRAM
    #pragma surface surf Lambert alpha:blend

    sampler2D _MainTex;
    fixed4 _Color;
    fixed4 _ThresholdMin;
    fixed4 _ThresholdMax;

    struct Input {
      float2 uv_MainTex;
    };

    void surf (Input IN, inout SurfaceOutput o) {
      fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
      o.Albedo = c.rgb;
      o.Alpha = c.a;
      if (_ThresholdMin.r <= c.r && c.r <= _ThresholdMax.r
      &&_ThresholdMin.g <= c.g && c.g <= _ThresholdMax.g
      &&_ThresholdMin.b <= c.b && c.b <= _ThresholdMax.b)
      {
        o.Alpha = 0;
      }
    }
    ENDCG
  }

  Fallback "Legacy Shaders/VertexLit"
}
