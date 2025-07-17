Shader "Custom/TrailOrb_HLSL_HDR"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _Color ("Color Tint (HDR)", Color) = (1,1,1,1)
        _Glow ("Glow Multiplier", Float) = 1
        _Speed ("UV Scroll Speed", Vector) = (1, 0, 0, 0)
        _Power ("Brightness Power", Float) = 1
        [NoScaleOffset] _TilingOffset ("Tiling Offset", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _Glow;
            float2 _Speed;
            float _Power;
            float4 _TilingOffset;

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

                float2 tiledUV = v.uv * _TilingOffset.xy + _TilingOffset.zw;

                // Scroll up (invert Y)
                tiledUV += float2(_Speed.x, -_Speed.y) * _Time.y;

                o.uv = tiledUV;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Enhance glow using alpha
                tex.rgb *= pow(tex.a, _Power);

                // Multiply HDR color + optional glow boost
                tex.rgb *= _Color.rgb * _Glow;
                tex.a *= _Color.a;

                return tex;
            }
            ENDCG
        }
    }
}
