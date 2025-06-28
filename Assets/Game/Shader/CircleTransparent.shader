Shader "Custom/CircleTransparent" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1
        _Feather ("Feather", Range(0,1)) = 0.5
        _MinAlpha ("Min Alpha", Range(0,1)) = 0.3 // 添加最小透明度属性
    }

    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _Center;
            float _Radius;
            float _Feather;
            float _MinAlpha;

            v2f vert (appdata v) {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 计算到中心的距离
                float2 center = _Center;
                float distance = length(i.worldPos.xy - center);
                
                // 计算基础透明度 (0=透明, 1=不透明)
                float alpha = smoothstep(_Radius * (1 - _Feather), _Radius, distance);
                
                // 应用最小透明度限制
                alpha = max(alpha, _MinAlpha);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a *= alpha; // 应用计算后的透明度
                return col;
            }
            ENDCG
        }
    }
}