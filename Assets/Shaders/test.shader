Shader "Unlit/test"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Value("Value", float ) = 1.0 
        _Height("Height,", float ) = 1.0
        _Color1("Color 1", Color ) = (1.0,1.0,1.0,1.0)
        _Color2("Color 2", Color ) = (1.0,1.0,1.0,1.0)
        _Start("Start", float) = 0.0
        _End("End", float) = 1.0
    }
        SubShader
    {
        Tags { "RenderType" = "Transparent"
               "Queue" = "Transparent"}
        LOD 100

        Pass
        {
            Cull Off
            BLEND One One
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float _Value;
            float _Height;
            float4 _Color1;
            float4 _Color2;
            float _Start;
            float _End;
            



            struct meshdata //per vertex mesh data
            {
                float4 vertex : POSITION; //position
                float3 normal : NORMAL; //vertex normals
                float4 color : COLOR; //vertex color
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0; //uv coords, channel 0
            };

            struct v2f //input for frag shader
            {
                float2 uv : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normals : TEXCOORD0;
                float3 objvert : TEXCOORD1;
            };

            v2f vert (meshdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); //local to clip space
                o.objvert = v.vertex;
                o.normals = v.normal;
                o.uv = v.uv;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float InverseLerp(float a, float b, float c) {
                return (c - a) / (b - a);
            }

            float4 frag(v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog

                //float4 colore = float4(0.5, (i.objvert.y+_Height/2)/_Height,0.7,1); //color based on height; adjusted to height of object.
                //float4 colore = lerp(_Color1, _Color2, InverseLerp(_Start, _End, (i.objvert.y + _Height / 2) / _Height)); //also color based on height, but using lerp
                //float4 colore = lerp(_Color1, _Color2, abs( frac( ( (i.objvert.y + _Height / 2) / _Height) * 3) *2-1 ) ); //triangle function
                float4 colore;
                if (i.normals.x == 0 && i.normals.z == 0) {
                    colore = float4(0, 0, 0, 0);
                }
                else {
                    colore = lerp(_Color1, _Color2, (cos(-1 * _Time.y * 4 + 20 * (i.objvert.y + _Height / 2) / _Height) + 1) / 2); //cosine wave
                }
                UNITY_APPLY_FOG(i.fogCoord, colore);
                return colore;
            }
            ENDCG
        }
    }
}
