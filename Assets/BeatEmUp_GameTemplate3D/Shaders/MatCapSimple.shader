Shader "Custom/MatCapSimple"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_MatCapStrength ("MatCapStrength", Range (0,3)) = 1
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
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 cap	: TEXCOORD0;
					float2 uv 	: TEXCOORD1;
					UNITY_FOG_COORDS(2)
				};

				uniform float4 _MainTex_ST;
				
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					UNITY_TRANSFER_FOG(o, o.pos);
					return o;
				}

				uniform sampler2D _MainTex;
				uniform float4 _Color;
				uniform sampler2D _MatCap;
				uniform float _MatCapStrength;
				
				float4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv);
					float4 mc =  tex * (tex2D(_MatCap, i.cap) * _MatCapStrength);
					mc = _Color * mc * 2.0;
					UNITY_APPLY_FOG(i.fogCoord, mc);

					return mc;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}
