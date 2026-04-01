Shader "zerinlabs/sh_cartoonWater_distortion"
{

	Properties
	{
		_colEdge1("Edge 1 colour", color) = (1.0, 1.0, 1.0, 1.0)
		_colEdge2("Edge 2 colour", color) = (0.07, 0.4, 0.73, 1.0)
		_colWater("Water colour", color) = (0.1, 0.48, 0.87, 1.0)

		_DM("DistorsionMap", 2D) = "white" {}

		_Size("DistorsionMap size",float) = 0.1
		_Freq("Frequency",float) = 0.1
		_Amp("Amplitude",float) = 0.1

		_Edge1step("Edge 1 step",Range(0, 1)) = 0.9
		_Edge2step("Edge 2 step",Range(0, 1)) = 0.8
	}

	SubShader
	{

		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		LOD 200

		Lighting Off

		CGPROGRAM

		#include "UnityCG.cginc"

		#pragma surface surf Standard alphatest:_Cutoff vertex:vert addshadow

		uniform sampler2D 	_DM;
		
		uniform fixed4 _colEdge1;
		uniform fixed4 _colEdge2;
		uniform fixed4 _colWater;

		uniform float _Size;
		uniform float _Freq;
		uniform float _Amp;

		uniform float _Edge1step;
		uniform float _Edge2step;

		struct Input
		{
			float2 dist_uv;
			float4 color : COLOR;

			float3 worldPos; //world mapping coords
		};

		/*
		struct appdata_full 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			fixed4 color : COLOR;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			half4 texcoord2 : TEXCOORD2;
			half4 texcoord3 : TEXCOORD3;
			half4 texcoord4 : TEXCOORD4;
			half4 texcoord5 : TEXCOORD5;
		};
		*/
		
		uniform float4 _DM_ST;

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			float2 dist = float2(cos(_Time.y * _Freq), sin(_Time.y* _Freq)) * _Amp;

			//o.dist_uv = TRANSFORM_TEX(v.texcoord, _DM) + dist;
			o.dist_uv = worldPos.xz * _Size + dist;

			o.color = v.color;
		}

		/*
		https://docs.unity3d.com/Manual/SL-SurfaceShaders.html

		struct SurfaceOutputStandard
		{
		fixed3 Albedo;      // base (diffuse or specular) color
		fixed3 Normal;      // tangent space normal, if written
		half3 Emission;
		half Metallic;      // 0=non-metal, 1=metal
		half Smoothness;    // 0=rough, 1=smooth
		half Occlusion;     // occlusion (default 1)
		fixed Alpha;        // alpha for transparencies
		};
		*/

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 dm = tex2D(_DM, IN.dist_uv);

			float VC = 1.0 - IN.color.r;

			float mk1 = clamp((dm.r + VC) * VC, 0.0, 1.0);
			float mk2 = clamp((dm.g + VC) * VC, 0.0, 1.0);

			mk1 = step(_Edge1step, mk1);
			mk2 = step(_Edge2step, mk2);

			float4 finalCol = lerp(_colWater, _colEdge2, mk2);
			finalCol = lerp(finalCol, _colEdge1, mk1);

			o.Albedo = finalCol.rgb;
		}

		ENDCG
		}
				 
		Fallback "Diffuse"
}