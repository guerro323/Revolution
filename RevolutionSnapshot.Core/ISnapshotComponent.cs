using RevolutionSnapshot.Core.ECS;

namespace RevolutionSnapshot.Core
{
	/// <summary>
	/// A snapshot data contain information about current data on a snapshot...
	/// </summary>
	public interface ISnapshotComponent<T> : IRevolutionComponent
		where T : ISnapshotComponent<T>
	{
		/// <summary>
		/// Interpolate between current value and next value
		/// </summary>
		/// <param name="nextState"></param>
		/// <param name="factor">The interpolation amount [0..1]</param>
		void Interpolate(in T nextState, float factor);
	}
}