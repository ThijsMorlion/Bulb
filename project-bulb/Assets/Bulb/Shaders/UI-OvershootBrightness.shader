// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,atwp:True,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33265,y:32743,varname:node_3138,prsc:2|emission-4375-OUT,alpha-3245-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:6620,x:32238,y:32900,ptovrint:False,ptlb:MainTexture,ptin:_MainTexture,varname:node_6620,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ce5da90d31c9bef41aed7dfc1699f3d1,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8022,x:32545,y:32850,varname:node_8022,prsc:2,tex:ce5da90d31c9bef41aed7dfc1699f3d1,ntxv:0,isnm:False|TEX-6620-TEX;n:type:ShaderForge.SFN_Slider,id:8379,x:32388,y:33039,ptovrint:False,ptlb:ColorMultiplier,ptin:_ColorMultiplier,varname:node_8379,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:2;n:type:ShaderForge.SFN_Multiply,id:4375,x:32785,y:32749,varname:node_4375,prsc:2|A-8022-RGB,B-8379-OUT;n:type:ShaderForge.SFN_Slider,id:9476,x:32388,y:33201,ptovrint:False,ptlb:AlphaMultiplier,ptin:_AlphaMultiplier,varname:node_9476,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:3245,x:32796,y:33113,varname:node_3245,prsc:2|A-8022-A,B-9476-OUT;proporder:6620-8379-9476;pass:END;sub:END;*/

Shader "Shader Forge/UI-OvershootBrightness" {
    Properties {
        _MainTexture ("MainTexture", 2D) = "white" {}
        _ColorMultiplier ("ColorMultiplier", Range(1, 2)) = 1
        _AlphaMultiplier ("AlphaMultiplier", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Stencil ("Stencil ID", Float) = 0
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilOpFail ("Stencil Fail Operation", Float) = 0
        _StencilOpZFail ("Stencil Z-Fail Operation", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            Stencil {
                Ref [_Stencil]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilOp]
                Fail [_StencilOpFail]
                ZFail [_StencilOpZFail]
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            uniform float _ColorMultiplier;
            uniform float _AlphaMultiplier;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_8022 = tex2D(_MainTexture,TRANSFORM_TEX(i.uv0, _MainTexture));
                float3 emissive = (node_8022.rgb*_ColorMultiplier);
                float3 finalColor = emissive;
                return fixed4(finalColor,(node_8022.a*_AlphaMultiplier));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
