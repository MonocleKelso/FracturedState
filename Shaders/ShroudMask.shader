Shader "FracturedState/ShroudObject" {
	Properties {
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader {
		Lighting Off
		Cull Off
		Fog { Mode Off }
		Tags { "RenderType"="Opaque" }
		Color[_Color]
		Pass { }
	} 
	FallBack "VertexLit"
}
