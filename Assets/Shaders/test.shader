Shader "Custom/test" {
	Properties{
		_MainTex("Base (RGB) TransGloss (A)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		//_SpecColor("Specular Color", Color) = (1,1,1,1)
		//_Shininess("Shininess", Float) = 10
		_Attenuation("Attenuation", Range(0,1.5)) = 1
		_TimeVar("Time", Float) = 0
		_RandomSeed("Random Seed", Vector) = (1,1,1,1)
		_XHeightSpeedLengthOffset("X Height Speed Length Offset", Vector) = (1,1,1,1)
		_YHeightSpeedLengthOffset("Y Height Speed Length Offset", Vector) = (1,1,1,1)
		_ZHeightSpeedLengthOffset("Z Height Speed Length Offset", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags{ "RenderType" = "Fade" "Queue" = "Transparent" }
		Pass{
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			//user defined variables
			uniform float4 _Color;
			uniform float _Attenuation;
			uniform float _TimeVar;
			uniform float4 _RandomSeed;
			uniform float4 _XHeightSpeedLengthOffset;
			uniform float4 _YHeightSpeedLengthOffset;
			uniform float4 _ZHeightSpeedLengthOffset;

			//Unity defined variables
			uniform float4 _LightColor0;

			uniform sampler2D _MainTex;

			//base input structs
			struct vertexOutput {
				float4 pos       : SV_POSITION;
				float4 posWorld  : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
				float3 lightDir  : TEXCOORD2;
				float2 uv_MainTex : TEXCOORD3;
				LIGHTING_COORDS(4, 5)
			};

			//vertex function
			vertexOutput vert(appdata_tan v) {
				vertexOutput o;

				v.vertex.x += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _XHeightSpeedLengthOffset.w) * _XHeightSpeedLengthOffset.y + _XHeightSpeedLengthOffset.z) * _XHeightSpeedLengthOffset.x;
				v.vertex.y += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _YHeightSpeedLengthOffset.w) * _YHeightSpeedLengthOffset.y + _YHeightSpeedLengthOffset.z) * _YHeightSpeedLengthOffset.x;
				v.vertex.z += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _ZHeightSpeedLengthOffset.w) * _ZHeightSpeedLengthOffset.y + _ZHeightSpeedLengthOffset.z) * _ZHeightSpeedLengthOffset.x;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.posWorld = mul(_Object2World, v.vertex);
				o.normalDir = normalize(mul(float4(v.normal, 0), _World2Object).xyz);
				
				TANGENT_SPACE_ROTATION;
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;

			}

			//fragment function
			float4 frag(vertexOutput i) : COLOR{
				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float atten = LIGHT_ATTENUATION(i) * _Attenuation;

				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float lambert = max(0, dot(normalDirection, lightDirection));
				float3 diffuseReflection = _LightColor0.xyz * lambert;
				float3 lightFinal = atten * diffuseReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				return float4(lightFinal * _Color.rgb, _Color.a);
			}

			ENDCG
		}
	}
	
	//FallBack "Standard"
	FallBack "Diffuse"	
	//Fallback " VertexLit", 1
}
