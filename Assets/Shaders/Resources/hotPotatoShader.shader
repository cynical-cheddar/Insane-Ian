// Shader to highlight car with Gubbinz through wall

Shader "Unlit/Hot Potato Shader"
{
    Properties
    {
        _Color("Color", Color) = (0.5, 0.65, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        _Glossiness("Glossiness", Float) = 32
        _Metallic("Metallic", Range(0,1)) = 0.0

        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        _NumberOfSections("Number Of Sections", Int) = 5

        _SeeThroughColour("See Through Colour",Color) = (1, 0, 0, 1)
    }


        CGINCLUDE
#include "UnityCG.cginc"
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#pragma target 3.0

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
            struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2f {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float3 worldNormal : NORMAL;
            float3 viewDir : TEXCOORD1;
            float4 pos : SV_POSITION;
            float4 color : COLOR;
            LIGHTING_COORDS(2, 3)
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        float4 _Color;
        float4 _AmbientColor;

        float _Glossiness;
        float4 _SpecularColor;
        float4 _SeeThroughColour;

        float4 _RimColor;
        float _RimAmount;
        float _RimThreshold;

        int _IsSeeThrough;
        int _NumberOfSections;

        v2f vert(appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            o.viewDir = WorldSpaceViewDir(v.vertex);
            o.color = _SeeThroughColour;
            TRANSFER_SHADOW(o)
            return o;
        }
        ENDCG

        SubShader
        {
            //FIRST PASS: will render the see through colour through all other objects
            LOD 100
            Pass {
                Name "OUTLINE"
                Tags { "LightMode" = "Always" }
                Cull Off
                ZWrite Off
                ZTest Always
                Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        half4 frag(v2f i) :COLOR {
            return i.color;
        }
        ENDCG
        }
            //SECOND PASS: will render the normal "shaderNoBorder" shader
            Pass {
                Name "BASE"
                Tags { "RenderType" = "Opaque"
                               "LightMode" = "ForwardBase"
                               "PassFlags" = "OnlyDirectional"}
                ZWrite On
                ZTest LEqual
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #pragma multi_compile_fwdbase
                #pragma target 3.0

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    float3 normal = normalize(i.worldNormal);
                    float NdL = dot(_WorldSpaceLightPos0, normal);


                    float shadow = SHADOW_ATTENUATION(i);
                    float lightIntensity = 0;
                    float4 light = 0;

                    for (int j = -_NumberOfSections; j < _NumberOfSections; j++) {
                        lightIntensity = NdL > (1.0 / (j + 1)) ? 1 : 0;
                        light += (0.5 / _NumberOfSections) * (lightIntensity * _LightColor0);
                    }
                    float3 viewDir = normalize(i.viewDir);
                    float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
                    float NdH = dot(normal, halfVector);
                    float specularIntensity = pow(NdH * lightIntensity, _Glossiness * _Glossiness);

                    float specularIntensitySmooth = smoothstep(0.01, 0.01, specularIntensity);
                    float4 specular = specularIntensitySmooth * _SpecularColor;

                    float4 rimDot = 1 - dot(viewDir, normal);
                    float rimIntensity = rimDot * pow(NdL, _RimThreshold);
                    rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                    float4 rim = rimIntensity * _RimColor;

                    float attenuation = LIGHT_ATTENUATION(i);

                    return attenuation * _Color * col * (_AmbientColor + light + specular + rim);
                    }
            ENDCG
            }
    UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

        }

}
