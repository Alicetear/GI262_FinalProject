Shader "Outpost/Menu Watchtower"
{
    Properties
    {
        [PerRendererData] _MainTex ("Empty road", 2D) = "white" {}
        _BeaconTint ("Beacon red", Color) = (0.62745,0.21176,0.23529,1)
        _SweepSeconds ("Sweep period", Range(3,30)) = 11
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _BeaconTint;
            float _SweepSeconds;

            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Varying { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            Varying vert(Input v)
            {
                Varying o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(Varying i) : SV_Target
            {
                float4 road = tex2D(_MainTex, i.uv);
                float2 lamp = float2(0.846, 0.863);
                float groundY = 0.081;
                float sweep = sin(_Time.y * 6.2831853 / _SweepSeconds);
                float groundX = 0.645 + 0.185 * sweep;
                float depth = (lamp.y - i.uv.y) / (lamp.y - groundY);
                float axis = lerp(lamp.x, groundX, depth);
                float width = 0.002 + 0.052 * saturate(depth);
                float distance = abs(i.uv.x - axis) / width;
                float cone = exp2(-2.4 * distance * distance) * step(0, depth);
                cone *= 1.0 - smoothstep(0.94, 1.04, depth);
                float luminance = dot(road.rgb, float3(0.2126,0.7152,0.0722));
                float surface = saturate(12.0 * luminance);
                float fog = 0.79 + 0.10 * sin(49.0 * i.uv.y + 8.0 * i.uv.x);
                float beam = cone * fog * (0.28 + 0.45 * surface) * (1.0 - 0.43 * saturate(depth));
                float2 ground = (i.uv - float2(groundX,groundY)) / float2(0.09,0.028);
                float pool = exp2(-dot(ground, ground) * 1.7) * (0.13 + surface * 1.65);
                float2 lens = (i.uv - lamp) / float2(0.005,0.008);
                float glow = 0.55 * exp2(-dot(lens,lens));
                road.rgb += _BeaconTint.rgb * (beam + pool + glow);
                return road * i.color;
            }
            ENDCG
        }
    }
}
