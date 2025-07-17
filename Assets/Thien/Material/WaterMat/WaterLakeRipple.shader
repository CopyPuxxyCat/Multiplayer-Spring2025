Shader "Custom/WaterSlowLake_AlphaClip"
{
    Properties
    {
        _RippleSpeed("Ripple Speed", Vector)      = (0.5,  0.5,  0, 0)
        _VoronoiSpeed("Voronoi Speed", Float)     = 0.5
        _RippleAmount("Ripple Amount", Float)     = 1.0
        _VoronoiScale("Voronoi Scale", Float)     = 2.0
        [HDR]_RippleColor("Ripple Color", Color)       = (0.2, 0.6, 1, 1)
        _BottomStrength("Bottom Strength", Float) = 1.0
        _VerticalOffset("Vertical Offset", Float) = 0.0
        _Cutoff("Alpha Clip Threshold", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100
        Cull Off
        ZWrite Off

        Pass
        {
            // no blending, purely cutout
            //Blend Off
            // alternatively if you want blended edges, uncomment:
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Uniforms
            float2 _RippleSpeed;
            float  _VoronoiSpeed;
            float  _RippleAmount;
            float  _VoronoiScale;
            float4 _RippleColor;
            float  _BottomStrength;
            float  _VerticalOffset;
            float  _Cutoff;

            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898,78.233))) * 43758.5453);
            }
            float noise(float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n00 = rand(p + float2(0,0));
                float n01 = rand(p + float2(0,1));
                float n10 = rand(p + float2(1,0));
                float n11 = rand(p + float2(1,1));
                float nx0 = lerp(n00, n10, f.x);
                float nx1 = lerp(n01, n11, f.x);
                return lerp(nx0, nx1, f.y);
            }
            float voronoi(in float2 x)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                float md = 1.0;
                for(int j=-1; j<=1; j++)
                for(int i=-1; i<=1; i++)
                {
                    float2 g = float2(i, j);
                    float2 o = rand(n + g) * 0.8;
                    float2 r = g + o - f;
                    md = min(md, dot(r,r));
                }
                return sqrt(md);
            }

            struct appData { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert(appData v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv + float2(0, _VerticalOffset);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t = _Time.y;
                float2 rippleOffset = _RippleSpeed * t;
                float  voroOffset   = _VoronoiSpeed * t;

                // Voronoi-based ripple
                float2 uvVor = IN.uv * _VoronoiScale + rippleOffset;
                float v = voronoi(uvVor + voroOffset);
                v = pow(v, 0.2);
                v = saturate((v - 0.0) / (1.0 - 0.0));
                v *= _RippleAmount;

                float3 rippleCol = _RippleColor.rgb * v;
                float  rippleA   = _RippleColor.a * v;

                // Bottom fade
                float bottom = 1.0 - IN.uv.y;
                bottom = pow(bottom, 1.0);
                bottom = saturate((bottom - 0.0) / (1.0 - 0.0));
                bottom *= _BottomStrength;

                // Composite
                float3 col   = lerp(rippleCol, float3(0,0,0), bottom);
                float  alpha = rippleA;

                // Alpha‐clip here:
                clip(alpha - _Cutoff);

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}
