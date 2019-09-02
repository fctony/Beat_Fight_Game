Shader "Custom/MatcapAdditive"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_AOMap ("Ambient Occlusion Map (RGB)", 2D) = "white" {}
		//_Offset ("Offset", Float) = 0
	}
	
	Subshader
	{
		Tags { "RenderType"="Opaque"}

		Pass
		{
			Tags { "LightMode" = "Always" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma shader_feature MATCAP_ACCURATE
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					float2 uv_bump : TEXCOORD1;
			
					float3 c0 : TEXCOORD2;
					float3 c1 : TEXCOORD3;
					UNITY_FOG_COORDS(4)
				};
				
				uniform float4 _MainTex_ST;
				uniform float4 _BumpMap_ST;
				//uniform float _Offset;
				
				v2f vert (appdata_tan v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					//o.pos.z = o.pos.z - _Offset;

					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.uv_bump = TRANSFORM_TEX(v.texcoord,_BumpMap);

					v.normal = normalize(v.normal);
					v.tangent = normalize(v.tangent);
					TANGENT_SPACE_ROTATION;
					o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
					o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));

					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}
				
				uniform sampler2D _MainTex;
				uniform sampler2D _AOMap;
				uniform sampler2D _BumpMap;
				uniform sampler2D _MatCap;
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv);
					fixed4 _AOTex = tex2D(_AOMap, i.uv);
					fixed3 normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));
					
					half2 capCoord = half2(dot(i.c0, normals), dot(i.c1, normals));
					float4 mc = (tex + (tex2D(_MatCap, capCoord*0.5 + 0.5)*2.0) - 1.0) * _AOTex;

					UNITY_APPLY_FOG(i.fogCoord, mc);
					return mc;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}
