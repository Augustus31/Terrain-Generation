Shader "Custom/surfTest"
{
    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture 1", 2D) = "white" {}
        _MainTexNM("Texture 1 Normal Map", 2D) = "white"{}
        _MountainTex("Texture 2", 2D) = "white" {}
        _MountainTexNM("Texture 2 Normal Map", 2D) = "white"{}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Low("Low", float) = -0.5
        _High("High", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows //vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MainTexNM;
        sampler2D _MountainTex;
        sampler2D _MountainTexNM;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MountainTex;
            float2 uv_MainTexNM;
            float2 uv_MountainTexNM;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        float _Low;
        float _High;
        //fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        float InverseLerp(float a, float b, float c) {
            return (c - a) / (b - a);
        }

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //o.customColor = abs(v.normal); pass stuff like this
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float relHeight = InverseLerp(_Low, _High, IN.worldPos.y);
            float4 col = tex2D(_MainTex, IN.uv_MainTex);
            float4 col2 = tex2D(_MountainTex, IN.uv_MountainTex);
            float4 outcol = (0.5, 0.5, 0.5, 1);
            if (relHeight <= 0.7) {
                outcol = col;
                o.Normal = UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM));
            }
            if (relHeight > 0.7 && relHeight < 0.8) {
                outcol = col * ((0.8 - relHeight) / 0.1) + (1 - (0.8 - relHeight) / 0.1) * col2;
                o.Normal = UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM)) * ((0.8 - relHeight) / 0.1) + (1 - (0.8 - relHeight) / 0.1) * UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM));
            }
            if (relHeight >= 0.8) {
                outcol = col2;
                o.Normal = UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM));
            }
            // Albedo comes from a texture tinted by color
            o.Albedo = outcol.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = outcol.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
