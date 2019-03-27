// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:1,cusa:True,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:True,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,atwp:True,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:1873,x:33297,y:32709,varname:node_1873,prsc:2|emission-5261-OUT,alpha-6033-OUT;n:type:ShaderForge.SFN_Multiply,id:1086,x:32697,y:32809,cmnt:RGB,varname:node_1086,prsc:2|A-8224-RGB,B-5983-RGB;n:type:ShaderForge.SFN_Color,id:5983,x:32388,y:32868,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:5412,x:32287,y:33058,varname:node_5412,prsc:2,tex:0a1116a10619f734fa3defc890efe0f7,ntxv:0,isnm:False|UVIN-5290-OUT,TEX-2863-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:2863,x:31999,y:32880,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_2863,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:0a1116a10619f734fa3defc890efe0f7,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8224,x:32255,y:32718,varname:node_8224,prsc:2,tex:0a1116a10619f734fa3defc890efe0f7,ntxv:0,isnm:False|UVIN-9673-UVOUT,TEX-2863-TEX;n:type:ShaderForge.SFN_TexCoord,id:9673,x:31958,y:32615,varname:node_9673,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:4455,x:31695,y:33088,varname:node_4455,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:5290,x:32053,y:33154,varname:node_5290,prsc:2|A-4455-UVOUT,B-2921-OUT;n:type:ShaderForge.SFN_Vector4Property,id:5868,x:31695,y:33304,ptovrint:False,ptlb:Offset,ptin:_Offset,varname:node_5868,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.1,v2:-0.1,v3:0,v4:0;n:type:ShaderForge.SFN_Append,id:2921,x:31862,y:33239,varname:node_2921,prsc:2|A-5868-X,B-5868-Y;n:type:ShaderForge.SFN_Color,id:7625,x:32287,y:33205,ptovrint:False,ptlb:ShadowColor,ptin:_ShadowColor,varname:node_7625,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0,c3:0,c4:0.5;n:type:ShaderForge.SFN_Lerp,id:4909,x:32743,y:33050,varname:node_4909,prsc:2|A-2042-OUT,B-9120-OUT,T-8224-A;n:type:ShaderForge.SFN_Vector1,id:9120,x:32506,y:33072,varname:node_9120,prsc:2,v1:0;n:type:ShaderForge.SFN_Add,id:6033,x:32984,y:33016,varname:node_6033,prsc:2|A-8224-A,B-4909-OUT;n:type:ShaderForge.SFN_Multiply,id:2042,x:32537,y:33186,varname:node_2042,prsc:2|A-5412-A,B-7625-A;n:type:ShaderForge.SFN_Lerp,id:5261,x:32911,y:32841,varname:node_5261,prsc:2|A-1086-OUT,B-7625-RGB,T-4909-OUT;proporder:5983-2863-5868-7625;pass:END;sub:END;*/

Shader "Shader Forge/UI-Dropshadow" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _Offset ("Offset", Vector) = (-0.1,-0.1,0,0)
        _ShadowColor ("ShadowColor", Color) = (1,0,0,0.5)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _Stencil ("Stencil ID", Float) = 0
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilOpFail ("Stencil Fail Operation", Float) = 0
        _StencilOpZFail ("Stencil Z-Fail Operation", Float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
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
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _Offset;
            uniform float4 _ShadowColor;
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
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_8224 = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float2 node_5290 = (i.uv0+float2(_Offset.r,_Offset.g));
                float4 node_5412 = tex2D(_MainTex,TRANSFORM_TEX(node_5290, _MainTex));
                float node_4909 = lerp((node_5412.a*_ShadowColor.a),0.0,node_8224.a);
                float3 emissive = lerp((node_8224.rgb*_Color.rgb),_ShadowColor.rgb,node_4909);
                float3 finalColor = emissive;
                return fixed4(finalColor,(node_8224.a+node_4909));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
