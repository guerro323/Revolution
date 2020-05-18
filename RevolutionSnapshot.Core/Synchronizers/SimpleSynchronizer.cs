using System;
using RevolutionSnapshot.Core.ECS;

namespace RevolutionSnapshot.Core.Synchronizers
{
	public class SimpleSynchronizer<T>
		where T : ISnapshotComponent<T>
	{
		public void OnSerialize(ReadOnlySpan<RevolutionChunk> chunks)
		{
			foreach (ref readonly var chunk in chunks)
			{
				foreach (ref readonly var component in chunk.GetComponents<T>())
				{
					
				}
			}
		}
	}
}