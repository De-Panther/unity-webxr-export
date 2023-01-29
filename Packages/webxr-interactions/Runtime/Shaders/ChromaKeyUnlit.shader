Shader "WebXR Mixed Reality Capture/Chroma Key Unlit" {
  Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _LightingTex ("Lighting (RGB)", 2D) = "white" {}
    _ThresholdMin ("Threshold Min", Color) = (0,0.39,0,1)
    _ThresholdMax ("Threshold Max", Color) = (0.23,1,0.23,1)
  }
  SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
    LOD 200

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0
      #pragma multi_compile_fog
      #include "UnityCG.cginc"

      struct appdata_t {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        float2 texcoord2 : TEXCOORD1;
        UNITY_FOG_COORDS(2)
        UNITY_VERTEX_OUTPUT_STEREO
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      sampler2D _LightingTex;
      float4 _LightingTex_ST;
      fixed4 _Color;
      fixed4 _ThresholdMin;
      fixed4 _ThresholdMax;

      v2f vert (appdata_t v)
      {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.texcoord2 = TRANSFORM_TEX(v.texcoord, _LightingTex);
        UNITY_TRANSFER_FOG(o,o.vertex);
        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        fixed4 ligthing = tex2D(_LightingTex, i.texcoord2);
        fixed4 color = tex2D(_MainTex, i.texcoord) * ligthing * _Color;
        if (_ThresholdMin.r <= color.r && color.r <= _ThresholdMax.r
        &&_ThresholdMin.g <= color.g && color.g <= _ThresholdMax.g
        &&_ThresholdMin.b <= color.b && color.b <= _ThresholdMax.b)
        {
          color.a = 0;
        }
        UNITY_APPLY_FOG(i.fogCoord, color);
        return color;
      }
      ENDCG
    }
  }

}
