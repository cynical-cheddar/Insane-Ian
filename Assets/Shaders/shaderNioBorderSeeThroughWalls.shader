// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Shader No Border"
{
    Properties
    {
        _Color("Color", Color) = (0.5, 0.65, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"
               "LightMode" = "ForwardBase"
               "PassFlags" = "OnlyDirectional"
               "Queue" = "Transparent+1"}
        LOD 100

        Pass
        {
            ZWrite On
            ZTest Greater
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                
                //SHADOW_COORDS(2)
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(2, 3)

            };


            float4 _Color;


            v2f vert (appdata v)
            {
                
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

    }

}
