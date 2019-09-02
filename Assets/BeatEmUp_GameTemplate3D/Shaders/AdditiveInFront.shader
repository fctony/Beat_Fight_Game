Shader "Custom/AdditiveInFront" {

	Properties {
		_Color ("Main Color", COLOR) = (1,1,1,1)
	    _MainTex ("Particle Texture", 2D) = "white" {}
	}
	 
	Category {
	    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	    Blend SrcAlpha One
	    Cull Off Lighting Off ZWrite Off ZTest Always
	 
	    SubShader {
	        Pass {
	     
	            CGPROGRAM
	            #pragma vertex vert
	            #pragma fragment frag
	            #pragma multi_compile_fog
	 
	            #include "UnityCG.cginc"
	 
	            sampler2D _MainTex;
	            float4 _MainTex_ST;
	         
	            struct appdata_t {
	                float4 vertex : POSITION;
	                fixed4 color : COLOR;
	                float2 texcoord : TEXCOORD0;
	            };
	 
	            struct v2f {
	                float4 vertex : SV_POSITION;
	                fixed4 color : COLOR;
	                float2 texcoord : TEXCOORD0;
	                UNITY_FOG_COORDS(1)
	            };
	 
	            v2f vert (appdata_t v) {
	                v2f o;
	                o.vertex = UnityObjectToClipPos(v.vertex);
	                o.color = v.color;
	                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
	                UNITY_TRANSFER_FOG(o,o.vertex);
	                return o;
	            }
	         
	            fixed4 frag (v2f i) : SV_Target {            
	                fixed4 col = i.color * tex2D(_MainTex, i.texcoord);
	                UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
	                return col;
	            }
	            ENDCG
	        }
	    }
	}
}