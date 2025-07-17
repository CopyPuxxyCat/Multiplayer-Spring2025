Shader "Custom/MarkShaderHLSL"
{
    Properties
    {
        _MainColor      ("Main Color",        Color)  = (1,1,1,1)
        _CellSpeed      ("Cell Speed",        Float)  = 1
        _ScrollSpeed    ("Scroll Speed",      Vector) = (1,1,0,0)
        _Scale          ("Scale",             Float)  = 1
        _Power          ("Power",             Float)  = 1
        _WallBrightness ("Wall Brightness",   Float)  = 1
        _WallPower      ("Wall Power",        Float)  = 1
        _Tiling         ("Tiling",            Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _MainColor;
            float  _CellSpeed;
            float  _Scale;
            float  _Power;
            float  _WallBrightness;
            float  _WallPower;
            float4 _ScrollSpeed;
            float4 _Tiling;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            static float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
            }

            static float voronoi(float2 uv)
            {
                float2 g = floor(uv);
                float2 f = frac(uv);
                float minDist = 1.0;

                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    float2 cell = float2(x, y);
                    float  h    = hash21(g + cell);
                    float2 cp   = cell + float2(h, h);
                    float  d    = length(f - cp);
                    minDist     = min(minDist, d);
                }
                return minDist;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // animate UV
                float2 uvOff = v.uv * _Tiling.xy
                             + (_ScrollSpeed.xy * _Time.y * _CellSpeed);
                o.uv = uvOff * _Scale;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // base Voronoi‑driven brightness
                float cell = voronoi(i.uv);
                cell = pow(cell, _Power);
                cell = clamp(cell, 0, 1);
                float brightness = cell * _WallBrightness;
                float alphaBase  = brightness * _WallPower;
                float3 rgbBase   = _MainColor.rgb * brightness;

                // fade out toward the top of the mesh
                float fade = saturate(1 - i.uv.y);
                rgbBase *= fade;
                alphaBase *= fade;

                return float4(rgbBase, alphaBase);
            }
            ENDHLSL
        }
    }
}
