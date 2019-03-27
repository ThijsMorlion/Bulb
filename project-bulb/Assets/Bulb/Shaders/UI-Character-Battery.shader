// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33274,y:32678,varname:node_3138,prsc:2|emission-8784-OUT,alpha-4681-A;n:type:ShaderForge.SFN_Slider,id:1790,x:31850,y:32919,ptovrint:False,ptlb:BlendFactor,ptin:_BlendFactor,varname:node_1790,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Tex2d,id:4681,x:32256,y:32540,ptovrint:False,ptlb:Neutral,ptin:_Neutral,varname:node_4681,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:8c76ab53798dfb34eb5bbb9d55fc60b6,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:8784,x:32524,y:32809,varname:node_8784,prsc:2|A-4681-RGB,B-3521-RGB,T-1790-OUT;n:type:ShaderForge.SFN_Tex2d,id:3521,x:32256,y:32720,ptovrint:False,ptlb:Quarter,ptin:_Quarter,varname:_Neutral_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:cb6bac068dc151a41b4a6bd710f0f098,ntxv:0,isnm:False;proporder:4681-1790-3521;pass:END;sub:END;*/

Shader "Shader Forge/UI-Character-Battery" {
    Properties {
        [HideInInspector]_Neutral ("Neutral", 2D) = "white" {}
        _BlendFactor ("BlendFactor", Range(0, 1)) = 0
        [HideInInspector]_Quarter ("Quarter", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float _BlendFactor;
            uniform sampler2D _Neutral; uniform float4 _Neutral_ST;
            uniform sampler2D _Quarter; uniform float4 _Quarter_ST;
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
                float4 _Neutral_var = tex2D(_Neutral,TRANSFORM_TEX(i.uv0, _Neutral));
                float4 _Quarter_var = tex2D(_Quarter,TRANSFORM_TEX(i.uv0, _Quarter));
                float3 emissive = lerp(_Neutral_var.rgb,_Quarter_var.rgb,_BlendFactor);
                float3 finalColor = emissive;
                return fixed4(finalColor,_Neutral_var.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
