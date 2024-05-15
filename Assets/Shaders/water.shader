Shader "Custom/water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff("Cutoff", float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Cutoff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float InverseLerp(float a, float b, float c) {
            return (c - a) / (b - a);
        }

        float2 unity_gradientNoise_dir(float2 p)
        {
            p = p % 289;
            float x = (34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        float perlin(float2 p)
        {
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(unity_gradientNoise_dir(ip), fp);
            float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
        }

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float4 worldpos = mul(unity_ObjectToWorld, v.vertex);
            //v.vertex.y += (sin(perlin(3 * worldpos.xz * 2)+ 40.0 * _Time)-0.5)/15 + (sin(15 * worldpos.x - 3*worldpos.z + 50.0*_Time) - 0.5)/25 + (sin(9 * worldpos.z + 3 * worldpos.x + 30.0 * _Time)-0.5) / 25;
            float factor = 0.4;
            float timeChanger = sin(20 * perlin(0.2 * worldpos.xz) + 20 * _Time) / 10 + sin(8*worldpos.x - 5*worldpos.z + 23 * _Time)/15;
            v.vertex.y += factor * timeChanger;
            //o.pos = mul(unity_ObjectToWorld, v.vertex).y;
            //o.customColor = abs(v.normal); pass stuff like this

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 deepblue = float4(75, 180, 222, 1) / 256;
            float4 lightblue = float4(252, 252, 252, 1) / 256;
            float dif = IN.worldPos.y - _Cutoff;
            float relHeight = saturate(-1 + 3.5 / (1 + exp(-25 * (dif-0.045))));
            float4 outcol = lerp(deepblue, lightblue, relHeight);


            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = outcol.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
