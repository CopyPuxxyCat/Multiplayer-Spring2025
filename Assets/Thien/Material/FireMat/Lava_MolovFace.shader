Shader "Custom/LavaMolovFace_BuiltIn"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _CloudDirection("Cloud Direction", Vector) = (1,1,0,0)
        _CloudTiling("Cloud Tiling", Vector) = (1,1,0,0)
        _Power("Power", Float) = 1
        [HDR]_InfaceColor("Inface Color", Color) = (1, 0.5, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float2 _CloudDirection;
            float2 _CloudTiling;
            float _Power;
            float4 _InfaceColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // _Time.y is time in seconds
                float2 scrollUV = i.uv * _CloudTiling + (_CloudDirection * _Time.y);

                float noise = tex2D(_NoiseTex, scrollUV).r;
                noise = pow(noise, _Power);

                float3 color = noise * _InfaceColor.rgb;

                return float4(color, 1);
            }
            ENDCG
        }
    }
}
