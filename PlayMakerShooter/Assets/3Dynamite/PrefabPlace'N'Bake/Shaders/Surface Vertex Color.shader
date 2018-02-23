// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "3Dynamite/Surface Vertex Color"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalStrenght("Normal Strenght", Range( 0 , 1)) = 1
		_Metallic("Metallic", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_AmbientOcclusion("Ambient Occlusion", 2D) = "white" {}
		_AmbientIntensity("Ambient Intensity", Range( 0 , 1)) = 0.5
		_DetailMapsTiling("Detail Maps Tiling", Float) = 5
		_AlbedoDetail("Albedo Detail", 2D) = "white" {}
		_AlbedoDetailAmount("Albedo Detail Amount", Range( 0 , 1)) = 0.5
		_NormalMapDetail("Normal Map Detail", 2D) = "bump" {}
		_NMDetailStrenght("NM Detail Strenght", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _NormalMapDetail;
		uniform float _DetailMapsTiling;
		uniform float _NMDetailStrenght;
		uniform sampler2D _NormalMap;
		uniform float _NormalStrenght;
		uniform sampler2D _AlbedoDetail;
		uniform float _AlbedoDetailAmount;
		uniform sampler2D _Albedo;
		uniform sampler2D _Metallic;
		uniform float _Smoothness;
		uniform sampler2D _AmbientOcclusion;
		uniform float _AmbientIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _Color0 = float4(0.5019608,0.5019608,1,0);
			float2 temp_cast_0 = (_DetailMapsTiling).xx;
			float2 uv_TexCoord29 = i.uv_texcoord * temp_cast_0 + float2( 0,0 );
			float4 lerpResult32 = lerp( _Color0 , float4( UnpackNormal( tex2D( _NormalMapDetail, uv_TexCoord29 ) ) , 0.0 ) , _NMDetailStrenght);
			float2 uv_TexCoord9 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float4 lerpResult18 = lerp( _Color0 , float4( UnpackNormal( tex2D( _NormalMap, uv_TexCoord9 ) ) , 0.0 ) , _NormalStrenght);
			o.Normal = BlendNormals( lerpResult32.rgb , lerpResult18.rgb );
			float4 temp_cast_5 = (1.0).xxxx;
			float2 temp_cast_6 = (_DetailMapsTiling).xx;
			float2 uv_TexCoord24 = i.uv_texcoord * temp_cast_6 + float2( 0,0 );
			float4 lerpResult37 = lerp( temp_cast_5 , tex2D( _AlbedoDetail, uv_TexCoord24 ) , _AlbedoDetailAmount);
			float2 uv_TexCoord4 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			o.Albedo = ( lerpResult37 * tex2D( _Albedo, uv_TexCoord4 ) * i.vertexColor ).rgb;
			float2 uv_TexCoord11 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			o.Metallic = UnpackNormal( tex2D( _Metallic, uv_TexCoord11 ) ).x;
			o.Smoothness = _Smoothness;
			float2 uv_TexCoord14 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float4 lerpResult22 = lerp( float4(1,1,1,0) , float4( UnpackNormal( tex2D( _AmbientOcclusion, uv_TexCoord14 ) ) , 0.0 ) , _AmbientIntensity);
			o.Occlusion = lerpResult22.r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}