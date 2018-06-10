Shader "Custom/Caustics" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CausticsIntensity("Caustics intensity", Range(0, 1)) = 0.5
		_CausticsWidth("Caustics width", Range(0, 0.5)) = 0.05
		_CausticsFeather("Caustics feather width", Range(0, 0.5)) = 0.1
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _CausticsIntensity;
			float _CausticsWidth;
			float _CausticsFeather;
			static const int OCTAVES = 5;

			float map(float x0, float y0, float x1, float y1, float v)
			{
				return (v - x0) / (y0 - x0) * (y1 - x1) + x1;
			}

			f_in vert (v_in i)
			{
				f_in o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (f_in i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				UNITY_APPLY_FOG(i.fogCoord, col);
				float caustics = 0;
				for(int octave = 1; octave <= OCTAVES; octave++) {
					caustics += (cnoise(float3(octave * i.uv.x, octave * i.uv.y, _Time.y)) / octave);
				}

				caustics = abs(caustics);

				float feather = _CausticsFeather;
				float width = _CausticsWidth;

				if(caustics < 0.5 - width - feather) caustics = 0.0;
				else if(caustics < 0.5 - width) caustics = map(0.5 - width - feather, 0.5 - width, 0.0, 1.0, caustics);
				else if(caustics < 0.5 + width) caustics = 1.0;
				else if(caustics < 0.5 + width + feather) caustics = map(0.5 + width, 0.5 + width + feather, 1.0, 0.0, caustics);
				else caustics = 0.0;

				caustics *= _CausticsIntensity;

				return col + caustics;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
