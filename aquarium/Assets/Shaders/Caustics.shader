Shader "Custom/Caustics" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CausticsIntensity("Caustics intensity", Range(0, 1)) = 0.5
		_CausticsWidth("Caustics width", Range(0, 0.5)) = 0.05
		_CausticsFeather("Caustics feather width", Range(0, 0.5)) = 0.1
		_CausticsSpeed("Caustics speed", Float) = 0.5
		_CausticsScale("Caustics scale", Float) = 1.0
        _LightColor("Light color", Color) = (1, 1, 1, 1)
        _LightDirection("Light direction", Vector) = (1, 2, 0, 0)
		_AmbientIntensity("Ambient intensity", Range(0, 5)) = 1.0
		_DiffuseIntensity("Diffuse intensity", Range(0, 5)) = 1.0
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
			#include "Lighting.cginc"

			struct v_in {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct f_in {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 world : TEXCOORD2;
				float3 worldNormal: TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _CausticsIntensity;
			float _CausticsWidth;
			float _CausticsFeather;
			float _CausticsSpeed;
			float _CausticsScale;
			float _AmbientIntensity;
			float _DiffuseIntensity;
            float4 _LightDirection;
            float4 _LightColor;
			static const int OCTAVES = 5;

			float map(float x0, float y0, float x1, float y1, float v)
			{
				return (v - x0) / (y0 - x0) * (y1 - x1) + x1;
			}

            #include "ClassicNoise3D.hlslinc"
   
            
			f_in vert (v_in i)
			{
				f_in o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.world = mul(unity_ObjectToWorld, i.vertex);
				o.worldNormal = UnityObjectToWorldNormal(i.normal);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (f_in i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				UNITY_APPLY_FOG(i.fogCoord, col);
				float caustics = 0;
				// float3 lightDirection = _WorldSpaceLightPos0.xyz; // This variable is zero in build
                float3 lightDirection = normalize(_LightDirection);
				float causticsX = i.world.x / 100.0f;
				float causticsY = i.world.z / 100.0f;
				for(int octave = 1; octave <= OCTAVES; octave++) {
					caustics += (cnoise(float3(_CausticsScale * octave * causticsX, _CausticsScale * octave * causticsY, _CausticsSpeed * _Time.y)) / octave);
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
                
				float ldn = saturate(dot(i.worldNormal, lightDirection));
				caustics *= ldn;
    
				float4 diffuse = _DiffuseIntensity * _LightColor * ldn;

				float4 ambient = _AmbientIntensity * _LightColor * col;
    
				return col * diffuse + ambient + caustics;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
