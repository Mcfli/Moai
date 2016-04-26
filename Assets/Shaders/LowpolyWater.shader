
Shader "Trolltunga/LowPolyWaves 2.0"
{
	Properties
	{
		_Color("Color", Color) = (1,0,0,1)
		_SpecColor("Specular Material Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Float) = 1.0
		_WaveLength("Wave length", Float) = 0.5
		_WaveHeight("Wave height", Float) = 0.5
		_WaveSpeed("Wave speed", Float) = 1.0
		_RandomHeight("Random height", Float) = 0.5
		_RandomSpeed("Random Speed", Float) = 0.5
	}
		SubShader
	{

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			float rand(float3 co)
			{
				return frac(sin(dot(co.xyz ,float3(12.9898,78.233,45.5432))) * 43758.5453);
			}

			float rand2(float3 co)
			{
				return frac(sin(dot(co.xyz ,float3(19.9128,75.2,34.5122))) * 12765.5213);
			}

			float _WaveLength;
			float _WaveHeight;
			float _WaveSpeed;
			float _RandomHeight;
			float _RandomSpeed;

			uniform float4 _LightColor0;

			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;

			struct v2g
			{
				float4  pos : SV_POSITION;
				float3	norm : NORMAL;
				float2  uv : TEXCOORD0;
				float3  viewDir     : TEXCOORD1;
				float3  lightDir    : TEXCOORD2;
				LIGHTING_COORDS(3, 4)
			};

			struct g2f
			{
				float4  pos : SV_POSITION;
				float3  norm : NORMAL;
				float2  uv : TEXCOORD0;
				float3 diffuseColor : TEXCOORD1;
				float3 specularColor : TEXCOORD2;
			};

			v2g vert(appdata_full v)
			{
				float3 v0 = v.vertex.xyz;

				float phase0 = (_WaveHeight)* sin((_Time[1] * _WaveSpeed) + (v0.x * _WaveLength) + (v0.z * _WaveLength) + rand2(v0.xzz));
				float phase0_1 = (_RandomHeight)*sin(cos(rand(v0.xzz) * _RandomHeight * cos(_Time[1] * _RandomSpeed * sin(rand(v0.xxz)))));

				v0.y += phase0 + phase0_1;

				v.vertex.xyz = v0;

				v2g OUT;
				OUT.pos = v.vertex;
				OUT.norm = v.normal;
				OUT.uv = v.texcoord;
				TANGENT_SPACE_ROTATION;
				OUT.viewDir = mul(rotation, normalize(ObjSpaceViewDir(v.vertex)));
				OUT.lightDir = mul(rotation, normalize(ObjSpaceLightDir(v.vertex)));
				return OUT;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[1].pos.xyz;
				float3 v2 = IN[2].pos.xyz;

				//float3 centerPos = (v0 + v1 + v2) / 3.0;

				float3 normalDirection = normalize(cross(v1 - v0, v2 - v0));

				float3 viewDirection = normalize((IN[0].viewDir + IN[1].viewDir + IN[2].viewDir)/3);
				float3 lightDirection = normalize((IN[0].lightDir + IN[1].lightDir + IN[2].lightDir)/3) ;
				float attenuation = 1.0;

				float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb;

				float3 diffuseReflection =
					saturate(dot(normalDirection, lightDirection)) * attenuation;

				float3	specularReflection = attenuation 
						* _SpecColor.rgb * pow(max(0.0, dot(
							reflect(-lightDirection, normalDirection),
							viewDirection)), _Shininess);
				

				float3 diff =  _Color.rgb * diffuseReflection ;

				g2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, IN[0].pos);
				OUT.norm = normalDirection;
				OUT.uv = IN[0].uv;
				OUT.diffuseColor = diff;
				OUT.specularColor = specularReflection;
				triStream.Append(OUT);

				OUT.pos = mul(UNITY_MATRIX_MVP, IN[1].pos);
				OUT.norm = normalDirection;
				OUT.uv = IN[1].uv;
				OUT.diffuseColor = diff;
				OUT.specularColor = specularReflection;
				triStream.Append(OUT);

				OUT.pos = mul(UNITY_MATRIX_MVP, IN[2].pos);
				OUT.norm = normalDirection;
				OUT.uv = IN[2].uv;
				OUT.diffuseColor = diff;
				OUT.specularColor = specularReflection;
				triStream.Append(OUT);
			}

			half4 frag(g2f IN) : COLOR
			{
				return float4(
				IN.diffuseColor + IN.specularColor , _Color.a);
			}

			ENDCG
		}
	}
}