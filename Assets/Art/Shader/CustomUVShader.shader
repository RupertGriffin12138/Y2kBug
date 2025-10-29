Shader "Custom/UVShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeProgress ("Fade Progress", Range(0,1)) = 0
        _FadeDirection ("Fade Direction", Vector) = (1, -1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
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

            sampler2D _MainTex;
            float _FadeProgress;
            float2 _FadeDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // ��������ϵ����µĽ���
                // ���Ͻ�UV����Ϊ(0,1)�����½�Ϊ(1,0)
                float fadeValue = (i.uv.x + (1 - i.uv.y)) / 2.0f;
                
                // ���ݽ�����ȵ���͸����
                float alpha = step(fadeValue, _FadeProgress);
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}