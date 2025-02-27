Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 100)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 offset = _BlurSize / _ScreenParams.xy;
                fixed4 color = tex2D(_MainTex, uv);
                color += tex2D(_MainTex, uv + offset);
                color += tex2D(_MainTex, uv - offset);
                color += tex2D(_MainTex, uv + float2(offset.x, -offset.y));
                color += tex2D(_MainTex, uv + float2(-offset.x, offset.y));
                return color / 2.5;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}