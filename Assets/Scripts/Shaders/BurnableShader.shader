Shader "BurnableShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BurnTex ("Burn Texture", 2D) = "white" {}
        _BurnPatternTex ("Burn Pattern Texture", 2D) = "white" {}
        _BaseTex ("Base Texture", 2D) = "white" {}
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
            sampler2D _BurnTex;
            float4 _BurnTex_ST;
            sampler2D _BurnPatternTex;
            float4 _BurnPatternTex_ST;
            sampler2D _BaseTex;
            float4 _BaseTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                // sample the texture
                fixed burnLevel = tex2D(_BurnPatternTex, i.uv).r;
                if (burnLevel == 0) {
                    col = tex2D(_BaseTex, i.uv);
                }
                else {
                    col = tex2D(_MainTex, i.uv);
                    if (burnLevel < 1) {
                        fixed4 burnCol = tex2D(_BurnTex, i.uv);
                        col = burnCol * (1 - burnLevel) + col * burnLevel;
                    }
                }
                col.a = burnLevel;
                return col;
            }
            ENDCG
        }
    }
}
