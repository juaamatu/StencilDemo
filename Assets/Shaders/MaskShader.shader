Shader "Hidden/MaskShader"
{
    Properties
    {
         [HideInInspector] _PortalIndex ("PortalIndex", int) = 1
    }
    
    SubShader
    {
        ZWrite On ZTest LEqual

        Pass
        {
            Stencil
            {
                Ref [_PortalIndex]
                Comp Always
                Pass Replace
            }
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                return half4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
