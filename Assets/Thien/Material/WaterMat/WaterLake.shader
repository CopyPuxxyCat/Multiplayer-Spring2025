Shader "Custom/WaterSlowLake"
{
    Properties
    {
        _RippleSpeed("Ripple Speed", Vector)      = (0.5,  0.5,  0, 0)
        _VoronoiSpeed("Voronoi Speed", Float)     = 0.5
        _RippleAmount("Ripple Amount", Float)     = 1.0
        _VoronoiScale("Voronoi Scale", Float)     = 2.0
        _RippleColor("Ripple Color", Color)       = (0.2, 0.6, 1, 1)
        _BottomStrength("Bottom Strength", Float) = 1.0
        _VerticalOffset("Vertical Offset", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Uniforms (from Properties)
            float2 _RippleSpeed;
            float  _VoronoiSpeed;
            float  _RippleAmount;
            float  _VoronoiScale;
            float4 _RippleColor;
            float  _BottomStrength;
            float  _VerticalOffset;

            // Simple random and noise functions
            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898,78.233))) * 43758.5453);
            }
            float noise(float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
                // Quintic interpolation
                f = f * f * (3.0 - 2.0 * f);

                float n00 = rand(p + float2(0.0,0.0));
                float n01 = rand(p + float2(0.0,1.0));
                float n10 = rand(p + float2(1.0,0.0));
                float n11 = rand(p + float2(1.0,1.0));

                float nx0 = lerp(n00, n10, f.x);
                float nx1 = lerp(n01, n11, f.x);
                return lerp(nx0, nx1, f.y);
            }

            // 2D Voronoi / cellular noise
            float voronoi(in float2 x)
            {
                float2 n = floor(x);
                float2 f = frac(x);

                float md = 1.0;
                for(int j=-1; j<=1; j++)
                for(int i=-1; i<=1; i++)
                {
                    float2 g = float2(i, j);
                    float2 o = rand(n + g) * float2(0.8,0.8);
                    float2 r = g + o - f;
                    float d = dot(r, r);
                    md = min(md, d);
                }
                return sqrt(md);
            }

            struct appData
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv      : TEXCOORD0;
                float4 vertex  : SV_POSITION;
            };

            v2f vert(appData v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // apply vertical offset in UV space if needed
                o.uv = v.uv + float2(0, _VerticalOffset);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // time-based offsets
                float t = _Time.y;
                float2 rippleOffset = _RippleSpeed * t;
                float  voroOffset   = _VoronoiSpeed * t;

                // -- RIPPLES VIA VORONOI --
                float2 uvVor = IN.uv * _VoronoiScale + rippleOffset;
                float  v = voronoi(uvVor + voroOffset);
                // Graph used a Power node and a Remap [0-1]
                v = pow(v, 0.2);
                v = saturate((v - 0.0) / (1.0 - 0.0));
                v *= _RippleAmount;

                float3 rippleCol = _RippleColor.rgb * v;
                float  rippleA   = _RippleColor.a * v;

                // -- BOTTOM FOG / DEPTH FADE --
                // Graph had a OneMinus on UV.y, then power/remap and multiplied by bottomStrength
                float bottom = 1.0 - IN.uv.y;
                bottom = pow(bottom, 1.0);
                bottom = saturate((bottom - 0.0) / (1.0 - 0.0));
                bottom *= _BottomStrength;

                // Final composite: lerp between ripple color and a darker color by bottom factor
                float3 col = lerp(rippleCol, float3(0,0,0), bottom);
                float  alpha = saturate(rippleA);

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}
