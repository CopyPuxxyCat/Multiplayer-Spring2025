Shader "Custom/OrbCore_DoubleSided_Colored"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _FrontColor ("Front Color (HDR)", Color) = (1,1,1,1)
        [HDR] _BackColor ("Back Color (HDR)", Color) = (1,1,1,1)
        _Glow ("Glow Multiplier", Float) = 1
        _Speed ("UV Scroll Speed", Vector) = (0, 0, 0, 0)
        _Power ("Brightness Power", Float) = 1
        [NoScaleOffset] _TilingOffset ("Tiling Offset", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // Back Face Pass
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BackColor;
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
                tiledUV += float2(_Speed.x, -_Speed.y) * _Time.y;
                o.uv = tiledUV;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                tex.rgb *= pow(tex.a, _Power);
                tex.rgb *= _BackColor.rgb * _Glow;
                tex.a *= _BackColor.a;
                return tex;
            }
            ENDCG
        }

        // Front Face Pass
        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _FrontColor;
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
                tiledUV += float2(_Speed.x, -_Speed.y) * _Time.y;
                o.uv = tiledUV;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                tex.rgb *= pow(tex.a, _Power);
                tex.rgb *= _FrontColor.rgb * _Glow;
                tex.a *= _FrontColor.a;
                return tex;
            }
            ENDCG
        }
    }
}
