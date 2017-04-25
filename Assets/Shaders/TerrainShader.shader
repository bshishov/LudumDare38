Shader "Custom/TerrainShader" {
	Properties {
		_GridColor ("Grid Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Ramp("Lightning Ramp", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_Dirt("Dirt", 2D) = "white" {}
		_Sand("Sand", 2D) = "white" {}
		_Snow("Snow", 2D) = "white" {}
		_Grass("Grass", 2D) = "white" {}
		_Swamp("Swamp", 2D) = "white" {}

		_Grid("Grid", 2D) = "white" {}
		_SelectedCell("Metallic", int) = 0 

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
		sampler2D _Swamp;
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
		fixed4 _GridColor;
		int _SelectedCell;

		//#define RESCALE(X,A,B) (abs((X) - (A)) / abs((B) - (A)))
		#define RESCALE(X,A,B) (distance(X, A) / distance(B, A))
		#define CLAMP_RANGE(X,A,B) clamp(sign((X) - (A)) * sign((B) - (X)), 0, 1)
		//#define IN_RANGE(X,A,B) (1.0 - (clamp(RESCALE(X,A,B), 0.0, 1.0) - 0.5) * 2.0)		
		#define IN_RANGE(X,A,B) CLAMP_RANGE(X, A, B) * (1 - abs(RESCALE(X, A, B) - 0.5) * 2.0)
		//#define IN_RANGE(X,A,B) CLAMP_RANGE(X, A, B)

		void surf(Input IN, inout SurfaceOutputStandard  o)
		{
			fixed dissolve = tex2D(_MainTex, IN.uv_MainTex).r;
			half height = IN.worldPos.y;
			half temperature = 255.0 * IN.vColor.r - 100.0;// +_SinTime.z * 5;
			half humidity = 255.0 * IN.vColor.g;// +_SinTime.z * 5;
			
			// BASE TERRAIN
			half snow = clamp(IN_RANGE(temperature, -20.0, 40.0) + IN_RANGE(height, 1.2, 3.0), 0, 1);  			
			half dirt = IN_RANGE(temperature, 10, 40.0);			
			half swamp = IN_RANGE(temperature, 25.0, 100.0) * IN_RANGE(humidity, 50.0, 110.0);
			half grass = IN_RANGE(temperature, 25.0, 100.0) * IN_RANGE(humidity, -10.0, 110.0);
			half sand = clamp(IN_RANGE(temperature, 50.0, 160.0) + IN_RANGE(height, -0.5, 0.2), 0, 1);
			

			half itotal = 1.0 / (snow + dirt + sand + grass + swamp + 0.01);

			//o.Albedo = IN.vColor;
			o.Albedo += itotal * snow * tex2D(_Snow, IN.uv_MainTex).rgb;
			o.Albedo += itotal * grass * tex2D(_Grass, IN.uv_MainTex).rgb;
			o.Albedo += itotal * swamp * tex2D(_Swamp, IN.uv_MainTex).rgb;
			o.Albedo += itotal * dirt * tex2D(_Dirt, IN.uv_MainTex).rgb;
			o.Albedo += itotal * sand * tex2D(_Sand, IN.uv_MainTex).rgb;
			
		

			uint i = _SelectedCell % 30;
			uint j = _SelectedCell / 30;
			float isSelected = CLAMP_RANGE(IN.worldPos.x + 15.0, i, i + 1) * CLAMP_RANGE(IN.worldPos.z + 15.0, j, j + 1);
			//o.Albedo *= 1 + isSelected * 2;


			// Grid			
			fixed grid = tex2D(_Grid, IN.uv_Grid).r;
			o.Albedo = (1 - grid) * o.Albedo + grid * _GridColor + isSelected * grid;
			o.Emission = grid * _GridColor;
			
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic; 
			o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
