namespace RevolutionSnapshot.Core
{
	/// <summary>
	/// Provide generic cloning support for <see cref="T"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICloneable<out T>
	{
		T Clone();
	}
}