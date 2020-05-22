using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Collections.Pooled;

namespace RevolutionSnapshot.Core.ECS
{
	/// <summary>
	///     A chunk contain entities with the same components.
	/// </summary>
	public class RevolutionChunk : IEquatable<RevolutionChunk>, IDisposable
	{
		/// <summary>
		///     Get the components of this chunk
		/// </summary>
		public readonly PooledDictionary<Type, ComponentArray> Components;

		/// <summary>
		///     Get the component types of this chunk
		/// </summary>
		public readonly Type[] ComponentTypes;

		private readonly PooledList<RawEntity>            entities;
		private readonly PooledDictionary<RawEntity, int> entityToIndex;

		private Action<int> updateEntityIndex;

		public RevolutionChunk(IEnumerable<Type> components)
		{
			ComponentTypes = components.ToArray();
			entities       = new PooledList<RawEntity>();
			Components     = new PooledDictionary<Type, ComponentArray>(ComponentTypes.Length);
			foreach (var type in ComponentTypes) Components[type] = new ComponentArray(type);

			entityToIndex = new PooledDictionary<RawEntity, int>();
			updateEntityIndex = i => entityToIndex[entities[i]] = i;
		}

		/// <summary>
		///     Get the array of entities. If an entity was added or removed, the array will be recomputed.
		/// </summary>
		public Span<RawEntity> Span => entities.Span;

		/// <summary>
		/// Dispose and clear component data
		/// </summary>
		public void Dispose()
		{
			Array.Clear(ComponentTypes, 0, ComponentTypes.Length);
			entities.Clear();
			foreach (var component in Components.Values)
			{
				component.Dispose();
			}

			Components.Clear();

			entityToIndex.Dispose();
			entities.Dispose();
			Components.Dispose();
		}

		public bool Equals(RevolutionChunk other)
		{
			return Equals(ComponentTypes, other.ComponentTypes) && Equals(entities, other.entities) && Equals(Components, other.Components);
		}

		/// <summary>
		///     Remove an entity on this chunk.
		/// </summary>
		/// <param name="entity">The entity to remove</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void RemoveEntity(RawEntity entity)
		{
			var idx = IndexOf(entity);
			foreach (var component in Components.Values)
				component.RemoveAt(idx);
			
			entities.RemoveAt(idx);
			// swapping back the index of the next entity to the previous index
			if (idx < entities.Count)
			{
				entityToIndex.Remove(entity);
				entityToIndex[entities[idx]] = idx;
				Parallel.For(idx, entities.Count, updateEntityIndex);
			}
		}

		/// <summary>
		///     Add an entity to this chunk
		/// </summary>
		/// <param name="entity">The entity</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddEntity(RawEntity entity)
		{
			entityToIndex[entity] = entities.Count;

			entities.Add(entity);
			foreach (var component in Components.Values)
				component.Add(entity);
		}

		/// <summary>
		///     Get the index of an entity on this chunk.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(RawEntity entity)
		{
			//return entities.IndexOf(entity);
			return entityToIndex[entity];
		}

		/// <summary>
		/// Get a component array of type <see cref="T"/>.
		/// </summary>
		/// <typeparam name="T">Component to get</typeparam>
		/// <returns></returns>
		public Span<T> GetComponents<T>()
		{
			return Components[typeof(T)].GetSpan<T>();
		}

		public override bool Equals(object obj)
		{
			return obj is RevolutionChunk other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ComponentTypes, entities, Components);
		}
	}
}