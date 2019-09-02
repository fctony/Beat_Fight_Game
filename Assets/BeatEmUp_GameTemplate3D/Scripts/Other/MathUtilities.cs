using UnityEngine;

public static class MathUtilities {

	//curve calculation for ease out effect
	public static float Sinerp(float start, float end, float value)	{
		return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
	}
	
	//curve calculation for ease in effect
	public static float Coserp(float start, float end, float value)	{
		return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
	}

	//curve calculation for easing at start + end
	public static float CoSinLerp(float start, float end, float value) {
        return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
    }
}
