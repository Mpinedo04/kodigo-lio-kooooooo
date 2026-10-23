// The Digital Sea (F9.2): slow waves, a grid of data lines and twinkling bits. Works on a subdivided plane.
Shader "CodeLyoko/DigitalSea"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.01, 0.16, 0.32, 1)
        _ShallowColor ("Shallow Color", Color) = (0.05, 0.62, 0.9, 1)
        [HDR] _LineColor ("Data Line Color", Color) = (0.55, 1.4, 1.6, 1)
        _GridScale ("Grid Scale", Float) = 0.28
        _LineWidth ("Line Width", Range(0.001, 0.2)) = 0.035
        _WaveHeight ("Wave Height", Float) = 0.35
        _WaveSpeed ("Wave Speed", Float) = 0.6
        _FlowSpeed ("Data Flow Speed", Float) = 0.35
        _Sparkle ("Sparkle Amount", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Sea"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _LineColor;
                float _GridScale;
                half _LineWidth;
                float _WaveHeight;
                float _WaveSpeed;
                float _FlowSpeed;
                half _Sparkle;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float wave : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            float Hash(float2 cell)
            {
                return frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float t = _Time.y * _WaveSpeed;
                float wave = sin(positionWS.x * 0.13 + t) * 0.6 + cos(positionWS.z * 0.11 + t * 1.3) * 0.4;
                positionWS.y += wave * _WaveHeight;
                output.positionWS = positionWS;
                output.wave = wave * 0.5 + 0.5;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.positionWS.xz * _GridScale;
                float2 flowUv = uv + float2(0.0, _Time.y * _FlowSpeed);
                float2 cell = abs(frac(flowUv) - 0.5);
                half lines = 1.0h - smoothstep(0.0h, _LineWidth, min(cell.x, cell.y));

                float2 id = floor(flowUv);
                float seed = Hash(id);
                half twinkle = step(0.93, seed) * saturate(sin(_Time.y * 3.0 + seed * 40.0)) * _Sparkle;

                half3 color = lerp(_DeepColor.rgb, _ShallowColor.rgb, input.wave);
                color += _LineColor.rgb * (lines * 0.45h + twinkle);
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
