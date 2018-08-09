Shader "Custom/SeaweedShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_XSpeed("X speed", Range(0, 50)) = 19
		_YSpeed("Y speed", Range(0, 50)) = 23
		_Rigidity("Rigidity", Range(1, 50)) = 20
		_XSwayDepth("X Sway depth", Range(0, 1)) = 0.5
		_ZSwayDepth("Z Sway depth", Range(0, 1)) = 0.5
		_YOffset("Y offset (world coordinates)", float) = 0.5
		_AmbientIntensity("Ambient intensity", Range(0, 5)) = 1.0
		_DiffuseIntensity("Diffuse intensity", Range(0, 5)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 200
		Pass {
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct v_in {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct f_in {
				float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 worldNormal: TEXCOORD2;
			};

			sampler2D _MainTex;
            float4 _MainTex_ST;

			float _XSpeed;
			float _YSpeed;
			float _Rigidity;
			float _XSwayDepth;
			float _ZSwayDepth;
			float _YOffset;
			float _AmbientIntensity;
			float _DiffuseIntensity;

			f_in vert (v_in i) {
				f_in o;
				float3 wpos = mul(unity_ObjectToWorld, i.vertex.xyz);
				o.worldNormal = UnityObjectToWorldNormal(i.normal);
				float x = i.vertex.x;
				float z = i.vertex.z;
				if(wpos.y > _YOffset) {
					x = sin(wpos.x / _Rigidity + (_Time.x * _XSpeed)) * (wpos.y - _YOffset) * 5;
					z = sin(wpos.z / _Rigidity + (_Time.x * _YSpeed)) * (wpos.y - _YOffset) * 5;
				}
        //o.vertex = UnityObjectToClipPos(i.vertex);
				o.vertex = i.vertex;
				o.vertex.x += x * _XSwayDepth;
				o.vertex.y += z * _ZSwayDepth;
				o.vertex = UnityObjectToClipPos(o.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				return o;
			}

			fixed4 frag (f_in i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				float ldn = saturate(dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
				float4 diffuse = _DiffuseIntensity * _LightColor0 * ldn;
				diffuse *= col;
				diffuse.a = 1; // dubious
				float4 ambient = _AmbientIntensity * float4(UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb, 1);
				return col;
				//return diffuse + ambient;
			}
		ENDCG
		}

	}
	FallBack "Diffuse"
}
