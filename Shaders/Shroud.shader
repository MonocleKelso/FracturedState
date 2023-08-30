Shader "FracturedState/Shroud" {
	Properties {
		_MainTex ("Render Input", 2D) = "white" {}
		_ShroudTex ("Shroud Texture", 2D) = "white" {}
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode Off }
		Pass {
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				sampler2D _MainTex;
				sampler2D _ShroudTex;
			
				float4 frag(v2f_img IN) : COLOR {
					half4 c = tex2D (_MainTex, IN.uv);
					half4 s = tex2D(_ShroudTex, IN.uv);
					return c * s;
				}
			ENDCG
		}
	}
}