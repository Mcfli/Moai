Shader "Custom/test" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_Shininess ("Shininess", Float) = 10
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

			//Unity defined variables
			uniform float4 _LightColor0;

			//base input structs
			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			//vertex function
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				
				float3 normalDirection = normalize(mul(float4(v.normal,0), _World2Object).xyz);
				float3 viewDirection = normalize(float3(float4(_WorldSpaceCameraPos.xyz, 1) - mul(_Object2World, v.vertex).xyz));
				float atten = 1;
				
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float lambert = max(0, dot(normalDirection, lightDirection));
				float3 diffuseReflection = atten * _LightColor0.xyz * lambert;
				float3 specularReflection = atten * _SpecColor.rgb * lambert * pow(max(0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				o.col = float4(lightFinal * _Color.rgb, 1);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			//fragment function
			float4 frag(vertexOutput i) : COLOR{
				return i.col;
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}
