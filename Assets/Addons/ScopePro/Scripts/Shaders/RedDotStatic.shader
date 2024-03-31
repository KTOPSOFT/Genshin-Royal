Shader "MFPS/Sights/Red Dot Static" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_RedDotColor ("Emission Color", Color) = (1,1,1,1)
		_RedDotTex ("Red Dot Texture (A)", 2D) = "white" {}
		_RedDotSize ("Size", Range(0,10)) = 0.0
		[Toggle(FIXED_SIZE)] _FixedSize ("Use Fixed Size", Float) = 0
		_RedDotDist ("Distance Offset", Range(0,50)) = 2.0
		_OffsetX ("Horizontal Offset", Float) = 0.0
		_OffsetY ("Vertical Offset", Float) = 0.0
	}
 
	SubShader {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
       
       
        Pass {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
				#pragma shader_feature FIXED_SIZE

                #include "UnityCG.cginc"
 
                struct appdata_t {
                     float4 vertex : POSITION;
                     float3 normal : NORMAL;
                     float3 tangent : TANGENT;
                };
 
                struct v2f {
                    float4 vertex : SV_POSITION;
                    float3 pos : TEXCOORD0;
					float2 RedDotUV : TEXCOORD1;
                    float3 normal : NORMAL;
                    float3 tangent : TANGENT;
                };
 
				fixed4 _Color;
				sampler2D _RedDotTex;
				fixed4 _RedDotColor;
				fixed _RedDotSize;
				fixed _RedDotDist;
				fixed _OffsetX;
				fixed _OffsetY;
                       
                v2f vert (appdata_t v) {
					v2f o = (v2f)0;
                    o.vertex = UnityObjectToClipPos(v.vertex);
					o.pos = UnityObjectToViewPos(v.vertex);

					o.normal = mul(UNITY_MATRIX_IT_MV, v.normal);   //transform normal into eye space
					o.tangent = mul(UNITY_MATRIX_IT_MV, v.tangent);

                    return o;
                }
                       
                fixed4 frag (v2f i): COLOR {

                float3 normal = normalize(i.normal);    //get normal of fragment
                float3 tangent = normalize(i.tangent);  //get tangent
                float3 cameraDir = normalize(i.pos);    //get direction from camera to fragment, normalize(i.pos - float3(0, 0, 0))

                float3 offset = cameraDir + normal;     //calculate offset from two points on unit sphere, cameraDir - -normal

                float3x3 mat = float3x3(
                    tangent,
                    cross(normal, tangent),
                    normal
                );

                offset = mul(mat, offset);  //transform offset into tangent space

                fixed4 col = _Color;
                offset.y += _OffsetY;
                float2 uv = offset.xy / _RedDotSize;              //sample and scale
                fixed redDot = tex2D(_RedDotTex, uv + float2(0.5, 0.5)).a;
                col.rgb += redDot * _RedDotColor.rgb * _RedDotColor.a;
                col.a += redDot * _RedDotColor.a;

                return col;  //shift sample to center of texture

                  /*  fixed4 col = _Color;
					fixed redDot = tex2D (_RedDotTex, i.RedDotUV).a;
					col.rgb += redDot * _RedDotColor.rgb * _RedDotColor.a;
					col.a += redDot * _RedDotColor.a;
                    return col;*/
                }
            ENDCG
		}
	}
 }