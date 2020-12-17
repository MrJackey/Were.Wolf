Shader "Sprites/Void"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0, 0, 0, 1)
		_FadeHeight ("Fade Height", Float) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
			"DisableBatching"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
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
				float2 worldPos : POSITION1;
				float2 origin : POSITION2;
			};

			sampler2D _MainTex;
			CBUFFER_START(UnityPerMaterial)
				fixed4 _Color;
				float _FadeHeight;
			CBUFFER_END

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				// Top right corner of sprite in world space.
				o.origin = mul(unity_ObjectToWorld, float4(0.5, 0.5, 0, 1));

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float dist = -(i.worldPos - i.origin).y;
				float value = smoothstep(0, _FadeHeight, dist);
				return _Color * value;
			}
			ENDCG
		}
	}
}