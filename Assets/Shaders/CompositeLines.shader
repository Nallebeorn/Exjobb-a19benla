Shader "Line Art/Composite Lines"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Composite Image Space Lines"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct VertexIn
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOut vert(VertexIn v)
            {
                VertexOut o;
                o.pos = TransformObjectToHClip(v.pos.xyz);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _LinesTex;
            float4 _OutlineColor;

            float4 frag(VertexOut v) : SV_Target
            {
                float3 col;
                float3 base = tex2D(_MainTex, v.uv).rgb;
                col = lerp(base, _OutlineColor.rgb, tex2D(_LinesTex, v.uv).r);
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}