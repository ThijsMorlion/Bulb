// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33274,y:32678,varname:node_3138,prsc:2|emission-9656-OUT,alpha-4681-A;n:type:ShaderForge.SFN_Slider,id:1790,x:31441,y:32931,ptovrint:False,ptlb:BlendFactor,ptin:_BlendFactor,varname:node_1790,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Tex2d,id:4681,x:32256,y:32540,ptovrint:False,ptlb:Neutral,ptin:_Neutral,varname:node_4681,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:8bca1e8fadcce544f83697c49b4d876b,ntxv:0,isnm:False;n:type:ShaderForge.SFN_RemapRange,id:8197,x:32038,y:32924,varname:node_8197,prsc:2,frmn:0,frmx:0.25,tomn:0,tomx:1|IN-1790-OUT;n:type:ShaderForge.SFN_Lerp,id:8784,x:32524,y:32809,varname:node_8784,prsc:2|A-4681-RGB,B-3521-RGB,T-8197-OUT;n:type:ShaderForge.SFN_Tex2d,id:3521,x:32256,y:32720,ptovrint:False,ptlb:Quarter,ptin:_Quarter,varname:_Neutral_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:61623bbcc0d0e794badb473db094862b,ntxv:0,isnm:False;n:type:ShaderForge.SFN_RemapRange,id:299,x:32038,y:33113,varname:node_299,prsc:2,frmn:0.25,frmx:0.5,tomn:0,tomx:1|IN-8624-OUT;n:type:ShaderForge.SFN_Clamp,id:7531,x:31804,y:32924,varname:node_7531,prsc:2|IN-1790-OUT,MIN-2779-OUT,MAX-9499-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2779,x:31583,y:33042,ptovrint:False,ptlb:node_2779,ptin:_node_2779,varname:node_2779,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:9499,x:31583,y:33124,ptovrint:False,ptlb:node_2779_copy,ptin:_node_2779_copy,varname:_node_2779_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.25;n:type:ShaderForge.SFN_Clamp,id:8624,x:31804,y:33113,varname:node_8624,prsc:2|IN-1790-OUT,MIN-9499-OUT,MAX-6869-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6869,x:31583,y:33215,ptovrint:False,ptlb:node_2779_copy_copy,ptin:_node_2779_copy_copy,varname:_node_2779_copy_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_ValueProperty,id:4592,x:31583,y:33319,ptovrint:False,ptlb:node_2779_copy_copy_copy,ptin:_node_2779_copy_copy_copy,varname:_node_2779_copy_copy_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:2542,x:32256,y:32924,ptovrint:False,ptlb:Quarter_copy,ptin:_Quarter_copy,varname:_Quarter_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:918ca9223a30c4b4cb26094903e9ce18,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:4993,x:32728,y:32950,varname:node_4993,prsc:2|A-8784-OUT,B-2542-RGB,T-299-OUT;n:type:ShaderForge.SFN_Tex2d,id:4387,x:32256,y:33136,ptovrint:False,ptlb:Quarter_copy_copy,ptin:_Quarter_copy_copy,varname:_Quarter_copy_copy,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f72b2db740cb81c449292ee7ff4b02c4,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Clamp,id:8067,x:31804,y:33285,varname:node_8067,prsc:2|IN-1790-OUT,MIN-6869-OUT,MAX-4592-OUT;n:type:ShaderForge.SFN_RemapRange,id:1344,x:32038,y:33298,varname:node_1344,prsc:2,frmn:0.5,frmx:1,tomn:0,tomx:1|IN-8067-OUT;n:type:ShaderForge.SFN_Lerp,id:9656,x:32949,y:33109,varname:node_9656,prsc:2|A-4993-OUT,B-4387-RGB,T-1344-OUT;proporder:4681-1790-3521-9499-6869-4592-2542-4387;pass:END;sub:END;*/

Shader "Shader Forge/UI-Character-Bulb" {
    Properties {
        [HideInInspector]_Neutral ("Neutral", 2D) = "white" {}
        _BlendFactor ("BlendFactor", Range(0, 1)) = 1
        [HideInInspector]_Quarter ("Quarter", 2D) = "white" {}
        [HideInInspector]_node_2779_copy ("node_2779_copy", Float ) = 0.25
        [HideInInspector]_node_2779_copy_copy ("node_2779_copy_copy", Float ) = 0.5
        [HideInInspector]_node_2779_copy_copy_copy ("node_2779_copy_copy_copy", Float ) = 1
        [HideInInspector]_Quarter_copy ("Quarter_copy", 2D) = "white" {}
        [HideInInspector]_Quarter_copy_copy ("Quarter_copy_copy", 2D) = "white" {}
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
            uniform float _node_2779_copy;
            uniform float _node_2779_copy_copy;
            uniform float _node_2779_copy_copy_copy;
            uniform sampler2D _Quarter_copy; uniform float4 _Quarter_copy_ST;
            uniform sampler2D _Quarter_copy_copy; uniform float4 _Quarter_copy_copy_ST;
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
                float4 _Quarter_copy_var = tex2D(_Quarter_copy,TRANSFORM_TEX(i.uv0, _Quarter_copy));
                float4 _Quarter_copy_copy_var = tex2D(_Quarter_copy_copy,TRANSFORM_TEX(i.uv0, _Quarter_copy_copy));
                float3 emissive = lerp(lerp(lerp(_Neutral_var.rgb,_Quarter_var.rgb,(_BlendFactor*4.0+0.0)),_Quarter_copy_var.rgb,(clamp(_BlendFactor,_node_2779_copy,_node_2779_copy_copy)*4.0+-1.0)),_Quarter_copy_copy_var.rgb,(clamp(_BlendFactor,_node_2779_copy_copy,_node_2779_copy_copy_copy)*2.0+-1.0));
                float3 finalColor = emissive;
                return fixed4(finalColor,_Neutral_var.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
