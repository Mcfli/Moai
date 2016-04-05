Shader "Custom/test" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Float) = 10
		_TimeVar("Time", Float) = 0
		_ChunkPosition("Chunk Position", Vector) = (0,0,0,0) // only use x and z
		_WaveHeight("Wave Height", Vector) = (1,1,1,1) // Y is diagonal
		_WaveSpeed("Wave Speed", Vector) = (1,1,1,1)   // W doesn't do anything
		_WaveLength("Wave Length", Vector) = (1,1,1,1)
		_WaveOffset("Wave Offset", Vector) = (1,1,1,1)
		[Toggle] _Invert("Invert Every Other", Float) = 1
	}
	SubShader {
		Pass{
			Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM
			//pragmas
			#pragma vertex vert
			#pragma fragment frag
			
			//user defined variables
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;
			uniform float _TimeVar;
			uniform float4 _ChunkPosition;
			uniform float4 _WaveHeight;
			uniform float4 _WaveSpeed;
			uniform float4 _WaveLength;
			uniform float4 _WaveOffset;
			uniform float _Invert;

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
				v.vertex.y =  sin((_TimeVar + _WaveOffset.x) * _WaveSpeed.x + (_ChunkPosition.x + v.vertex.x) * _WaveLength.x) * _WaveHeight.x;
				v.vertex.y += sin((_TimeVar + _WaveOffset.y) * _WaveSpeed.y + (_ChunkPosition.x + _ChunkPosition.z + v.vertex.x + v.vertex.z) * _WaveLength.y) * _WaveHeight.y;
				v.vertex.y += sin((_TimeVar + _WaveOffset.z) * _WaveSpeed.z + (_ChunkPosition.z + v.vertex.z) * _WaveLength.z) * _WaveHeight.z;

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
				float3 specularReflection = atten * _SpecColor.rgb * lambert * pow(max(0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				return float4(lightFinal * _Color.rgb, 1);
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}
