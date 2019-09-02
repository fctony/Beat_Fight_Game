Shader "Custom/UnlitLightMapEmissive"
{
	Properties
	{
	 	_Color ("Main Color", COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_LightMap ("LightMap", 2D) = "white" {}
		_LightStrength  ("light Strength", Range (0,5)) = 1
		_Emission  ("Emission", Range (0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 vertex : SV_POSITION;
			};

			uniform sampler2D _MainTex;
			uniform sampler2D _LightMap;
			uniform float _LightStrength;
			uniform float _Emission;
			fixed4 _Color;
			float4 _MainTex_ST;
			float4 _LightMap_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _LightMap);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 lm = tex2D(_LightMap, i.uv2);
				float4 col = tex * ((lm * _LightStrength) + _Emission) * _Color;

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
