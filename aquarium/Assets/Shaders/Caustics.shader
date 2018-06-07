Shader "Custom/Caustics" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CausticsIntensity("Caustics intensity", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "../Noise Shader/HLSL/ClassicNoise3D.hlsl"

			struct v_in {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct f_in {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _CausticsIntensity;
			static const int OCTAVES = 5;

			f_in vert (v_in i)
			{
				f_in o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				return o;
			}

			fixed4 frag (f_in i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 caustics = fixed4(0, 0, 0, 0);
				for(int octave = 1; octave <= OCTAVES; octave++) {
					caustics.rgb += (cnoise(float3(octave * i.uv.x, octave * i.uv.y, _Time.y)) / octave);
				} // it sure ain't pretty...
				caustics -= fixed4(0.7, 0.7, 0.7, 0);
				caustics.x = abs(caustics.x);
				caustics.y = abs(caustics.y);
				caustics.z = abs(caustics.z);
				caustics = fixed4(1, 1, 1, 1) - caustics;
				caustics.x = abs(caustics.x);
				caustics.y = abs(caustics.y);
				caustics.z = abs(caustics.z);
				caustics *= caustics * caustics;
				caustics *= _CausticsIntensity;
				return col + caustics;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
