Shader "Custom/SpinEye"
{
    Properties
    {
        _MainTex    ("Mask Texture", 2D)     = "white" {}
        _Spin       ("Spin Speed",   Vector) = (0.5,0.5,0,0)
        _Tiling     ("UV Tiling",     Vector) = (1,1,0,0)
        [HDR]_FrontColor ("Front Color",   Color)  = (1,1,0,0)
        _BackColor  ("Back Color",    Color)  = (0,0,0,1)
        _Power      ("Contrast Pow",  Float)  = 2
        _Clip       ("Alpha Clip Th.", Float)  = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4   _MainTex_ST;
            float4   _Spin;
            float4   _Tiling;
            float4   _FrontColor;
            float4   _BackColor;
            float    _Power;
            float    _Clip;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // compute animated UV
                float2 uv = v.uv * _Tiling.xy
                          + _Spin.xy * _Time.y;
                o.uv = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // sample mask
                float m = tex2D(_MainTex, i.uv).r;

                // boost contrast
                m = pow(saturate(m), _Power);

                // color mix
                fixed4 col = lerp(_BackColor, _FrontColor, m);

                // alpha clip
                clip(m - _Clip);

                return col;
            }
            ENDHLSL
        }
    }
}
