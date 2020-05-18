using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
		public readonly Dictionary<Type, ComponentArray> Components;

		/// <summary>
		///     Get the component types of this chunk
		/// </summary>
		public readonly Type[] ComponentTypes;

		private readonly List<RawEntity>            entities;
		private readonly Dictionary<RawEntity, int> entityToIndex;
		private          RawEntity[]                cachedArray;
		private          bool                       cacheIsDirty;

		public RevolutionChunk(IEnumerable<Type> components)
		{
			cachedArray = new RawEntity[0];

			ComponentTypes = components.ToArray();
			entities       = new List<RawEntity>();
			Components     = new Dictionary<Type, ComponentArray>(ComponentTypes.Length);
			foreach (var type in ComponentTypes) Components[type] = new ComponentArray(type);

			entityToIndex = new Dictionary<RawEntity, int>();
		}

		/// <summary>
		///     Get the array of entities. If an entity was added or removed, the array will be recomputed.
		/// </summary>
		public RawEntity[] AsArray
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!cacheIsDirty)
					return cachedArray;

				cacheIsDirty = false;
				return cachedArray = entities.ToArray();
			}
		}

		/// <summary>
		/// Dispose and clear component data
		/// </summary>
		public void Dispose()
		{
			Array.Clear(ComponentTypes, 0, ComponentTypes.Length);
			entities.Clear();
			Components.Clear();
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
			cacheIsDirty = true;
			foreach (var component in Components.Values)
				component.RemoveAt(entities.IndexOf(entity));
			entities.Remove(entity);

			entityToIndex.Clear();

			var i                                        = 0;
			foreach (var e in entities) entityToIndex[e] = i++;
		}

		/// <summary>
		///     Add an entity to this chunk
		/// </summary>
		/// <param name="entity">The entity</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AddEntity(RawEntity entity)
		{
			cacheIsDirty = true;

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
			return entityToIndex[entity];
		}

		/// <summary>
		/// Get a component array of type <see cref="T"/>.
		/// </summary>
		/// <typeparam name="T">Component to get</typeparam>
		/// <returns></returns>
		public Span<T> GetComponents<T>()
		{
			return Components[typeof(T)].GetArray<T>().AsSpan();
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