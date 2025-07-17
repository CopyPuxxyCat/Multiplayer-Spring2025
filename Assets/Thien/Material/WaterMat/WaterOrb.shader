Shader "Custom/WaterSlowLake_SlimeBounceX"
{
    Properties
    {
        _RippleSpeed("Ripple Speed", Vector)      = (0.5, 0.5, 0, 0)
        _VoronoiSpeed("Voronoi Speed", Float)     = 0.5
        _RippleAmount("Ripple Amount", Float)     = 1.0
        _VoronoiScale("Voronoi Scale", Float)     = 2.0
        [HDR]_RippleColor("Ripple Color", Color)  = (0.2, 0.6, 1, 1)
        _BottomStrength("Bottom Strength", Float) = 1.0
        _VerticalOffset("Vertical Offset", Float) = 0.0

        _BounceSpeed("Bounce Speed", Float)       = 2.0
        _BounceAmount("Bounce Amount", Float)     = 0.1
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

            float2 _RippleSpeed;
            float  _VoronoiSpeed;
            float  _RippleAmount;
            float  _VoronoiScale;
            float4 _RippleColor;
            float  _BottomStrength;
            float  _VerticalOffset;
            float  _BounceSpeed;
            float  _BounceAmount;

            float rand(float2 n) { return frac(sin(dot(n, float2(12.9898,78.233))) * 43758.5453); }
            float voronoi(in float2 x)
            {
                float2 n = floor(x), f = frac(x);
                float md = 1.0;
                for(int j=-1;j<=1;j++)for(int i=-1;i<=1;i++){
                    float2 g=i,j;
                    float2 o=rand(n+g)*0.8;
                    float2 r=g+o-f;
                    md=min(md,dot(r,r));
                }
                return sqrt(md);
            }

            struct appData { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float2 uv:    TEXCOORD0; float4 pos:SV_POSITION; };

            v2f vert(appData v)
            {
                v2f o;
                // radial wave
                float2 centeredUV = v.uv - 0.5;
                float dist = length(centeredUV);
                float t = _Time.y * _BounceSpeed;
                float wave = sin(t - dist * 10.0) * exp(-dist * 2.0);
                float bounceX = wave * _BounceAmount;

                // **X‑axis displacement**
                float3 displaced = v.vertex.xyz + float3(bounceX, 0, 0);
                o.pos = UnityObjectToClipPos(float4(displaced,1));

                o.uv = v.uv + float2(0, _VerticalOffset);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float t = _Time.y;
                float2 ro = _RippleSpeed * t;
                float vo = _VoronoiSpeed * t;
                float2 uvv = IN.uv * _VoronoiScale + ro;
                float v = voronoi(uvv + vo);
                v = pow(v, 0.2) * _RippleAmount;
                float3 col = _RippleColor.rgb * v;
                float  a   = _RippleColor.a * v;

                float b = pow(1 - IN.uv.y,1.0) * _BottomStrength;
                col = lerp(col, float3(0,0,0), saturate(b));

                return float4(col, a);
            }
            ENDCG
        }
    }
}
