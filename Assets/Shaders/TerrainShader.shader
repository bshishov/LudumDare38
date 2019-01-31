Shader "Custom/TerrainShader" {
	Properties {
		_Ramp("Lightning Ramp", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_MainTex("Bedrock", 2D) = "white" {}

		_State("State", 2D) = "white" {}

		_Dirt("Dirt", 2D) = "white" {}
		_Sand("Sand", 2D) = "white" {}
		_Snow("Snow", 2D) = "white" {}
		_Grass("Grass", 2D) = "white" {}
		_Swamp("Swamp", 2D) = "white" {}

		// Defined globally
		//_Selection("Selection", Vector) = (0, 0, 0, 1)		
		
		_SelectionColor("Selection Color", Color) = (1,1,1,1)

		_Scale("Scale", float) = 1

		_NoiseMap("Noise", 2D) = "white" {}
		_NoiseScale("Noise Scale", float) = 1
		_NoiseBlendSharpness("Noise Blend Sharpness",float) = 1
		
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
		sampler2D _State;
		sampler2D _Dirt;
		sampler2D _Sand;
		sampler2D _Snow;
		sampler2D _Grass;
		sampler2D _Swamp;
		sampler2D _Ramp;				
		sampler2D _NoiseMap;
		
		float _NoiseScale;
		float _NoiseBlendSharpness;

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
			float2 uv_State;
			float2 uv_Grid;
			//float4 vColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;		
		half4 _Selection;
		fixed4 _SelectionColor;
				
		#define RESCALE(X,A,B) (distance(X, A) / distance(B, A))
		#define CLAMP_RANGE(X,A,B) saturate(sign((X) - (A)) * sign((B) - (X)))
		#define IN_RANGE(X,A,B) CLAMP_RANGE(X, A, B) * (1 - abs(RESCALE(X, A, B) - 0.5) * 2.0)		

		//static const half smoothFactor = 10;

		half moreThan(half x, half a, half smoothFactor = 10)
		{	
			return 1 / (1 + exp((a - x) / smoothFactor));
		}

		half lessThan(half x, half b, half smoothFactor = 10)
		{	
			return 1 / (1 + exp((x - b) / smoothFactor));
		}

		half between(half x, half a, half b, half smoothFactor=10)
		{	
			half p1 = 1 + exp((a - x) / smoothFactor);
			half p2 = 1 + exp((x - b) / smoothFactor);
			return 1 / (p1 * p2);
		}

		half not(half x) { return 1 - x; }

		void surf(Input IN, inout SurfaceOutputStandard  o)
		{
			// Find our UVs for each axis based on world position of the fragment.
			half2 yUV = IN.worldPos.xz / _NoiseScale;
			half2 xUV = IN.worldPos.zy / _NoiseScale;
			half2 zUV = IN.worldPos.xy / _NoiseScale;
			// Now do texture samples from our diffuse map with each of the 3 UV set's we've just made.
			half yDiff = tex2D(_NoiseMap, yUV).r;
			half xDiff = tex2D(_NoiseMap, xUV).r;
			half zDiff = tex2D(_NoiseMap, zUV).r;
			// Get the absolute value of the world normal.
			// Put the blend weights to the power of BlendSharpness, the higher the value, 
			// the sharper the transition between the planar maps will be.
			half3 blendWeights = pow(abs(IN.worldNormal), _NoiseBlendSharpness);
			// Divide our blend mask by the sum of it's components, this will make x+y+z=1
			blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
			// Finally, blend together all three samples based on the blend mask.
			half noise = xDiff * blendWeights.x + yDiff * blendWeights.y + zDiff * blendWeights.z;
			noise = noise * 2;			

			// Terrain state
			#define TERRAIN_MAX_HEIGHT 4
			#define TERRAIN_SEA_LEVEL 0
			half3 state = tex2D(_State, IN.uv_State);
			half height = lerp(-1000, 4000, (1 + IN.worldPos.y) / TERRAIN_MAX_HEIGHT); // meters
			half temperature = lerp(-100, 100, state.r); // celsius
			half humidity = lerp(0, 100, state.g);  // in percentage
			
			// BASE TERRAIN Rules
			// k values represent the "presence" factor and defined by rules that return 
			// values in range 0..1
			// multiplication (*) represents logical AND
			// addition (+) states for logical OR
			// So expression like:
			//		between(temperature, -40, 40) * moreThan(humidity, 40)
			// Should be readed as:
			//  if 
			//		temperature between -40 and 40 degrees celcius
			//	and
			//		humidity more than 40%

			// Snow layer
			half k1 = between(temperature, -100, 0);			
			
			// Dirt layer
			half k2 = between(temperature, -40, 40) * moreThan(humidity, 40);			
			
			// Swamp layer
			half k3 = between(temperature, 25, 100) * moreThan(humidity, 60);			
			
			// Grass layer
			half k4 = between(temperature, 0, 30) * moreThan(humidity, 20);			
			
			// Sand layer
			half k5 = between(temperature, 40, 100) * lessThan(humidity, 50) + 
					  lessThan(height, TERRAIN_SEA_LEVEL + 350, 50) +
					  moreThan(height, TERRAIN_SEA_LEVEL + 2200, 50) * between(temperature, 10, 100);
						
			// Make sure everything is in bounds
			k1 = saturate(k1);
			k2 = saturate(k2);
			k3 = saturate(k3);
			k4 = saturate(k4);
			k5 = saturate(k5);			
			half k6 = saturate(blendWeights.r + blendWeights.b);

			// Textures
			half3 v1 = tex2D(_Snow, IN.uv_MainTex).rgb;
			half3 v2 = tex2D(_Dirt, IN.uv_MainTex).rgb;
			half3 v3 = tex2D(_Swamp, IN.uv_MainTex).rgb;
			half3 v4 = tex2D(_Grass, IN.uv_MainTex).rgb;
			half3 v5 = tex2D(_Sand, IN.uv_MainTex).rgb;			

			// [!!!BLENDING!!!]
			// Base color
			half3 bedrock = tex2D(_MainTex, IN.uv_MainTex);
			half3 color = bedrock;

			// Converts the PRESENCE factor to alpha of the layer
			#define ALPHA(x) saturate(2 * x * (1 + 0.6 * (noise * noise - 0.5)));
			k1 = ALPHA(k1);
			k2 = ALPHA(k2);
			k3 = ALPHA(k3);
			k4 = ALPHA(k4);
			k5 = ALPHA(k5);			

			// Alpha blending the layers
			color = v1 * k1 + color * (1 - k1);
			color = v2 * k2 + color * (1 - k2);
			color = v3 * k3 + color * (1 - k3);
			color = v4 * k4 + color * (1 - k4);
			color = v5 * k5 + color * (1 - k5);

			color = bedrock * k6 + color * (1 - k6);


			
			o.Albedo = color;

			// Selection			
			half selectionSize = _Selection.w * 0.5;			
			half r = selectionSize * selectionSize * selectionSize * selectionSize;					
			half selectionMask = saturate(1 - (pow(IN.worldPos.x - _Selection.x, 4)/r  + pow(IN.worldPos.z - _Selection.y, 4) / r));
			half selectionOutline = saturate(selectionMask - sign(selectionMask - 0.5)) * selectionMask;
			selectionMask = saturate(selectionOutline) * _SelectionColor.a;

			o.Albedo = selectionMask * _SelectionColor.rgb  + o.Albedo * (1 - selectionMask);
			o.Emission = selectionMask * _SelectionColor.rgb;			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
