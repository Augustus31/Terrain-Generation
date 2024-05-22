Shader "Custom/surf2"
{
    Properties
    {
        _MainTex("Texture 1", 2D) = "white" {}
        _MainTexNM("Texture 1 Normal Map", 2D) = "white"{}
        _MountainTex("Texture 2", 2D) = "white" {}
        _MountainTexNM("Texture 2 Normal Map", 2D) = "white"{}
        _RockTex("Texture 3", 2D) = "white" {}
        _RockTexNM("Texture 3 Normal Map", 2D) = "white"{}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Low("Low", float) = -0.5
        _High("High", float) = 0.5
        _CellSize("Cell Size", Range(0, 100)) = 0.1
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

        #include "Random.cginc"
        #include "WhiteNoise.cginc"

        sampler2D _MainTex;
        sampler2D _MainTexNM;
        sampler2D _MountainTex;
        sampler2D _MountainTexNM;
        sampler2D _RockTex;
        sampler2D _RockTexNM;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MountainTex;
            float2 uv_MainTexNM;
            float2 uv_MountainTexNM;
            float2 uv_RockTex;
            float2 uv_RockTexNM;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        float _Low;
        float _High;
        float _CellSize;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

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

        float fbm(float a, float b, int octaves, float amp, float freq, bool set)
        {
            float result = 0;
            float ampmod = amp;
            float freqmod = freq;
            for (int it1 = 0; it1 < octaves; it1++)
            {
                result += ampmod * (perlin(float2(abs(1000 + a * freqmod), abs(1000 + b * freqmod)) - 0.5));
                ampmod = ampmod / 2;
                freqmod = freqmod * 2;
            }

            //calculate max and min
            float ampcalc = amp;
            float sum = 0;
            for (int it2 = 0; it2 < octaves; it2++)
            {
                sum += 0.5 * ampcalc;
                ampcalc = ampcalc / 2;
            }
            if (set) {
                _Low = -1 * sum;
                _High = sum;
            }
            return result;
        }

        float3 voronoiNoise(float2 value) {
            float2 baseCell = floor(value);

            //first pass to find the closest cell
            float minDistToCell = 10;
            float2 toClosestCell;
            float2 closestCell;
            [unroll]
            for (int x1 = -1; x1 <= 1; x1++) {
                [unroll]
                for (int y1 = -1; y1 <= 1; y1++) {
                    float2 cell = baseCell + float2(x1, y1);
                    float2 cellPosition = cell + rand2dTo2d(cell);
                    float2 toCell = cellPosition - value;
                    float distToCell = length(toCell);
                    if (distToCell < minDistToCell) {
                        minDistToCell = distToCell;
                        closestCell = cell;
                        toClosestCell = toCell;
                    }
                }
            }

            //second pass to find the distance to the closest edge
            float minEdgeDistance = 10;
            [unroll]
            for (int x2 = -1; x2 <= 1; x2++) {
                [unroll]
                for (int y2 = -1; y2 <= 1; y2++) {
                    float2 cell = baseCell + float2(x2, y2);
                    float2 cellPosition = cell + rand2dTo2d(cell);
                    float2 toCell = cellPosition - value;

                    float2 diffToClosestCell = abs(closestCell - cell);
                    bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y < 0.1;
                    if (!isClosestCell) {
                        float2 toCenter = (toClosestCell + toCell) * 0.5;
                        float2 cellDifference = normalize(toCell - toClosestCell);
                        float edgeDistance = dot(toCenter, cellDifference);
                        minEdgeDistance = min(minEdgeDistance, edgeDistance);
                    }
                }
            }

            //float random = rand2dTo1d(closestCell);
            float random = nrand(closestCell);
            return float3(minDistToCell, random, minEdgeDistance);
        }

        float InverseLerp(float a, float b, float c) {
            return (c - a) / (b - a);
        }

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //v.vertex.y += 6;
            //o.customColor = abs(v.normal); pass stuff like this

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float relHeight = InverseLerp(_Low, _High, IN.worldPos.y);
            float4 col = tex2D(_MainTex, IN.uv_MainTex * 10);
            float4 col2 = tex2D(_MountainTex, IN.uv_MountainTex * 10);
            float4 col3 = tex2D(_RockTex, IN.uv_RockTex * 20);
            float4 outcol = float4(0.5, 0.5, 0.5, 1);
            float snowStart = 0.7+ perlin(IN.worldPos.xz*0.5)/7;
            float snowFull = snowStart + 0.05; //+ perlin((IN.worldPos.xz + float2(500,500)) * 0.5) / 20;
            float rockStart = 0.5 + perlin(IN.worldPos.xz/2) / 6;
            float rockFull = rockStart + 0.09;

            //Shading of grass
            float4 deepGreen = float4(19, 139, 39, 1) / 256;
            float4 lightGreen = float4(191, 125, 55, 1) / 256;

            float correctedHeight = InverseLerp(0, rockFull, relHeight);
            float4 lerpedGreen = lerp(deepGreen, lightGreen, correctedHeight);

            float perlin1 = perlin(IN.worldPos.xz * 3) * 2;
            if (perlin1 < -0.5) {
                col = float4(25, 75, 21, 1) / 256;
            }
            else if (perlin1 < 0) {
                col = float4(35, 60, 30,1) / 256;
            }
            else if (perlin1 < 0.5) {
                col = float4(20, 65, 20, 1) / 256;
            }
            else {
                col = float4(31, 70, 25, 1) / 256;
            }
            col = col * lerpedGreen * 1.3;

            float2 posxz = IN.worldPos.xz / _CellSize;
            float noise = voronoiNoise(posxz).y;
            float4 brown = float4(87, 70, 70, 1) / 256;
            float4 gray = float4(115, 112, 112, 1) / 256;
            col3 = lerp(brown, gray, noise);

            float2 posxz2 = IN.worldPos.xz / (_CellSize/2);
            float noise2 = voronoiNoise(posxz2).y;
            float4 darkwhite = float4(199, 197, 197, 1) / 256;
            float4 lightwhite = float4(247, 247, 247, 1) / 256;
            col2 = lerp(darkwhite, lightwhite, noise2);

            float snowLerped = saturate(InverseLerp(snowStart, snowFull, relHeight));
            float rockLerped = saturate(InverseLerp(rockStart, rockFull, relHeight));
            o.Normal = lerp(lerp(UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM * 10)), UnpackNormal(tex2D(_RockTexNM, IN.uv_RockTexNM * 20)), rockLerped), UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM * 10)), snowLerped);
            outcol = lerp(lerp(col, col3, rockLerped), col2, snowLerped);

            /*if (snowLerped == 0) {
                //outcol = col* (1 - rockLerped) + col3 * (rockLerped);
                o.Normal = UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM * 10)) * (1 - rockLerped) + UnpackNormal(tex2D(_RockTexNM, IN.uv_RockTexNM * 20)) * (rockLerped);
            }
            else {
                //outcol = col3 * (1 - snowLerped) + col2 * (snowLerped);
                o.Normal = UnpackNormal(tex2D(_RockTexNM, IN.uv_RockTexNM * 20)) * (1 - snowLerped) + UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM * 10)) * (snowLerped);
            }*/
            //outcol = col * (1 - rockLerped) + col3 * (rockLerped) + col3 * (1 - snowLerped) + col2 * (snowLerped);
            //o.Normal = UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM * 10)) * (1 - rockLerped) + UnpackNormal(tex2D(_RockTexNM, IN.uv_RockTexNM * 10)) * (rockLerped) + UnpackNormal(tex2D(_RockTexNM, IN.uv_RockTexNM * 10)) * (1 - snowLerped) + UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM * 10)) * (snowLerped);
            //o.Normal = UnpackNormal(tex2D(_MainTexNM, IN.uv_MainTexNM * 10)) * (1 - lerped) + UnpackNormal(tex2D(_MountainTexNM, IN.uv_MountainTexNM * 10)) * lerped;

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = outcol.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = outcol.a;
        }
        ENDCG
    }
    //FallBack "Diffuse"
}
