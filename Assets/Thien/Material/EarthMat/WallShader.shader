Shader "Custom/WallTextureWithTex"
{
    Properties
    {
        // --- Main Albedo + Tint ---
        _MainTex      ("Albedo (RGB) + Occlusion (A)", 2D)    = "white" {}
        _WallColor    ("Wall Tint Color",               Color) = (1,1,1,1)

        // --- Normal Map ---
        _NormalTex    ("Normal Map",                    2D)    = "bump" {}

        // --- Glow Mask ---
        _GlowTex      ("Glow Mask (R)",                 2D)    = "white" {}
        _GlowColor    ("Glow Color",                    Color) = (1,1,0,1)
        _GlowPower    ("Glow Intensity Power",          Float) = 2.0

        // --- Trail Mask (now rotating) ---
        _TrailTex     ("Trail Mask (R)",                2D)    = "white" {}
        _TrailTiling  ("Trail UV Tiling (X,Y)",         Vector)= (1,1,0,0)
        _TrailSpeed   ("Trail Spin Speed (radians/s)",  Float) = 1.0
        _TrailPower   ("Trail Contrast Power",          Float) = 2.0
        _TrailColor   ("Trail Color",                   Color) = (0,1,1,1)
        _TrailDistort ("Trail Distortion Amount",       Float) = 0.2

        // --- Fresnel ---
        _FresnelColor ("Fresnel Color",                 Color) = (1,1,1,1)
        _FresnelPower ("Fresnel Power",                 Float) = 2.0
        _FresnelStr   ("Fresnel Strength",              Float) = 0.5

        // --- Crack Overlay (optional) ---
        _CrackTex     ("Crack Mask (R)",                2D)    = "white" {}
        _CrackColor   ("Crack Color",                   Color) = (1,1,0,1)
        _CrackAmount  ("Crack Blend Amount",            Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Samplers + transforms
            sampler2D _MainTex;      float4 _MainTex_ST;
            sampler2D _NormalTex;    float4 _NormalTex_ST;
            sampler2D _GlowTex;      float4 _GlowTex_ST;
            sampler2D _TrailTex;     float4 _TrailTex_ST;
            sampler2D _CrackTex;     float4 _CrackTex_ST;

            // Wall tint
            float4 _WallColor;

            // Fresnel
            float4 _FresnelColor;
            float  _FresnelPower;
            float  _FresnelStr;

            // Glow
            float4 _GlowColor;
            float  _GlowPower;

            // Trail
            float2 _TrailTiling;
            float  _TrailSpeed;
            float  _TrailPower;
            float4 _TrailColor;
            float  _TrailDistort;

            // Crack
            float4 _CrackColor;
            float  _CrackAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float3 worldN : TEXCOORD0;
                float3 worldP : TEXCOORD1;
                float2 uvMain : TEXCOORD2;
                float2 uvNorm : TEXCOORD3;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos    = UnityObjectToClipPos(v.vertex);
                o.worldP = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldN = UnityObjectToWorldNormal(v.normal);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvNorm = TRANSFORM_TEX(v.uv, _NormalTex);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // normalize normals & view
                float3 N = normalize(IN.worldN);
                float3 V = normalize(_WorldSpaceCameraPos - IN.worldP);

                // 1) Albedo + Occlusion + Tint
                float4 ao = tex2D(_MainTex, IN.uvMain);
                float3 albedo = ao.rgb * _WallColor.rgb;
                float occ     = ao.a;

                // 2) Normal perturb
                float3 nt = UnpackNormal(tex2D(_NormalTex, IN.uvNorm));
                N = normalize(N + nt * 0.5);

                // 3) Fresnel
                float fres = pow(1 - saturate(dot(N, V)), _FresnelPower) * _FresnelStr;
                float3 fresCol = _FresnelColor.rgb * fres;

                // 4) Glow
                float gmask = tex2D(_GlowTex, IN.uvMain).r;
                gmask = pow(gmask, _GlowPower);
                float3 glowCol = _GlowColor.rgb * gmask;

                // 5) Rotating Trail
                // -- rotate UV about (0.5,0.5)
                float2 uvT = IN.uvMain;
                float angle = _Time.y * _TrailSpeed;
                float c = cos(angle), s = sin(angle);
                float2 center = float2(0.5, 0.5);
                float2 d = uvT - center;
                uvT = float2(d.x*c - d.y*s, d.x*s + d.y*c) + center;
                uvT = frac(uvT * _TrailTiling);

                // -- optional distortion from trail tex
                float2 dmask = tex2D(_TrailTex, uvT).rg * 2 - 1;
                uvT += dmask * _TrailDistort * _Time.y;

                float tmask = tex2D(_TrailTex, uvT).r;
                tmask = pow(saturate(tmask), _TrailPower);
                float3 trailCol = _TrailColor.rgb * tmask;

                // 6) Crack
                float cmask = tex2D(_CrackTex, IN.uvMain).r * _CrackAmount;
                float3 crackCol = _CrackColor.rgb * cmask;

                // Composite
                float3 col = albedo * occ;
                col += fresCol;
                col += glowCol;
                col = lerp(col, trailCol, tmask);
                col = lerp(col, crackCol, cmask);

                return float4(col, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
