/*======= (c) Blueprint Reality Inc., 2017. All rights reserved =======*/
Shader "MixCast/ChromaKey/Depth" {

	Properties{
		_MainTex("Base(RGB)", 2D) = "white"{}

		_KeyHsvMid("Key HSV Mid", Vector) = (0,0,0)
		_KeyHsvRange("Key HSV Range", Vector) = (0, 0, 0)
		_KeyHsvFeathering("Key HSV Feathering", Vector) = (0,0,0)
		_KeyHsvWeights("Key HSV Weight", Vector) = (1, 1, 1)

		_PlayerDepth("Player Depth", Range(0,1)) = 0
		_GroundHeight("Ground Height", Float) = 0

		_ColorExponent("Exponent", Float) = 1
	}

		SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

		//ColorMask RGB
		Lighting Off
		//AlphaTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "BPR_ShaderHelpers.cginc"

		sampler2D _MainTex;
	float4 _TextureTransform;

	float3 _KeyHsvMid;
	float3 _KeyHsvRange;
	float3 _KeyHsvFeathering;
	float3 _KeyHsvWeights;

	fixed _PlayerDepth;
	fixed _GroundHeight;

	float _ColorExponent = 1;

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 pos : SV_POSITION;
		float4 scrPos : TEXCOORD1;
		//float4 worldPos : TEXCOORD3;

		float3 viewT : TEXCOORD4;
	};

	v2f vert(appdata_full v)
	{
		v2f o;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.scrPos = ComputeScreenPos(o.pos);
		o.uv = v.texcoord;

		o.viewT = WorldSpaceViewDir(v.vertex);

		return o;
	}

	struct frag_output {
		float4 col:COLOR;
		float dep : DEPTH;
	};

	frag_output frag(v2f i) {
		frag_output o;

		float2 uvs = i.uv * _TextureTransform.xy + _TextureTransform.zw;
		clip(0.5 - abs(uvs.x - 0.5));	//instead of clamping the texture, just cut it off at the edges

		float3 inputRGB = tex2D(_MainTex, uvs).rgb;

		float outputAlpha = CalculateChromaAlpha(inputRGB, _KeyHsvMid, _KeyHsvRange, _KeyHsvFeathering, _KeyHsvWeights);
		clip(outputAlpha - 0.05);

		inputRGB = pow(inputRGB, _ColorExponent);
		o.col = float4(inputRGB, outputAlpha);

		fixed4 scrUvs = UNITY_PROJ_COORD(i.scrPos);

		float depth = _PlayerDepth;

		float3 worldPos = 0;

		if (worldPos.y < _GroundHeight)
		{
			float3 rayOrigin = _WorldSpaceCameraPos;
			float3 rayDir = normalize(i.viewT);
			float dist = (_GroundHeight - _WorldSpaceCameraPos.y) / i.viewT.y;
			if (dist > 0) {
				float3 rayHit = rayOrigin + dist * rayDir;

				float3 viewDir = normalize(UNITY_MATRIX_IT_MV[2].xyz);
				float groundDepth = dot(viewDir, rayHit - rayOrigin);

				depth = 0;// (groundDepth - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y) - 0.0001;
			}
		}

		o.dep = Buffer01Depth(depth);

		return o;
	}
	ENDCG
	}
	}

		FallBack "Unlit"
}