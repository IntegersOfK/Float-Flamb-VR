/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
Shader "MixCast/ChromaKey/Flat" {

	Properties{
		_MainTex("Base(RGB)", 2D) = "white"{}
		
		_keyColor("Key Colour", Color) = (1,1,1,1)
		_keyLimits("Key Limits", Vector) = (0.2, 0.2, 0.2)
		_keyFeathering("Key Feathering", Vector) = (0,0,0)
		_keyWeights("Channel Factors", Vector) = (1, 1, 1)
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Lighting Off
		ZWrite Off
		ZTest Off
		AlphaTest LEqual 0.1
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			sampler2D _MainTex;
			float4 _TextureTransform;	//Blit command forces _MainTex_ST to (1,1)(0,0) so we sidestep that here

			float3 _keyColor;
			float3 _keyLimits;
			float3 _keyFeathering;
			float3 _keyWeights;

			float _colorExp = 1;

			#include "UnityCG.cginc"
			#include "BPR_ShaderHelpers.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				float3 keyHSV : TEXCOORD1;
			};

			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;

				o.keyHSV = RGB_to_HSV(_keyColor);

				return o;
			}

			float4 frag(v2f i) : COLOR {
				float2 uvs = i.uv * _TextureTransform.xy + _TextureTransform.zw;
				clip(0.5 - abs(uvs.x - 0.5));	//instead of clamping the texture, just cut it off at the edges

				float3 inputRGB = tex2D(_MainTex, uvs).rgb;
				inputRGB = pow(inputRGB, _colorExp);
				
				float outputAlpha = CalculateChromaAlpha(inputRGB, i.keyHSV, _keyLimits, _keyFeathering, _keyWeights);
				return float4(inputRGB, outputAlpha);
			}
		ENDCG
		}
	}

	FallBack "Unlit"
}