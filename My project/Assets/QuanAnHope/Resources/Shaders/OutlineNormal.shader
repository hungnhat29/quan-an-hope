// Vien muc cho model Blender (sprint 8 F-070 AC-4): inverted hull day dinh ra theo phap tuyen mot khoang _Width
// (don vi cua model), ve mat sau. Khac Outline.shader (phong to ca khoi) vi model khong phai khoi loi don gian.
Shader "BepNhanPham/OutlineNormal"
{
    Properties
    {
        _Color ("Color", Color) = (0.17,0.14,0.13,1)
        _Width ("Width (object units)", Float) = 0.008
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Width;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            float4 vert (appdata v) : SV_POSITION
            {
                float3 p = v.vertex.xyz + normalize(v.normal) * _Width;
                return UnityObjectToClipPos(float4(p, 1));
            }
            fixed4 frag () : SV_Target { return _Color; }
            ENDCG
        }
    }
}
