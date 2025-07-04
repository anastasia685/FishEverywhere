Shader "Custom/DitheredFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Fade ("Fade Amount", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        fixed4 _Color;
        float _Fade;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        // Simple 4x4 Bayer matrix dithering
        float DitherThreshold4x4(int2 pixelCoord)
        {
            int x = pixelCoord.x & 3;
            int y = pixelCoord.y & 3;
            int index = x + y * 4;

            float thresholds[16] = {
                0.0,   0.5,   0.125, 0.625,
                0.75,  0.25,  0.875, 0.375,
                0.1875,0.6875,0.0625,0.5625,
                0.9375,0.4375,0.8125,0.3125
            };

            return thresholds[index];
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.screenPos = UnityObjectToClipPos(v.vertex);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // clip-space to screen-space
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            int2 pixelCoord = int2(floor(screenUV * _ScreenParams.xy));

            // dither threshold test
            float threshold = DitherThreshold4x4(pixelCoord);
            clip(_Fade - threshold); // dithered clipping

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
