Shader "Hidden/AtmosphereHaze"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_fadeColor("Fade Color", Color) = (0,0,0,1)
		_fadeDistance("Fade Distance", float) = 1000.0
	}
		SubShader
		{
			// No culling or depth
			//Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				uniform fixed4 _fadeColor;
				uniform fixed _fadeDistance;

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 worldPos : TEXCOORD1;
					float4 vertex : SV_POSITION;
				};

				v2f vert (appdata v)
				{
					v2f o;
					o.worldPos = mul(_Object2World, v.vertex);
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.uv;
					return o;
				}
				
				sampler2D _MainTex;

				fixed4 frag (v2f i) : SV_Target
				{
					fixed cameraDist = length(i.worldPos.xyz - _WorldSpaceCameraPos.xyz);
					fixed fadeWeight = cameraDist / _fadeDistance;
					fadeWeight = clamp(fadeWeight, 0.0, 1.0);
					fixed4 col = tex2D(_MainTex, i.uv);
					fixed4 finalCol = col * (1.0 - fadeWeight) + _fadeColor * fadeWeight;

					return finalCol;
				}
				ENDCG
		}
	}
}
