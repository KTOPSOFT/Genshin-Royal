Shader "MFPS/Sights/ScopePro" {
    Properties{
        _ReticleTexure("Reticle", 2D) = "white" {}
        _ReticleTint("Reticle Tint", Color) = (1,1,1,1)
        _ReticleDepth("Reticle Depth", Range(0, 100)) = 50
        _RenderTexture("Render Texture", 2D) = "white" {}
        _VignetteRadius("Vignette Radius", Range(0, 2)) = 0.25
        _VignetteSmoothness("Vignette Smoothness", Range(0, 2)) = 0.25
    }
        SubShader{
              Tags { "RenderType" = "Opaque" "Queue" = "Transparent" "ForceNoShadowCasting" = "True" }
              LOD 200
              Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float3 tangent : TANGENT;
                };

                sampler2D _RenderTexture;
                float4 _RenderTexture_ST;

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 pos : TEXCOORD1;
                    float3 normal : NORMAL;
                    float3 tangent : TANGENT;
                    float4 worldPos : TEXCOORD2;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _RenderTexture);
                    o.pos = UnityObjectToViewPos(v.vertex);
                    o.normal = mul(UNITY_MATRIX_IT_MV, v.normal);
                    o.tangent = mul(UNITY_MATRIX_IT_MV, v.tangent);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    return o;
                }

                fixed _VignetteRadius, _VignetteSmoothness;

                fixed4 frag(v2f i) : SV_Target
                {
                    float3 normal = normalize(i.normal);
                    float3 tangent = normalize(i.tangent);
                    float3 cameraDir = normalize(i.pos);

                    float3 offset = normal;

                    float3x3 mat = float3x3(
                        tangent,
                        cross(normal, tangent),
                        normal
                        );


                    offset = mul(mat, offset);
                    float camDist = distance(i.worldPos, _WorldSpaceCameraPos) * 10;
                    offset *= camDist;


                    float2 uv = (i.uv + offset.xy);
                    fixed4 col = tex2D(_RenderTexture, uv + float2(0.5, 0.5));

                    fixed2 position = (uv);
                    fixed len = length(position) * 2;
                    _VignetteRadius /= camDist;
                    _VignetteSmoothness /= camDist;

                    col.rgb *= lerp(1, 0, smoothstep(_VignetteRadius, _VignetteRadius + _VignetteSmoothness, len));
                    return col;
                }

                  ENDCG
            }

            Pass{
                  CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include "UnityCG.cginc"

                    struct appdata {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                        float3 normal : NORMAL;
                        float3 tangent : TANGENT;
                    };

                    sampler2D _ReticleTexure;
                    float4 _ReticleTexure_ST;
                    fixed4 _ReticleTint;
                    float _ReticleDepth;

                    struct v2f {
                        float2 uv : TEXCOORD0;
                        float4 vertex : SV_POSITION;
                    };

                    v2f vert(appdata v) {

                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.uv = TRANSFORM_TEX(v.uv, _ReticleTexure);                     
                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target{
                          float2 uv = (i.uv - 0.5) * _ReticleDepth + 0.5;
                        return tex2D(_ReticleTexure, uv) * _ReticleTint;
                    }
                     ENDCG
              }

        }

        CustomEditor "ScopeProShaderEditor"
}