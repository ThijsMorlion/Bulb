// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:True,tesm:0,olmd:0,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-8080-OUT;n:type:ShaderForge.SFN_Multiply,id:8080,x:32440,y:32772,varname:node_8080,prsc:2|A-9234-RGB,B-1034-OUT;n:type:ShaderForge.SFN_Slider,id:4567,x:31779,y:32808,ptovrint:False,ptlb:MaxGlow,ptin:_MaxGlow,varname:node_4567,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:2,max:2;n:type:ShaderForge.SFN_Time,id:6228,x:31317,y:33103,varname:node_6228,prsc:2;n:type:ShaderForge.SFN_Vector1,id:4648,x:31317,y:33017,varname:node_4648,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:4254,x:31555,y:33035,varname:node_4254,prsc:2|A-4648-OUT,B-6228-TTR;n:type:ShaderForge.SFN_Sin,id:5945,x:31738,y:33035,varname:node_5945,prsc:2|IN-4254-OUT;n:type:ShaderForge.SFN_Lerp,id:1034,x:32204,y:32921,varname:node_1034,prsc:2|A-4567-OUT,B-4629-OUT,T-7242-OUT;n:type:ShaderForge.SFN_Vector1,id:4629,x:31936,y:32917,varname:node_4629,prsc:2,v1:1;n:type:ShaderForge.SFN_Color,id:9234,x:31967,y:32561,ptovrint:False,ptlb:node_9234,ptin:_node_9234,varname:node_9234,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9117647,c2:0.4766902,c3:0,c4:1;n:type:ShaderForge.SFN_RemapRange,id:7242,x:31936,y:33035,varname:node_7242,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-5945-OUT;proporder:4567-9234;pass:END;sub:END;*/

Shader "Shader Forge/UI-Glow" {
    Properties {
        _MaxGlow ("MaxGlow", Range(1, 2)) = 2
        _node_9234 ("node_9234", Color) = (0.9117647,0.4766902,0,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform float _MaxGlow;
            uniform float4 _node_9234;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                #ifdef PIXELSNAP_ON
                    o.pos = UnityPixelSnap(o.pos);
                #endif
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_6228 = _Time;
                float3 emissive = (_node_9234.rgb*lerp(_MaxGlow,1.0,(sin((1.0*node_6228.a))*0.5+0.5)));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
