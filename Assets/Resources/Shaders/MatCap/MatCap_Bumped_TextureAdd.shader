// MatCap Shader, (c) 2015 Jean Moreno

Shader "MatCap/Bumped/Textured Add"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[Toggle(NORMAL_MAP)] _EnableNormalMap ("Enable Normal Map", Int) = 0
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		[Toggle(MATCAP_ACCURATE)] _MatCapAccurate ("Accurate Calculation", Int) = 0
	}
	
	Subshader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma shader_feature MATCAP_ACCURATE
				#pragma shader_feature NORMAL_MAP
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					#if NORMAL_MAP
						float2 uv_bump : TEXCOORD1;
					#else
						float3 objectNormal : TEXCOORD1;
					#endif
					#if MATCAP_ACCURATE
						fixed3 tSpace0 : TEXCOORD2;
						fixed3 tSpace1 : TEXCOORD3;
						fixed3 tSpace2 : TEXCOORD4;
					#else
						float3 c0 : TEXCOORD2;
						float3 c1 : TEXCOORD3;
					#endif
				};
				
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				
				#if NORMAL_MAP
					uniform sampler2D _BumpMap;
					uniform float4 _BumpMap_ST;
				#endif
				uniform sampler2D _MatCap;

				v2f vert (appdata_tan v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					#if MATCAP_ACCURATE
						//Accurate bump calculation: calculate tangent space matrix and pass it to fragment shader
						fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
						fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
						fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
						o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
						o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
						o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
					#else
						//Faster but less accurate method (especially on non-uniform scaling)
						v.normal = normalize(v.normal);
						v.tangent = normalize(v.tangent);
						TANGENT_SPACE_ROTATION;
						o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
						o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));
					#endif			

					#if NORMAL_MAP
						o.uv_bump = TRANSFORM_TEX(v.texcoord, _BumpMap);
					#else
						o.objectNormal = v.normal;
					#endif

					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 tex = tex2D(_MainTex, i.uv);
					float3 normals;
					#if NORMAL_MAP
						normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));
					#else
						normals = i.objectNormal;
					#endif
					#if MATCAP_ACCURATE
						//Rotate normals from tangent space to world space
						float3 worldNorm;
						worldNorm.x = dot(i.tSpace0.xyz, normals);
						worldNorm.y = dot(i.tSpace1.xyz, normals);
						worldNorm.z = dot(i.tSpace2.xyz, normals);
						worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
						float4 mc = tex2D(_MatCap, worldNorm.xy * 0.5 + 0.5);
					#else
						half2 capCoord = half2(dot(i.c0, normals), dot(i.c1, normals));
						float4 mc = tex2D(_MatCap, capCoord * 0.5 + 0.5);
					#endif
					
					return (tex + (mc * 2.0) - 1.0);
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}