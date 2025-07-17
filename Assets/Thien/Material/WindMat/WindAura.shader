Shader "Custom/VoronoiAura_HLSL"
{
    Properties
    {
        [HDR]_Color ("Aura Color", Color) = (1,1,1,1)
        _VoronoiSpeed ("Voronoi Scroll Speed", Vector) = (0.1, 0.1, 0, 0)
        _VoronoiTiling ("Voronoi Tiling", Vector) = (2, 2, 0, 0)
        _Clip ("Alpha Clip Threshold", Float) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float2 _VoronoiSpeed;
            float2 _VoronoiTiling;
            float _Clip;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 scroll = _VoronoiSpeed * _Time.y;
                o.uv = v.uv * _VoronoiTiling + scroll;

                return o;
            }

            // Simple fake Voronoi (actually using noise-like pattern)
            float random2(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = random2(i);
                float b = random2(i + float2(1, 0));
                float c = random2(i + float2(0, 1));
                float d = random2(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float n = noise(i.uv * 10); // 10x scale like in your graph

                // Alpha clip
                clip(n - _Clip);

                float3 col = _Color.rgb * n;
                return float4(col, _Color.a * n); // premultiplied alpha
            }
            ENDHLSL
        }
    }
}
