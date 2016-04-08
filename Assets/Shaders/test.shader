Shader "Custom/test" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		//_SpecColor("Specular Color", Color) = (1,1,1,1)
		//_Shininess("Shininess", Float) = 10
		_TimeVar("Time", Float) = 0
		_RandomSeed("Random Seed", Vector) = (1,1,1,1)
		_XHeightSpeedLengthOffset("X Height Speed Length Offset", Vector) = (1,1,1,1)
		_YHeightSpeedLengthOffset("Y Height Speed Length Offset", Vector) = (1,1,1,1)
		_ZHeightSpeedLengthOffset("Z Height Speed Length Offset", Vector) = (1,1,1,1)
	}
	SubShader {
		Pass{
			Tags{"Queue" = "Transparent" "LightMode" = "ForwardBase"}
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			//user defined variables
			uniform float4 _Color;
			//uniform float4 _SpecColor;
			//uniform float _Shininess;
			uniform float _TimeVar;
			uniform float4 _RandomSeed;
			uniform float4 _XHeightSpeedLengthOffset;
			uniform float4 _YHeightSpeedLengthOffset;
			uniform float4 _ZHeightSpeedLengthOffset;

			//Unity defined variables
			uniform float4 _LightColor0;

			//base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXTCOORD0;
				float3 normalDir : TEXTCOORD1;
			};

			//vertex function
			vertexOutput vert(vertexInput v) {
				vertexOutput o;

				//float displacement = 5;
				//float4 displacementDirection = float4(normalDirection.x, normalDirection.y, normalDirection.z, 0);
				//float4 newpos = v.vertex + displacement * displacementDirection;
				v.vertex.x += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _XHeightSpeedLengthOffset.w) * _XHeightSpeedLengthOffset.y + _XHeightSpeedLengthOffset.z) * _XHeightSpeedLengthOffset.x;
				v.vertex.y += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _YHeightSpeedLengthOffset.w) * _YHeightSpeedLengthOffset.y + _YHeightSpeedLengthOffset.z) * _YHeightSpeedLengthOffset.x;
				v.vertex.z += sin((_TimeVar + dot(v.vertex.xyz, _RandomSeed.xyz) * _RandomSeed.w + _ZHeightSpeedLengthOffset.w) * _ZHeightSpeedLengthOffset.y + _ZHeightSpeedLengthOffset.z) * _ZHeightSpeedLengthOffset.x;

				o.posWorld = mul(_Object2World, v.vertex);
				o.normalDir = normalize(mul(float4(v.normal, 0), _World2Object).xyz);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;

			}

			//fragment function
			float4 frag(vertexOutput i) : COLOR{
				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float atten = 1;

				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float lambert = max(0, dot(normalDirection, lightDirection));
				float3 diffuseReflection = atten * _LightColor0.xyz * lambert;
				//float3 specularReflection = atten * _SpecColor.rgb * lambert * pow(max(0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + /*specularReflection + */UNITY_LIGHTMODEL_AMBIENT.xyz;

				return float4(lightFinal * _Color.rgb, _Color.a);
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}
