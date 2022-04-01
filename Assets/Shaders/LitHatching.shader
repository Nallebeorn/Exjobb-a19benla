Shader "Line Art/Lit Hatching"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _UVScale ("UV Scale", Float) = 1
        _Cutoff ("Alpha Cutoff", Float) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Int) = 2 // Back
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _Cutoff;
        CBUFFER_END

        float rand(float2 seed)
        {
            return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
        }
        ENDHLSL

        Pass
        {
            Name "Color"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 pos : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 normal: NORMAL;
                float4 color: COLOR;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 color : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float4 clipPos : TEXCOORD4;
            };

            sampler2D _BaseMap;
            sampler2D _MaskTex;
            TEXTURE2D(_PaletteTex);
            SAMPLER(sampler_PaletteTex);
            float4 _ShadowColor;

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.pos = TransformObjectToHClip(v.pos.xyz);
                //float4 st = _PaletteTex_ST;
                //o.texcoord = (v.texcoord + st.zw - 0.5) * st.xy + 0.5;
                o.texcoord = v.texcoord;
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.color = v.color;
                const VertexPositionInputs positions = GetVertexPositionInputs(v.pos.xyz);
                o.shadowCoord = TransformWorldToShadowCoord(positions.positionWS);
                o.clipPos = positions.positionCS;

                return o;
            }

            float4 frag(Varyings v) : SV_Target
            {
                Light light = GetMainLight();

                float nDotL = saturate(dot(v.normal.xyz, light.direction));
                return nDotL;
                
                return float4(0.3, 0.7, 0.6, 1.0);
            }
            
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            // (Note, this doesn't support instancing for properties though. Same as URP/Lit)
            // #pragma multi_compile _ DOTS_INSTANCING_ON
            // (This was handled by LitInput.hlsl. I don't use DOTS so haven't bothered to support it)

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode"="DepthOnly"
            }

            ColorMask 0
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode"="DepthNormals"
            }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // Material keywords
            #pragma shader_feature_local _NORMALMAP
	        #pragma shader_feature_local_fragment _ALPHATEST_ON
	        #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            
            ENDHLSL
        }
    }
}