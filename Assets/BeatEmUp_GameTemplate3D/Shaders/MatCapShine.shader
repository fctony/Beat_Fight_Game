Shader "Custom/MatCapShine"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_MatCapStrength ("MatCapStrength", Range (0,3)) = 1
		_ShineRamp ("ShineRamp", 2D) = "white" {}
		_ShineStrength  ("Shine Strength", Range (0,5)) = 2
		_ScrollSpeed ("Shine Scroll Speed", Float) = 10.0
	}
	
	Subshader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv1 : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
					float2 cap : TEXCOORD2;
				};

				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv1 	: TEXCOORD0;
					float2 uv2 	: TEXCOORD1;
					float2 cap	: TEXCOORD2;
					UNITY_FOG_COORDS(3)
				};

				uniform float4 _MainTex_ST;
				uniform float4 _ShineRamp_ST;
				uniform float _ScrollSpeed;
				uniform sampler2D _MainTex;
				uniform sampler2D _ShineRamp;
				uniform sampler2D _MatCap;
				uniform float _MatCapStrength;
				uniform float _ShineStrength;
				
				v2f vert (appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);

					//effect uvs
					o.uv2 = TRANSFORM_TEX(v.uv2, _ShineRamp) + frac(float2(_ScrollSpeed, _ScrollSpeed) * _Time);

					//view normals
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;

					//fog
					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}

				float4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv1);
					fixed4 shine = tex2D(_ShineRamp, i.uv2) * _ShineStrength;
					float4 matCap =  tex2D(_MatCap, i.cap) * _MatCapStrength;
					float4 col = tex * matCap * 2.0 + shine;  
					UNITY_APPLY_FOG(i.fogCoord, col);

					return col;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

