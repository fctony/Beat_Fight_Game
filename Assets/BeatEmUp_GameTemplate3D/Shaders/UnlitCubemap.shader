Shader "Custom/UnlitCubemap" {

    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Cube ("Cube env tex", CUBE) = "black" {}
      _MixPower ("Mix Power", Range (1, 0.01)) = 0.5
    }

    SubShader {
      Tags { "RenderType" = "Opaque" }

      CGPROGRAM
      #pragma surface surf Lambert

      struct Input {
          float2 uv_MainTex;
          float3 worldRefl;
      };

      sampler2D _MainTex;
      samplerCUBE _Cube;
      fixed _MixPower;
 
      void surf (Input IN, inout SurfaceOutput o) {
        fixed4 mytex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = mytex.rgb * 0.5;
		o.Emission = texCUBE (_Cube, IN.worldRefl).rgb * mytex.a;
      }
      ENDCG
    }
    Fallback "Diffuse"
}
