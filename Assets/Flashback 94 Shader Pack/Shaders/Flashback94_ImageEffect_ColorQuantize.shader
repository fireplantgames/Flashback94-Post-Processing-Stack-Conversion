
//////////////////////////////////////////////////////////////////////////////////////////
//																						//
// Flashback '94 Shader Pack for Unity 3D												//
// © 2018 George Kokoris          														//
//																						//
// Image effect shader for use with the 'Flashback94_PostProcess' class					//
//																						//
// Modified by Fire Plant Games to work with the new post procesing stack               //
//////////////////////////////////////////////////////////////////////////////////////////

Shader "Hidden/Flashback 94/Color Quantize"
{
	Properties
	{
		_MainTex ("Render Input", 2D) = "white" {}
		_ColorSteps ("Color Steps", Float) = 256
		_ShouldStep ("Should Step Colors", Float) = 1.0
	}

	SubShader
	{
		ZTest Always
		ZWrite Off
		Cull Off

		Fog { Mode Off }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			half _ColorSteps;
			half _ShouldStep;
			
			fixed4 frag(v2f_img i) : SV_Target
			{
				if (_ShouldStep < 0)
					return tex2D(_MainTex, i.uv);

				half stepValue = 1 / floor(_ColorSteps + 0.5);
				fixed4 level = tex2D(_MainTex, i.uv) / stepValue;
				
				return floor(level + 0.5) * stepValue;
			}

			ENDCG
		}
	}
}
