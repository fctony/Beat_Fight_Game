Shader "Custom/SimpleAlpha"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	Subshader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
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
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

					UNITY_TRANSFER_FOG(o, o.pos);
					return o;
				}

				uniform sampler2D _MainTex;
				uniform float4 _Color;

				float4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
					UNITY_APPLY_FOG(i.fogCoord, tex);
					return tex;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}
