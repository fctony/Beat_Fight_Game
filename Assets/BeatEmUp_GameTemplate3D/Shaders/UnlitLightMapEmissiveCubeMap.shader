Shader "Custom/UnlitLightMapEmissiveCubeMap"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LightMap ("LightMap", 2D) = "black" {}
        _Cube ("Cubemap", CUBE) = "" {}
        _Emission  ("Emission", Range (0,1)) = 1
        _CubeStrength  ("CubeMapStrength", Range (0,1)) = 1
        _LightStrength  ("light Strength", Range (0,1)) = 1
 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        CGPROGRAM
        #pragma surface surf Lambert
 
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_LightMap;
            float3 worldRefl;
            float3 viewDir;
        };

        uniform sampler2D _LightMap;
        uniform sampler2D _MainTex;
        uniform samplerCUBE _Cube;
        uniform float _Emission;
        uniform float _CubeStrength;
        uniform float _LightStrength;
 
        void surf (Input IN, inout SurfaceOutput o)
        {
        	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
        	fixed4 lm = tex2D(_LightMap, IN.uv_LightMap);
           	o.Albedo = tex;
            o.Emission = tex * _Emission + (texCUBE (_Cube, IN.worldRefl).rgb - tex.a) * _CubeStrength + ((lm * _LightStrength) / tex.a/2);
        }
        ENDCG
    }
    Fallback "Unlit/Texture"
}