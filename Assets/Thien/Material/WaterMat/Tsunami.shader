Shader "Custom/SlimyBouncyTsunami"
{
    Properties
    {
        [HDR]_BaseColor  ("Base Water Color",      Color) = (0.1, 0.4, 0.8, 1)
        [HDR]_LineColor  ("Vein (Foam) Color",     Color) = (1, 1, 1, 1)
        _Speed1     ("Animation Speed 1",     Float) = 0.3
        _Speed2     ("Animation Speed 2",     Float) = 0.6
        _Scale1     ("Pattern Scale 1",       Float) = 5.0
        _Scale2     ("Pattern Scale 2",       Float) = 8.0
        _Threshold  ("Pattern Threshold",     Range(0,1)) = 0.4
        _WaveAmplitude ("Wave Amplitude",     Float) = 0.1
        _WaveFrequency ("Wave Frequency",     Float) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // properties
            float4 _BaseColor;
            float4 _LineColor;
            float  _Speed1;
            float  _Speed2;
            float  _Scale1;
            float  _Scale2;
            float  _Threshold;
            float  _WaveAmplitude;
            float  _WaveFrequency;

            struct appData
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 pos    : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appData v)
            {
                v2f o;
                // world space position for rim effect
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // bounce the vertices up and down for slimy wave
                float t = _Time.y;
                float wave = sin(v.vertex.x * _WaveFrequency + t * _Speed1) * _WaveAmplitude;
                float3 displaced = v.vertex.xyz;
                displaced.y += wave;
                o.pos = UnityObjectToClipPos(float4(displaced,1));
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // time
                float t = _Time.y;

                // two moving sine‐fields
                float2 uv1 = IN.uv * _Scale1 + float2(t * _Speed1, t * _Speed1 * 1.3);
                float f1  = sin(uv1.x + sin(uv1.y * 1.2));
                
                float2 uv2 = IN.uv * _Scale2 + float2(-t * _Speed2, t * _Speed2 * 0.8);
                float f2  = cos(uv2.y + cos(uv2.x * 1.5));

                // combine and threshold to foam mask
                float pattern = f1 * f2;
                float mask = smoothstep(_Threshold - 0.05, _Threshold + 0.05, abs(pattern));

                // bounce highlight for slimy look
                float bounce = abs(sin(pattern * 3.1415 * 2.0));
                // fresnel rim for extra gloss
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float rim = pow(1.0 - saturate(dot(viewDir, float3(0,1,0))), 2.0);

                // final color with slime shine
                float3 base = lerp(_BaseColor.rgb, _LineColor.rgb, mask);
                float3 slimeGlow = lerp(base, float3(1,1,1), bounce * rim);
                return float4(slimeGlow, 1);
            }
            ENDCG
        }
    }
}
