using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RevolutionSnapshot.Core.ECS
{
	public partial class RevolutionWorld
	{
		private static bool HasSameComponents(RevolutionChunk target, IReadOnlyList<Type> components)
		{
			var matches = 0;
			if (target.ComponentTypes.Length != components.Count)
				return false;
			for (var i = 0; i != target.ComponentTypes.Length; i++)
			for (var y = 0; y != components.Count; y++)
				if (target.ComponentTypes[i] == components[y])
				{
					matches++;
					if (matches > target.ComponentTypes.Length)
						goto end;
				}

			if (matches == target.ComponentTypes.Length)
				return true;

			end:
			return false;
		}

		private RevolutionChunk GetOrCreateChunk(IReadOnlyList<Type> components)
		{
			var currentChunk = default(RevolutionChunk);
			foreach (var chunk in Chunks)
				if (HasSameComponents(chunk, components))
				{
					currentChunk = chunk;
					break;
				}

			// no chunk found, create it
			if (currentChunk == null)
			{
				currentChunk = new RevolutionChunk(components);

				Array.Resize(ref Chunks, Chunks.Length + 1);
				Chunks[^1] = currentChunk;
			}

			return currentChunk;
		}

		private void MoveToChunk(RawEntity entity, RevolutionChunk newChunk)
		{
			if (entityToChunk.TryGetValue(entity, out var previousChunk))
			{
				if (!ReferenceEquals(previousChunk, newChunk))
				{
					newChunk.AddEntity(entity);
					CopyComponents(entity, previousChunk, newChunk);
					previousChunk.RemoveEntity(entity);
				}
			}
			else
				newChunk.AddEntity(entity);

			entityToChunk[entity] = newChunk;
		}

		private static void CopyComponents(RawEntity entity, RevolutionChunk source, RevolutionChunk destination)
		{
			var sourceIdx      = source.IndexOf(entity);
			var destinationIdx = destination.IndexOf(entity);

			foreach (var (type, ca) in source.Components)
			{
				if (Array.IndexOf(destination.ComponentTypes, type) < 0)
					continue;
				ca.CopyTo(sourceIdx, destination.Components[type], destinationIdx);
			}
		}
	}
}