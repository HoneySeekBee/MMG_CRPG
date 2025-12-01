Shader "Custom/InternalAura"
{
	
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color1 ("Color A", Color) = (1,0,0,1)
        _Color2 ("Color B", Color) = (0,1,0,1)
        _Color3 ("Color C", Color) = (0,0,1,1)

        _Speed ("Shift Speed", Range(0,5)) = 1
        _Saturation ("Color Strength", Range(0,2)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;

            float _Speed;
            float _Saturation;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 알파는 스프라이트 그대로
                float alpha = tex2D(_MainTex, i.uv).a;

                // 시간 기반 색 전환
                float t = frac(_Time.y * _Speed);

                float3 c1 = lerp(_Color1.rgb, _Color2.rgb, t);
                float3 c2 = lerp(_Color2.rgb, _Color3.rgb, t);
                float3 c3 = lerp(_Color3.rgb, _Color1.rgb, t);

                // UV 기반 약한 흐름 효과 추가 (더 멋지게 보임)
                float wave = sin(i.uv.x * 8 + _Time.y * 2) * 0.5 + 0.5;

                float3 color = lerp(c1, c2, wave);
                color = lerp(color, c3, t * wave);

                color *= _Saturation;

                return float4(color, alpha);
            }
            ENDCG
        }
    }
}
