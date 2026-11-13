// Vien den: ve mat sau cua mot ban sao to hon mot chut (inverted hull).
Shader "BepNhanPham/Outline"
{
    Properties { _Color ("Color", Color) = (0.17,0.14,0.13,1) }
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
            float4 vert (float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
            fixed4 frag () : SV_Target { return _Color; }
            ENDCG
        }
    }
}
