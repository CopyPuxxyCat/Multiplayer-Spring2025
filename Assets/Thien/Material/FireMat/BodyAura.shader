Shader "Custom/BodyEnchantShader"
{
    Properties
    {
        _MainTex           ("Base Texture",          2D)    = "white" {}
        _VoronoiTiling     ("Voronoi Tiling",        Vector) = (5,5,0,0)
        _VoronoiSpeed      ("Voronoi Speed",         Float)  = 1.0
        _RippleDisso       ("Ripple Dissolve",       Float)  = 0.5
        _RippleScale       ("Ripple Scale",          Float)  = 1.0
        _RippleSpeed       ("Ripple Speed",          Float)  = 1.0
        _MainColor         ("Main Color",            Color)  = (1,0.5,0,1)
        _Color             ("Overlay Color",         Color)  = (1,1,1,1)
        _FresnelPower      ("Fresnel Power",         Float)  = 5.0
        _Clip              ("Alpha Clip Threshold",  Float)  = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;

            float4 _MainColor;
            float4 _Color;
            float4 _VoronoiTiling;
            float  _VoronoiSpeed;
            float  _RippleDisso;
            float  _RippleScale;
            float  _RippleSpeed;
            float  _FresnelPower;
            float  _Clip;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir     : TEXCOORD3;
            };

            // 1D hash → [0,1]
            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
            }

            // 2D Worley/Voronoi distance field
            float voronoi(float2 uv)
            {
                float2 g = floor(uv);
                float2 f = frac(uv);
                float minDist = 1.0;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 cell = float2(x, y);
                        float h   = hash21(g + cell);
                        float2 cp = cell + float2(h, h);              // <— renamed, explicit
                        float  d  = length(f - cp);                   // no overload error
                        minDist   = min(minDist, d);
                    }
                }
                return minDist;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos    = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir     = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fresnel falloff
                float fresnel = pow(1 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);

                // Animated Voronoi
                float2 tu   = i.uv * _VoronoiTiling.xy + _Time.y * _VoronoiSpeed;
                float vno   = voronoi(tu);
                float rpt   = pow(vno, _RippleDisso);

                // Combine ripple + fresnel for emission
                float emiss = saturate(rpt * _RippleScale + fresnel);
                float4 col  = lerp(_Color, _MainColor, emiss);

                col.a = emiss;
                clip(col.a - _Clip);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
