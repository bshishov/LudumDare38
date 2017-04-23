Shader "Custom/TerrainShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Ramp("Lightning Ramp", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_Dirt("Dirt", 2D) = "white" {}
		_Sand("Sand", 2D) = "white" {}
		_Snow("Snow", 2D) = "white" {}
		_Grass("Grass", 2D) = "white" {}

		_Grid("Grid", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Ramp fullforwardshadows
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Dirt;
		sampler2D _Sand;
		sampler2D _Snow;
		sampler2D _Grass;
		sampler2D _Ramp;		
		sampler2D _Grid;

		half4 LightingRamp(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			half diff = NdotL * 0.5 + 0.5;
			half3 ramp = tex2D(_Ramp, float2(diff, diff)).rgb;
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
			float2 uv_Grid;
			float4 vColor : COLOR;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;		

		void surf(Input IN, inout SurfaceOutputStandard  o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo = c.rgb;			

			half height = clamp((IN.worldPos.y - 0.5) * 0.7, 0, 1);
			half temperature = 255.0 * IN.vColor.r - 100 + _SinTime.z * 5;
			
			half dissolve = clamp((temperature  + 20.0) / 60.0, 0, 1) + c.r * 0.2;
			dissolve = pow(dissolve, 5);
			half4 col1 = tex2D(_Snow, IN.uv_MainTex);
			half4 col2 = tex2D(_Grass, IN.uv_MainTex);
			o.Albedo = lerp(col1, col2, dissolve).rgb;

			/*
			if (height < 0.2)
			{
				dissolve = (height - 0) / (0.3 - 0);
				col1 = tex2D(_Sand, IN.uv_MainTex);
				col2 = tex2D(_Grass, IN.uv_MainTex);
			}
			else if (height > 0.3 && height < 0.6)
			{
				dissolve = (height - 0.3) / (0.6 - 0.3);
				col1 = tex2D(_Grass, IN.uv_MainTex);
				col2 = tex2D(_Dirt, IN.uv_MainTex);
				
			}
			else if (height > 0.6)
			{
				dissolve = (height - 0.6) / (1 - 0.6);
				col1 = tex2D(_Dirt, IN.uv_MainTex);
				col2 = tex2D(_Snow, IN.uv_MainTex);				
			}*/
			
			o.Albedo = lerp(col1, col2, dissolve * dissolve).rgb;
		
			float f = frac(IN.worldPos.x);			
			float distToX = max(1 - f, f);
			//o.Albedo *= fmod(IN.worldPos.x, 1);
			//o.Albedo *= fmod(IN.worldPos.z, 1);
			o.Albedo *= 1 - tex2D(_Grid, IN.uv_Grid).r * (1 - _Color);
			
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic; 
			o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
