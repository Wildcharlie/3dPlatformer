Shader "Custom/Water3" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,0,0,0)		
		_WaveSpeed ("Wave Speed", Float) = 1.0
		_WaveHeight ("Wave Height", Float) = 2.0
		_WaveFrequency ("Wave Frequency", Float) = 0.7
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#include "AutoLight.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
		#pragma vertex vert 			
		#pragma surface surf Lambert vertex:vert addshadow

		sampler2D _MainTex;
		float4 _Color;
		float2 _WaveSpeed;
		float2 _WaveHeight;
		float2 _WaveFrequency;
		
		struct Input {
			float2 uv_MainTex;
			//LIGHTING_COORDS(0,1) // replace 0 and 1 with the next available TEXCOORDs in your shader, don't put a semicolon at the end of this line.

		};

		void vert (inout appdata_full v) {
			float phase = _Time * _WaveSpeed;
			float4 wpos = mul( _Object2World, v.vertex);
			float offset = (wpos.x + (wpos.z * 0.2)) * _WaveFrequency;
			wpos.y = wpos.y + (sin(phase + offset) * _WaveHeight);
			v.vertex = mul(_World2Object, wpos);
			//TRANSFER_VERTEX_TO_FRAGMENT(o); // Calculates shadow and light attenuation and passes it to the frag shader.

		}		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = _Color; // tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			//float atten = LIGHT_ATTENUATION(_); // This is a float for your shadow/attenuation value, multiply your lighting value by this to get shadows. Replace i with whatever you've defined your input struct to be called (e.g. frag(v2f [b]i[/b]) : COLOR { ... ).

		}
		ENDCG
	} 
	FallBack "Diffuse"
}