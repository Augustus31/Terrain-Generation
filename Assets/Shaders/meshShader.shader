Shader "Unlit/meshShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MountainTex("Texture", 2D) = "white" {}
        _Low ("Low", float) = -0.5
        _High ("High", float) = 0.5
        
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MountainTex;
            float4 _Mountain_ST;
            float _Low;
            float _High;

            struct meshdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldpos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (meshdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldpos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float InverseLerp(float a, float b, float c) {
                return (c - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float relHeight = InverseLerp(_Low, _High, i.worldpos.y);
                float4 col = tex2D(_MainTex, i.uv*50);
                if (relHeight > 0.7 && relHeight < 0.8) {
                    col = col * ((0.8 - relHeight) / 0.1) + (1 - (0.8-relHeight)/0.1) * tex2D(_MountainTex, i.uv * 50);
                }
                if (relHeight > 0.8) {
                    col = tex2D(_MountainTex, i.uv * 50);
                }
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
