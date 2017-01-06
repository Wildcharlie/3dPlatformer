Shader "Custom/Water2" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
		#pragma vertex vert 			
		#pragma surface surf Lambert

		sampler2D _MainTex;
		float4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void vert (inout appdata_full v) {
			float phase = _Time * 40.0; // wave speed
			float4 wpos = mul( _Object2World, v.vertex);
			float offset = (wpos.x + (wpos.z * 0.2)) * 200; // wave density
			wpos.y = sin(phase + offset) * 0.15; // wave height
			v.vertex = mul(_World2Object, wpos);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color; // color + texture
			//fixed4 c = _Color; // only color
			o.Albedo = c.rgb;
			o.Alpha = c.a;			
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
