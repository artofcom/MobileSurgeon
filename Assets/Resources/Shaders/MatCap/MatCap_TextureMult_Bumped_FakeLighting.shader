// Added Additive Effect Elements for Fake Lighting Effects, 2019 Matt Christopherson
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// MatCap Shader, (c) 2015 Jean Moreno

Shader "MatCap/Bumped/Textured Multiply Cutout Lighting FX"
{
	Properties
	{
	_MainTex("Base (RGB)", 2D) = "white" {}
	
	[Toggle(ENABLE_HSV)] _EnableHSV("Apply HSV to Main Texture", Int) = 0
	_Hue ("Hue", Range (-1.0, 1.0)) = 0
	_Saturation ("Saturation", Range (-1.0, 1.0)) = 0
	_Value ("Value", Range (-1.0, 1.0)) = 0

	_BumpMap("Normal Map", 2D) = "bump" {}
	_MatCap("MatCap (RGB)", 2D) = "white" {}
	_GammaAdj("Gamma Adjust (Final)", Range(0.0, 4.0)) = 1
	_Transparency("Transparency", Range(0.0, 1)) = 1

	_AddCoordX ("Additive Texture Offset X", Float) = 0.5
	_AddCoordY ("Additive Texture Offset Y", Float) = 0.5
	_AddCoordZ ("Additive Texture Offset Z", Float) = 0.5
	_AddTilingX ("Additive Texture Scale X", Float) = 1
	_AddTilingY ("Additive Texture Scale Y", Float) = 1

	_AddTex ("Additive Effects", 2D) = "White" {}
	_AddValue ("Additive Value", Range(0.0,5.0)) = 0
	_AddTint ("Tint Final Add Color", Color) = (0,0,0,0)

	[Toggle(MATCAP_ACCURATE)] _MatCapAccurate("(World Space) Accurate Calculation", Int) = 0

	}

		Subshader
	{
		Tags{ "RenderType" = "Transparent" }

		Pass
	{
		Tags{ "LightMode" = "Always" }
		
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma shader_feature MATCAP_ACCURATE
#pragma shader_feature ENABLE_HSV
#include "UnityCG.cginc"

#define CGCOLOR_EPSILON 1e-10

inline float3 HUEtoRGB(float h)
{
    float R = abs(h * 6 - 3) - 1;
    float G = 2 - abs(h * 6 - 2);
    float B = 2 - abs(h * 6 - 4);
    return saturate(float3(R,G,B));
}

inline float3 RGBtoHCV(float3 rgb)
{
    float4 P = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0/3.0) : float4(rgb.gb, 0.0, -1.0/3.0);
    float4 Q = (rgb.r < P.x) ? float4(P.xyw, rgb.r) : float4(rgb.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + CGCOLOR_EPSILON) + Q.z);
    return float3(H, C, Q.x);
}

inline float3 RGBtoHSV(float3 rgb)
{
    float3 hcv = RGBtoHCV(rgb);
    float S = hcv.y / (hcv.z + CGCOLOR_EPSILON);
    return float3(hcv.x, S, hcv.z);
}

inline float3 RGBtoHSL(float3 rgb)
{
    float3 hcv = RGBtoHCV(rgb);
    float L = hcv.z - hcv.y * 0.5;
    float S = hcv.y / (1 - abs(L * 2 - 1) + CGCOLOR_EPSILON);
    return float3(hcv.x, S, L);
}


inline float3 HSVtoRGB(float3 hsv)
{
    float3 rgb = HUEtoRGB(hsv.x);
    return ((rgb - 1) * hsv.y + 1) * hsv.z;
}

inline float3 HSLtoRGB(float3 hsl)
{
    float3 rgb = HUEtoRGB(hsl.x);
    float C = (1 - abs(2 * hsl.z - 1)) * hsl.y;
    return (rgb - 0.5) * C + hsl.z;
}


	struct v2f
	{
		float4 pos	: SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_bump : TEXCOORD1;

#if MATCAP_ACCURATE
		fixed3 tSpace0 : TEXCOORD2;
		fixed3 tSpace1 : TEXCOORD3;
		fixed3 tSpace2 : TEXCOORD4;
#else
		float3 c0 : TEXCOORD2;
		float3 c1 : TEXCOORD3;
		float3 c3 : TEXCOORD4;
		float3 c4 : TEXCOORD5;
#endif
	};

	uniform float4 _MainTex_ST;
	uniform float4 _BumpMap_ST;

	uniform float _AddTilingX;
	uniform float _AddTilingY;

	v2f vert(appdata_tan v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv_bump = TRANSFORM_TEX(v.texcoord,_BumpMap);

#if MATCAP_ACCURATE
		//Accurate bump calculation: calculate tangent space matrix and pass it to fragment shader
		fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
		fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
		fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
		o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x)* _AddTilingX;
		o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y)* _AddTilingY;
		o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
