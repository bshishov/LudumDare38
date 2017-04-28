Shader "Custom/SimpleWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent-10" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				fixed4 diff : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			uniform sampler2D _CameraDepthTexture;
			
			v2f vert (appdata_base v)
			{
				v2f o;				
				v.vertex.y = sin(v.vertex.x * 20 + v.vertex.z * 10 + _Time.z) * 0.05f;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex) + half2(_Time.y, -_Time.y) * 0.05;
				UNITY_TRANSFER_FOG(o,o.vertex);			

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;

				// the only difference from previous shader:
				// in addition to the diffuse lighting from the main light,
				// add illumination from ambient or light probes
				// ShadeSH9 function from UnityCG.cginc evaluates it,
				// using world space normal
				o.diff.rgb += ShadeSH9(half4(worldNormal, 1));


				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
							
				fixed4 col = tex2D(_MainTex, i.uv) * i.diff;				
				col.a = pow(col.b, 3) * _Color.r + _Color.g;				

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
