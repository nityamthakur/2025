// Code Originally from Max O'Didily on Youtube
// https://www.youtube.com/watch?v=LGQuLwpWjDM

Shader "Custom/GreyScaleShader" 
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass 
        {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));
                
                    // Prevent pure black (0,0,0) from becoming white
                    if (col.r == 0 && col.g == 0 && col.b == 0)
                    {
                        return col; // Keep it black
                    }
                
                    return fixed4(grey, grey, grey, col.a);
                }                
                ENDCG
        }        
    }
    FallBack "Diffuse"
}