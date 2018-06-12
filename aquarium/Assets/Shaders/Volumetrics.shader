Shader "Custom/Volumetrics" {
	Properties {
		_Fresnel("Fresnel", Range (0.0, 20.0)) = 10.0
		_AlphaOffset("Alpha offset", Range(0.0, 1.0)) = 1.0
		_NoiseSpeed("Noise speed", Range(0.0, 1.0)) = 0.5
		_Ambient("Ambient", Range(0.0, 1.0)) = 0.3
		_Intensity("Intensity", Range(0.0, 5.0)) = 0.2
		_Fade("Fade", Range(0.0, 10.0)) = 1.
		_Currents("Currents", Range(0.0, 1.0)) = 0.1
		_CurrentsSpeed("Currents speed", Range(0.0, 10.0)) = 1.0
		_StretchU("Stretch U", Range(0.0, 10.0)) = 1.0
		_StretchV("Stretch V", Range(0.0, 10.0)) = 1.0
	}
 
	SubShader {
		Tags {"RenderType" = "Transparent" "Queue" = "Transparent"} 
		LOD 100

		ZWrite Off
		Blend SrcAlpha One
     
		Pass {  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../Noise Shader/HLSL/ClassicNoise3D.hlsl"


			struct v_in {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
 
			struct f_in {
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				half2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
 
			float _Fresnel;
			float _AlphaOffset;
			float _NoiseSpeed;
			float _Ambient;
			float _Intensity;
			float _Fade;
			float _Currents;
			float _CurrentsSpeed;
			float _StretchU;
			float _StretchV;
             
			f_in vert (v_in v){
				f_in o;

				float noise = _Currents * cnoise(v.normal + _CurrentsSpeed * _Time.y);
				float4 nv = float4(v.vertex.xyz + noise * v.normal, v.vertex.w);
				o.vertex = UnityObjectToClipPos(nv);	
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.uv.x = v.uv.x * _StretchU;
				o.uv.y = v.uv.y * _StretchV;

				return o;
			}
             
			fixed4 frag (f_in i) : SV_Target{
				float nu = (i.uv.x < .5)? i.uv.x : (1. - i.uv.x);
				nu = pow(nu, 2.);
				float2 n_uv = float2(nu, i.uv.y);

				float n_a = cnoise(float3(n_uv * 5., 1.) + _Time.y * _NoiseSpeed * -1.) * _Intensity + _Ambient; 
				float n_b = cnoise(float3(n_uv * 10., 1.) + _Time.y * _NoiseSpeed * -1.) * .9; 
				float n_c = cnoise(float3(n_uv * 20., 1.) + _Time.y * _NoiseSpeed * -2.) * .9; 
				float n_d = pow(cnoise(float3(n_uv * 30., 1.) + _Time.y * _NoiseSpeed * -2.), 2.) * .9; 
				float noise = n_a + n_b + n_c + n_d;
				noise = (noise < 0.)? 0. : noise;
				float4 col = float4(noise, noise, noise, 1.);

				half3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); // camera-to-vertex
				float raycast = saturate(dot(viewDir, i.normal));
				float fresnel = pow(raycast, _Fresnel);

				float fade = saturate(pow(1. - i.uv.y, _Fade));

				col.a *= fresnel * _AlphaOffset * fade;

				return col;
			}
			ENDCG
		}
	}
}
