// Mau phang 2 nac kieu hoat hinh. Sang co dinh tu tren xuong nen 4 goc phong sang deu nhau,
// chay giong nhau o Built-in va URP.
Shader "BepNhanPham/ToonFlat"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _ShadeColor ("Shade Color", Color) = (0.80,0.72,0.70,1)
        _Threshold ("Shade Threshold", Range(-1,1)) = -0.25
        _LightDir ("Light Direction (world)", Vector) = (0.2,1,-0.15,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ShadeColor;
            float _Threshold;
            float4 _LightDir;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 n : TEXCOORD1; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.n = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float ndl = dot(normalize(i.n), normalize(_LightDir.xyz));
                float lit = smoothstep(_Threshold - 0.02, _Threshold + 0.02, ndl);
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                c.rgb *= lerp(_ShadeColor.rgb, fixed3(1,1,1), lit);
                c.a = 1;
                return c;
            }
            ENDCG
        }
    }
}
