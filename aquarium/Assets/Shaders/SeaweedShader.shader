﻿Shader "Custom/SeaweedShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_XSpeed("X speed", Range(10, 50)) = 19
		_YSpeed("Y speed", Range(10, 50)) = 23
		_Rigidity("Rigidity", Range(1, 50)) = 20
		_SwayDepth("Sway depth", Range(0, 1)) = 0.5
		_YOffset("Y offset", float) = 0.5
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
			//#pragma addshadow

			#include "UnityCG.cginc"

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

			float _XSpeed;
			float _YSpeed;
			float _Rigidity;
			float _SwayDepth;
			float _YOffset;

			f_in vert (v_in i)
            {
                f_in o;
				float3 wpos = mul(unity_ObjectToWorld, i.vertex.xyz);
				float x = sin(wpos.x / _Rigidity + (_Time.x * _XSpeed)) * (i.vertex.y - _YOffset) * 5;
				float z = sin(wpos.z / _Rigidity + (_Time.x * _YSpeed)) * (i.vertex.y - _YOffset) * 5;
                //o.vertex = UnityObjectToClipPos(i.vertex);
				o.vertex = i.vertex;
				o.vertex.x += x * _SwayDepth;
				o.vertex.y += z * _SwayDepth;
				o.vertex = UnityObjectToClipPos(o.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            fixed4 frag (v_in i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
		}

	}
	FallBack "Diffuse"
}
