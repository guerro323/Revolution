namespace RevolutionSnapshot.Core
{
	public static class MathHelper
	{
		public static float Lerp(float current, float target, float amount)
		{
			return current + (target - current) * amount;
		}
	}
}