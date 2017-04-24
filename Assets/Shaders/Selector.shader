// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Selector"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Sparks("Sparks", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_SparksColor("Sparks Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha			
			//Blend One OneMinusSrcAlpha			
			ZWrite Off
			Cull Off

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
			sampler2D _Sparks;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _SparksColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				

				float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
				o.uv = TRANSFORM_TEX(v.uv * half2(scaleX, 1), _MainTex);			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half2 dir = half2(_Time.y * 0.25, 0);
				fixed4 col1 = tex2D(_MainTex, i.uv + dir);
				fixed4 col2 = tex2D(_MainTex, i.uv - dir);
				fixed4 sparks = tex2D(_Sparks, i.uv * 2 + half2(0, -_Time.y)) * (1 - i.uv.y) * (1 - i.uv.y);

				fixed4 res = lerp(col1, col2, _SinTime.z * 0.5 + 0.5) * _Color;
				return res + (1 - res.a) * sparks * _SparksColor;
			}
			ENDCG
		}
	}
}