#else
		//Faster but less accurate method (especially on non-uniform scaling)
		v.normal = normalize(v.normal);
		v.tangent = normalize(v.tangent);
		TANGENT_SPACE_ROTATION;
		o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
		o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));

		o.c3 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz) * _AddTilingX);
		o.c4 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz) * _AddTilingY);
#endif
		return o;
	}

	uniform sampler2D _MainTex;
	uniform sampler2D _BumpMap;
	uniform sampler2D _MatCap;

	uniform float _GammaAdj;
	uniform float _Transparency;

	uniform float _AddCoordX;
	uniform float _AddCoordY;
	uniform float _AddCoordZ;

	uniform sampler2D _AddTex;
	uniform float _AddValue;
	uniform float4 _AddTint;

	uniform float _Hue;
    uniform float _Saturation;
    uniform float _Value;

	fixed4 frag(v2f i) : COLOR
	{
	//Adjust Hue, Saturation and Value of Main Texture Befor applying MatCap
#if ENABLE_HSV
	fixed4 tex = tex2D(_MainTex, i.uv);
				
	float3 hsv = RGBtoHSV(tex.rgb);
	float alpha = tex.a;
				
	hsv.x  = hsv.x + _Hue;
	if (hsv.x < 0) hsv.x += 1;
	if (hsv.x > 1) hsv.x -= 1;
				
	hsv.y += _Saturation;
	hsv.y = saturate(hsv.y);
				
	hsv.z += _Value;
	hsv.z = saturate(hsv.z);
				
	tex.rgb = HSVtoRGB(hsv);

#else
	fixed4 tex = tex2D(_MainTex, i.uv);
#endif

	
	fixed3 normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));

#if MATCAP_ACCURATE
	//Rotate normals from tangent space to world space
	float3 worldNorm;
	worldNorm.x = dot(i.tSpace0.xyz, normals);
	worldNorm.y = dot(i.tSpace1.xyz, normals);
	worldNorm.z = dot(i.tSpace2.xyz, normals);
	worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);

	float4 mc = tex2D(_MatCap, worldNorm.xy * 0.5 + 0.5);

	float3 worldNorm2;
	worldNorm2.x = dot(i.tSpace0.xyz, normals) + _AddCoordX;
	worldNorm2.y = dot(i.tSpace1.xyz, normals) + _AddCoordY;
	worldNorm2.z = dot(i.tSpace2.xyz, normals) + _AddCoordZ;
	worldNorm2 = mul((float3x3)UNITY_MATRIX_V, worldNorm2);

	fixed4 addtex = tex2D(_AddTex, worldNorm2 * 0.5);
	


#else
	half2 capCoord = half2(dot(i.c0, normals), dot(i.c1, normals));
	float4 mc = tex2D(_MatCap, capCoord * 0.5 + 0.5);
	half2 capCoord2 = half2(dot(i.c3, normals) + _AddCoordX, dot(i.c4, normals) + _AddCoordY);
	fixed4 addtex = tex2D(_AddTex, capCoord2);


#endif

	fixed4 addalpha = (addtex * _AddTint) * (addtex.a * _AddTint.a);
	fixed4 add = (addalpha * _AddValue);

	mc.rgb *= _GammaAdj;
	fixed4 final = tex * (mc + add);
	final.a *= _Transparency;

	return final;
	}
		ENDCG
	}
	}

		Fallback "VertexLit"
}
