Shader "Line Art/Lines Post Processing"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        //[HideInInspector] _IndexTex ("Surface Index Texture", 2D) = "black" {}
        _Scale ("Scale", Float) = 0.0010
        _DepthThreshold ("Depth Threshold", float) = 0.001
        _NormalThresholdAngle ("Normal Threshold Angle", float) = 30
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
            Name "Image Space Lines"

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

            float3 VisualizeNormals(float3 n)
            {
                return n * 0.5 + float3(0.5, 0.5, 0.5);
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _Scale;
            float _DepthThreshold;
            float _NormalThresholdAngle;

            float3 SampleWorldSpacePosition(float2 uv)
            {
                float depth = SampleSceneDepth(uv);
                return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
            }

            float3 SampleWorldSpaceNormal(float2 uv)
            {
                return normalize(mul(unity_CameraToWorld, float4(SampleSceneNormals(uv), 0.0)).xyz);
            }

            float DistanceToPlane(float3 from, float3 planeOrigin, float3 planeNormal)
            {
                return dot(from - planeOrigin, planeNormal);
            }

            float4 frag(VertexOut v) : SV_Target
            {
                #define FILTER_SIZE 6

                float3 sobelFilterX[FILTER_SIZE] =
                {
                    float3(-1, -1, -1),
                    float3(-1, 0, -2),
                    float3(-1, 1, -1),

                    float3(1, -1, 1),
                    float3(1, 0, 2),
                    float3(1, 1, 1)
                };

                float3 sobelFilterY[FILTER_SIZE] =
                {
                    float3(-1, -1, -1),
                    float3(0, -1, -2),
                    float3(1, -1, -1),

                    float3(-1, 1, 1),
                    float3(0, 1, 2),
                    float3(1, 1, 1)
                };

                float3 col = float3(0, 0, 0);

                float2 pixelSize = _MainTex_TexelSize.xy;

                float3 planePos = SampleWorldSpacePosition(v.uv);
                float3 planeNormal = SampleWorldSpaceNormal(v.uv);

                float depthEdgeX = 0;
                float depthEdgeY = 0;

                float normalEdgeX = 0;
                float normalEdgeY = 0;
                
                for (int i = 0; i < FILTER_SIZE; i++)
                {
                    const float2 coordsX = v.uv + sobelFilterX[i].xy * pixelSize;
                    const float2 coordsY = v.uv + sobelFilterY[i].xy * pixelSize;
                    const float weightX = sobelFilterX[i].z;
                    const float weightY = sobelFilterY[i].z;

                    float3 posX = SampleWorldSpacePosition(coordsX);
                    float3 posY = SampleWorldSpacePosition(coordsY);

                    // depthEdgeX += SampleSceneDepth(coordsX) * weightX;
                    // depthEdgeY += SampleSceneDepth(coordsY) * weightY;
                    depthEdgeX += DistanceToPlane(posX, planePos, planeNormal) * weightX;
                    depthEdgeY += DistanceToPlane(posY, planePos, planeNormal) * weightY;

                    normalEdgeX += dot(planeNormal, SampleWorldSpaceNormal(coordsX)) * weightX;
                    normalEdgeY += dot(planeNormal, SampleWorldSpaceNormal(coordsY)) * weightY;
                }

                float depthEdge = sqrt(depthEdgeX * depthEdgeX + depthEdgeY * depthEdgeY) > _DepthThreshold;
                float normalThreshold = cos(DegToRad(_NormalThresholdAngle));
                float normalEdge = sqrt(normalEdgeX * normalEdgeX + normalEdgeY * normalEdgeY) > normalThreshold;

                float edge = max(depthEdge, normalEdge);

                col = 1.0 - edge;

                // col = VisualizeNormals(planeNormal);
                // col = SampleSceneDepth(v.uv);

                return float4(col.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}