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
                float4 baseCol = tex2D(_MainTex, v.uv);

                #define FILTER_SIZE 4

                float3 filter[FILTER_SIZE] =
                {
                    float3(-1, -1, 1),
                    float3(1, -1, 1),
                    float3(-1, 1, 1),
                    float3(1, 1, 1),
                };

                // float3 sobelFilterX[FILTER_SIZE] =
                // {
                //     float3(-1, -1, -1),
                //     float3(-1, 0, -2),
                //     float3(-1, 1, -1),
                //
                //     float3(1, -1, 1),
                //     float3(1, 0, 2),
                //     float3(1, 1, 1)
                // };
                //
                // float3 sobelFilterY[FILTER_SIZE] =
                // {
                //     float3(-1, -1, -1),
                //     float3(0, -1, -2),
                //     float3(1, -1, -1),
                //
                //     float3(-1, 1, 1),
                //     float3(0, 1, 2),
                //     float3(1, 1, 1)
                // };

                float3 col = float3(0, 0, 0);

                float2 pixelSize = _MainTex_TexelSize.xy;

                float3 planePos = SampleWorldSpacePosition(v.uv);
                float3 planeNormal = SampleWorldSpaceNormal(v.uv);

                float depthEdge = 0;
                float normalEdge = 0;

                float normalThreshold = cos(DegToRad(_NormalThresholdAngle));
                
                for (int i = 0; i < FILTER_SIZE; i++)
                {
                    const float2 coords = v.uv + filter[i].xy * pixelSize;

                    float3 pos = SampleWorldSpacePosition(coords);

                    // depthEdgeX += SampleSceneDepth(coordsX) * weightX;
                    // depthEdgeY += SampleSceneDepth(coordsY) * weightY;
                    depthEdge += step(_DepthThreshold, DistanceToPlane(pos, planePos, planeNormal));

                    normalEdge += 1.0 - step(normalThreshold, dot(planeNormal, SampleWorldSpaceNormal(coords)));
                }

                float edge = max(saturate(depthEdge), saturate(normalEdge));

                // edge = saturate(depthEdge);
                
                col = lerp(baseCol, _OutlineColor, edge).rgb;

                // col = VisualizeNormals(planeNormal);
                // col = SampleSceneDepth(v.uv);

                return float4(col.rgb, 1.0);
            }
            
            ENDHLSL
        }
    }
}