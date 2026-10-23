// Lyoko's white, hazy sky (F9.3): a soft vertical gradient tinted by the sector.
Shader "CodeLyoko/LyokoSky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.88, 0.94, 0.97, 1)
        _HorizonColor ("Horizon Color", Color) = (0.97, 0.99, 1, 1)
        _BottomColor ("Bottom Color", Color) = (0.55, 0.78, 0.86, 1)
        _HorizonSharpness ("Horizon Sharpness", Range(0.5, 8)) = 2.5
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _HorizonColor;
                half4 _BottomColor;
                half _HorizonSharpness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float y = normalize(input.direction).y;
                half up = pow(saturate(y), 1.0h / _HorizonSharpness);
                half down = pow(saturate(-y), 1.0h / _HorizonSharpness);
                half3 color = lerp(_HorizonColor.rgb, _TopColor.rgb, up);
                color = lerp(color, _BottomColor.rgb, down);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
