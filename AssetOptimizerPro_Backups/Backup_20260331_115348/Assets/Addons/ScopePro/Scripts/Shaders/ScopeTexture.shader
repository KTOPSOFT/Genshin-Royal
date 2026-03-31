// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MFPS/Sights/ScopeTexture"
{
	Properties
	{
		_Scope("Scope", 2D) = "white" {}
		_ColorMul("ColorMul", Color) = (1,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Scope;
		uniform float4 _Scope_ST;
		uniform float4 _ColorMul;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Scope = i.uv_texcoord * _Scope_ST.xy + _Scope_ST.zw;
			float4 tex2DNode3 = tex2D( _Scope, uv_Scope );
			float4 lerpResult14 = lerp( tex2DNode3 , ( tex2DNode3 * _ColorMul ) , tex2DNode3);
			o.Emission = lerpResult14.rgb;
			o.Alpha = tex2DNode3.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}