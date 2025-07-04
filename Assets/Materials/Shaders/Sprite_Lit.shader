Shader "Custom/TileMap"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1 // For alpha clipping shadows
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On // Enables correct depth rendering for shadows
        Cull Off  // Ensures sprite renders from both sides

        CGPROGRAM
        #pragma surface surf Standard alpha:clip fullforwardshadows addshadow

        sampler2D _MainTex;
        fixed4 _Color;
        float _Cutoff; // Used for alpha cutoff

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            clip(c.a - _Cutoff); // Ensures shadows work with alpha areas
        }
        ENDCG
    }

    FallBack "TransparentCutout"
}
