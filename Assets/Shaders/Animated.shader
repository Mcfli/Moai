Shader "Animated" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_EmissionColor("Emission", Color) = (0,0,0,1)
		_Ambient("Ambient Light Multiplier", Float) = 0.75
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		[Toggle]_Animate("Animate Material", Float) = 1
		//[HideInInspector]_TimeVar("Time", Float) = 0
		_RandomSeed("Random Seed", Vector) = (12.9898,78.233,45.5432,0.043)
		_XHeightSpeedLengthOffset("X Height Speed Length Offset", Vector) = (0.05,1,0.1,0)
		_YHeightSpeedLengthOffset("Y Height Speed Length Offset", Vector) = (0,0,0,0)
		_ZHeightSpeedLengthOffset("Z Height Speed Length Offset", Vector) = (0,0,0,0)
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
			
			uniform float _Animate;
			uniform float _TimeVar;
			uniform float4 _RandomSeed;
			uniform float4 _XHeightSpeedLengthOffset;
			uniform float4 _YHeightSpeedLengthOffset;
			uniform float4 _ZHeightSpeedLengthOffset;

			struct v2f{
				float4  pos         : SV_POSITION;
				float2  uv          : TEXCOORD0;
				float3  viewDir     : TEXCOORD1;
				float3  lightDir    : TEXCOORD2;
				LIGHTING_COORDS(3,4)
				
			};

			v2f vert(appdata_tan v){
				v2f o;

				v.vertex.x += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _XHeightSpeedLengthOffset.w) * _XHeightSpeedLengthOffset.y + _XHeightSpeedLengthOffset.z) * _XHeightSpeedLengthOffset.x * _Animate;
				v.vertex.y += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _YHeightSpeedLengthOffset.w) * _YHeightSpeedLengthOffset.y + _YHeightSpeedLengthOffset.z) * _YHeightSpeedLengthOffset.x * _Animate;
				v.vertex.z += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _ZHeightSpeedLengthOffset.w) * _ZHeightSpeedLengthOffset.y + _ZHeightSpeedLengthOffset.z) * _ZHeightSpeedLengthOffset.x * _Animate;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
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
				i.viewDir = normalize(i.viewDir);
				i.lightDir = normalize(i.lightDir);

				fixed atten = LIGHT_ATTENUATION(i);

				fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
				fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));

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
			Tags { "LightMode" = "ShadowCaster" }
           
			Fog { Mode Off }
			ZWrite On ZTest Less Cull Off
			Offset 1, 1
             
			CGPROGRAM
 
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
             
			#include "UnityCG.cginc"

			uniform float _Animate;
			uniform float _TimeVar;
			uniform float4 _RandomSeed;
			uniform float4 _XHeightSpeedLengthOffset;
			uniform float4 _YHeightSpeedLengthOffset;
			uniform float4 _ZHeightSpeedLengthOffset;
 
			struct v2f{ 
				V2F_SHADOW_CASTER;
			};
           
			v2f vert(appdata_base v){
				v2f o;

				v.vertex.x += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _XHeightSpeedLengthOffset.w) * _XHeightSpeedLengthOffset.y + _XHeightSpeedLengthOffset.z) * _XHeightSpeedLengthOffset.x * _Animate;
				v.vertex.y += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _YHeightSpeedLengthOffset.w) * _YHeightSpeedLengthOffset.y + _YHeightSpeedLengthOffset.z) * _YHeightSpeedLengthOffset.x * _Animate;
				v.vertex.z += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _ZHeightSpeedLengthOffset.w) * _ZHeightSpeedLengthOffset.y + _ZHeightSpeedLengthOffset.z) * _ZHeightSpeedLengthOffset.x * _Animate;

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