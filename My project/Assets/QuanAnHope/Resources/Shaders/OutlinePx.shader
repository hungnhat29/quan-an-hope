// Vien muc (sprint 9 B-03, Art Bible muc 10): inverted hull day dinh ra theo phap tuyen DA LAM MUOT (OutlineMesh) mot khoang tinh bang
// DIEM ANH man hinh (_WidthPx o 1280x720, doi theo do phan giai), khong bao gio day hon _WidthWorld met khi o xa.
// Huong day = phap tuyen nhan ma tran vat (khong nghich dao chuyen vi) -> khoi vuong bi keo dai van day ra goc, khong ho goc;
// tam mong chi co net quanh vien. Mat sau bi day lui _DepthPush met theo tia nhin de khong lo truoc mat vat.
// Chi dung UNITY_MATRIX_VP + _WorldSpaceCameraPos + truc phai cua camera (URP dat chac chan cac bien nay; ma tran chieu rieng
// UNITY_MATRIX_P cua built-in co the khong dung / bi lat khi ve vao render texture).
Shader "BepNhanPham/OutlinePx"
{
    Properties
    {
        _Color ("Color", Color) = (0.17,0.14,0.13,1)
        _WidthPx ("Width (px at 720p)", Float) = 4
        _WidthWorld ("Max width (m)", Float) = 0.028
        _DepthPush ("Depth push (m)", Float) = 0.003
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
            float _WidthPx, _WidthWorld, _DepthPush;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };

            float2 Ndc(float3 wp)
            {
                float4 c = mul(UNITY_MATRIX_VP, float4(wp, 1));
                return c.xy / c.w;
            }

            float4 vert (appdata v) : SV_POSITION
            {
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 wd = mul((float3x3)unity_ObjectToWorld, v.normal);
                float wl = length(wd);
                wd = wl > 1e-6 ? wd / wl : float3(0, 1, 0);

                float3 toCam = wp - _WorldSpaceCameraPos.xyz;
                float dist = max(length(toCam), 0.01);
                float3 camRight = normalize(float3(UNITY_MATRIX_V[0].x, UNITY_MATRIX_V[0].y, UNITY_MATRIX_V[0].z));

                // do day (diem anh): theo man hinh, nhung khong qua do day the gioi _WidthWorld khi o xa
                float2 halfScreen = _ScreenParams.xy * 0.5;
                float2 n0 = Ndc(wp);
                float pxWorld = length((Ndc(wp + camRight * _WidthWorld) - n0) * halfScreen);
                float pxRef = _WidthPx * _ScreenParams.y / 720.0;
                float px = min(pxRef, pxWorld);

                // huong tren man hinh cua phap tuyen
                float2 dir = (Ndc(wp + wd * 0.01) - n0) * halfScreen;
                float dl = length(dir);
                dir = dl > 1e-5 ? dir / dl : float2(0, 0);

                // day lui theo tia nhin roi day ngang tren man hinh
                float3 pushed = wp + toCam / dist * _DepthPush;
                float4 clip = mul(UNITY_MATRIX_VP, float4(pushed, 1));
                clip.xy += dir * px / halfScreen * clip.w;
                return clip;
            }

            fixed4 frag () : SV_Target { return _Color; }
            ENDCG
        }
    }
}
