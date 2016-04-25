Shader "LowpolyWater" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_EmissionColor("Emission", Color) = (0,0,0,1)
		_Ambient("Ambient Light Multiplier", Float) = 0.75
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
	_BumpMap("Normalmap", 2D) = "bump" {}
	[Toggle]_Animate("Animate Material", Float) = 1
		_WaveLength("Wave length", Float) = 0.5
		_WaveHeight("Wave height", Float) = 0.5
		_WaveSpeed("Wave speed", Float) = 1.0
		_RandomHeight("Random height", Float) = 0.5
		_RandomSpeed("Random Speed", Float) = 0.5
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

		Pass{
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_fog_exp2
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			float _WaveLength;
			float _WaveHeight;
			float _WaveSpeed;
			float _RandomHeight;
			float _RandomSpeed;

			float rand(float3 co)
			{
				return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
			}

			float rand2(float3 co)
			{
				return frac(sin(dot(co.xyz, float3(19.9128, 75.2, 34.5122))) * 12765.5213);
			}

			struct v2f {
				float4  pos         : SV_POSITION;
				float2  uv          : TEXCOORD0;
				float3	norm		: NORMAL;
				float3  viewDir     : TEXCOORD1;
				float3  lightDir    : TEXCOORD2;
				LIGHTING_COORDS(3,4)

			};

			v2f vert(appdata_tan v) {
				v2f o;
				float3 v0 = v.vertex.xyz;
				// Fake neighbor verts
				float3 v1 = v.vertex.xyz + float3(0.01, 0, 0);
				float3 v2 = v.vertex.xyz + float3(0, 0, 0.01);

				float phase0 = (_WaveHeight)* sin((_Time[1] * _WaveSpeed) + (v0.x * _WaveLength) + (v0.z * _WaveLength) + rand2(v0.xzz));
				float phase0_1 = (_RandomHeight)*sin(cos(rand(v0.xzz) * _RandomHeight * cos(_Time[1] * _RandomSpeed * sin(rand(v0.xxz)))));
				float phase1 = (_WaveHeight)* sin((_Time[1] * _WaveSpeed) + (v1.x * _WaveLength) + (v1.z * _WaveLength) + rand2(v1.xzz));
				float phase1_1 = (_RandomHeight)*sin(cos(rand(v1.xzz) * _RandomHeight * cos(_Time[1] * _RandomSpeed * sin(rand(v1.xxz)))));
				float phase2 = (_WaveHeight)* sin((_Time[1] * _WaveSpeed) + (v2.x * _WaveLength) + (v2.z * _WaveLength) + rand2(v2.xzz));
				float phase2_1 = (_RandomHeight)*sin(cos(rand(v2.xzz) * _RandomHeight * cos(_Time[1] * _RandomSpeed * sin(rand(v2.xxz)))));

				v0.y += phase0 + phase0_1;
				v1.y += phase1 + phase1_1;
				v2.y += phase2 + phase2_1;

				float3 approxNorm = cross(v2 - v0, v1 - v0);
				float3 vn = mul((_World2Object), approxNorm);
				v.normal = normalize(vn);

				v.vertex.xyz = v0;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
				o.norm = v.normal;
				TANGENT_SPACE_ROTATION;
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));

				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 _Color;
			uniform float4 _EmissionColor;
			uniform float _Ambient;
			sampler2D _MainTex;
			sampler2D _BumpMap;
			fixed4 _LightColor0;

			fixed4 frag(v2f i) : COLOR{
				fixed atten = LIGHT_ATTENUATION(i);

				i.uv.x += (_Time[1] * _WaveSpeed * 0.1);

				fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
				fixed3 normal = i.norm + UnpackNormal(tex2D(_BumpMap, i.uv));

				fixed diffuse = saturate(dot(normal, i.lightDir)) * _LightColor0.rgb * atten;
				fixed3 amb = UNITY_LIGHTMODEL_AMBIENT.rgb * _Ambient;

				fixed alpha = _Color.a;

				fixed3 lightFinal = tex.rgb * amb + tex.rgb * diffuse + _EmissionColor;
				return fixed4(lightFinal, alpha);
			}
			ENDCG
		}

		Pass{
				Name "ShadowCaster"
				Tags{ "LightMode" = "ShadowCaster" }

				Fog{ Mode Off }
				ZWrite On ZTest Less Cull Off
				Offset 1, 1

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"

				float _WaveLength;
				float _WaveHeight;
				float _WaveSpeed;
				float _RandomHeight;
				float _RandomSpeed;

				float rand(float3 co)
				{
					return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
				}

				float rand2(float3 co)
				{
					return frac(sin(dot(co.xyz, float3(19.9128, 75.2, 34.5122))) * 12765.5213);
				}

				struct v2f {
					V2F_SHADOW_CASTER;
				};

				v2f vert(appdata_base v) {
					v2f o;

					float3 v0 = v.vertex.xyz;
					float phase0 = (_WaveHeight)* sin((_Time[1] * _WaveSpeed) + (v0.x * _WaveLength) + (v0.z * _WaveLength) + rand2(v0.xzz));
					float phase0_1 = (_RandomHeight)*sin(cos(rand(v0.xzz) * _RandomHeight * cos(_Time[1] * _RandomSpeed * sin(rand(v0.xxz)))));

					v.vertex.y += phase0 + phase0_1;

					o.pos = v.vertex;

					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
						return o;
				}

				float4 frag(v2f i) : COLOR{
					SHADOW_CASTER_FRAGMENT(i)
				}

					ENDCG
				}
	}
		FallBack "Diffuse"
}