Shader "Custom/FlameBowShader"
{
    Properties
    {
        [HDR]_Color           ("Flame Color",         Color)  = (1,0.5,0,1)
        _Scale           ("Noise Scale",         Float)  = 1.0
        _Power           ("Flame Power",         Float)  = 2.0
        _CellSpeed       ("Cell Speed",          Float)  = 1.0
        _WallBrightness  ("Wall Brightness",     Float)  = 1.0
        _Tiling          ("UV Tiling",           Vector) = (3,3,0,0)
        _ScrollSpeed     ("Scroll Speed",        Vector) = (0,1,0,0)
        _Tails           ("Tails Exponent",      Float)  = 3.0
        _WallPower       ("Wall Power",          Float)  = 5.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // both‑sided flame
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;   // not used, but kept for compatibility
            float4   _Color;
            float    _Scale;
            float    _Power;
            float    _CellSpeed;
            float    _WallBrightness;
            float4   _Tiling;
            float4   _ScrollSpeed;
            float    _Tails;
            float    _WallPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
            };

            // 1D hash → [0,1]
            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
            }

            // 2D Voronoi distance (nearest‐point)
            float voronoi(in float2 uv)
            {
                float2 g = floor(uv);
                float2 f = frac(uv);
                float md = 1.0;

                // sample neighbouring cells
                for (int y=-1; y<=1; y++)
                for (int x=-1; x<=1; x++)
                {
                    float2 cell = float2(x,y);
                    float h   = hash21(g + cell);
                    float2 cp = cell + float2(h,h);
                    float d   = length(f - cp);
                    md        = min(md, d);
                }
                return md;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // animate & scale UVs
                float2 uv = i.uv * _Tiling.xy;
                uv += _Time.y * _ScrollSpeed.xy;

                // main Voronoi field
                float d = voronoi(uv * _Scale);
                float flameRaw = 1.0 - saturate(d);

                // power for core flame
                float flameCore = pow(flameRaw, _Power);

                // tails (outer wisps)
                float flameTail = pow(flameRaw, _Tails);

                // walls (cracks between cells)
                float walls = pow(d, _WallPower) * _WallBrightness;

                // combine
                float4 col = _Color * (flameCore + flameTail + walls);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
