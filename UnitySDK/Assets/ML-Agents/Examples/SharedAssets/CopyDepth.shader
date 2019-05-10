Shader "Hidden/CopyDepth"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _CameraDepthTexture;

            fixed4 frag (v2f i) : SV_Target
            {
                float d = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r) * 0xFFFFFF;
                fixed4 col;

                col.r = floor(fmod(d/0x10000, 0xFF)) / 255.0;
                col.g = floor(fmod(d/0x100, 0xFF)) / 255.0;
                col.b = floor(fmod(d, 0xFF)) / 255.0;

                return col;
            }
            ENDCG
        }
    }
}
