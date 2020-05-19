using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RevolutionSnapshot.Core.ECS
{
	/// <summary>
	///     Revolution is almost like an ECS framework, except it is not... (no way to filter entities, no entity version, ...)
	/// </summary>
	public partial class RevolutionWorld : IDisposable
	{
		/// <summary>
		///     Get all the chunks of this world.
		/// </summary>
		public RevolutionChunk[] Chunks;

		private Dictionary<RawEntity, RevolutionChunk> entityToChunk;

		// TODO: We need to separate this variable into another class
		private readonly TwoWayDictionary<EntityIdentifier, RawEntity> identifierToEntity;
		private          RawEntity                               lastEntity;

		public RevolutionWorld(int capacity = 0)
		{
			lastEntity = default;

			Chunks = new RevolutionChunk[1];
			// the first chunk has no component
			Chunks[0] = new RevolutionChunk(new Type[0]);

			entityToChunk      = new Dictionary<RawEntity, RevolutionChunk>(1);
			identifierToEntity = new TwoWayDictionary<EntityIdentifier, RawEntity>();
		}

		protected RevolutionChunk EmptyComponentChunk => Chunks[0];

		/// <summary>
		///     Dispose and clear all chunks of this world.
		/// </summary>
		public void Dispose()
		{
			foreach (var chunk in Chunks)
				chunk.Dispose();

			Array.Clear(Chunks, 0, Chunks.Length);

			Chunks        = null;
			entityToChunk = null;
		}

		/// <summary>
		///     Directly create an entity into a chunk
		/// </summary>
		/// <param name="chunk">The chunk to use</param>
		/// <returns>The new entity</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RevolutionEntity CreateEntityInChunk(RevolutionChunk chunk)
		{
			lastEntity = new RawEntity(lastEntity.Id + 1);
			chunk.AddEntity(lastEntity);
			entityToChunk[lastEntity] = chunk;

			return new RevolutionEntity(this, lastEntity);
		}

		/// <summary>
		///     Create an entity
		/// </summary>
		/// <returns>The new entity</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RevolutionEntity CreateEntity()
		{
			return CreateEntityInChunk(EmptyComponentChunk);
		}

		/// <summary>
		///     Create an entity with an identifier
		/// </summary>
		/// <param name="identity"></param>
		/// <typeparam name="TIdentity"></typeparam>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RevolutionEntity CreateIdentEntityInChunk<TIdentity>(TIdentity identity, RevolutionChunk chunk)
		{
			var ent = CreateEntityInChunk(chunk);
			var id  = new EntityIdentifier();
			id.Set(identity);
			identifierToEntity.Set(id, ent.Raw);

			return ent;
		}

		/// <summary>
		///     Create an entity with an identifier
		/// </summary>
		/// <param name="identity"></param>
		/// <typeparam name="TIdentity"></typeparam>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RevolutionEntity CreateIdentEntity<TIdentity>(TIdentity identity)
		{
			return CreateIdentEntityInChunk(identity, EmptyComponentChunk);
		}

		/// <summary>
		///     Get an entity from an identifier
		/// </summary>
		/// <param name="identity"></param>
		/// <typeparam name="TIdentity"></typeparam>
		/// <returns></returns>
		public RevolutionEntity GetEntityFromIdentifier<TIdentity>(TIdentity identity)
		{
			var id = new EntityIdentifier();
			id.Set(identity);
			if (identifierToEntity.TryGetValue(id, out var entity))
				return new RevolutionEntity(this, entity);
			return default;
		}

		/// <summary>
		///     Remove an entity
		/// </summary>
		/// <param name="entity"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveEntity(RawEntity entity)
		{
			entityToChunk[entity].RemoveEntity(entity);
			entityToChunk.Remove(entity);
		}

		public T GetIdentifier<T>(RawEntity entity)
		{
			return identifierToEntity.GetKey(entity).Get<T>();
		}

		public bool TryGetIdentifier<T>(RawEntity entity, out T value)
		{
			if (identifierToEntity.TryGetKey(entity, out var identifier))
			{
				value = identifier.Get<T>();
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		///     Set the component of an entity
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="value"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T SetComponent<T>(RawEntity entity, in T value)
			where T : IRevolutionComponent
		{
			Type[] componentList = {typeof(T)};
			if (entityToChunk.TryGetValue(entity, out var previousChunk))
			{
				componentList = componentList.Union(previousChunk.ComponentTypes).ToArray();

				// If we are still on the same chunk, don't change anything
				if (HasSameComponents(previousChunk, componentList))
				{
					ref var reference = ref GetComponent<T>(entity);
					reference = value;
					return ref reference;
				}
			}

			var currentChunk = GetOrCreateChunk(componentList);
			currentChunk.AddEntity(entity);
			entityToChunk[entity] = currentChunk;

			if (previousChunk != null)
			{
				CopyComponents(entity, previousChunk, currentChunk);
				previousChunk.RemoveEntity(entity);
			}

			ref var newReference = ref GetComponent<T>(entity);
			newReference = value;

			return ref newReference;
		}

		/// <summary>
		///     Get the component of an entity
		/// </summary>
		/// <param name="entity"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetComponent<T>(RawEntity entity)
			where T : IRevolutionComponent
		{
			var chunk = entityToChunk[entity];
			return ref chunk.Components[typeof(T)].GetSpan<T>()[chunk.IndexOf(entity)];
		}
		
		/// <summary>
		/// Remove a component from an entity
		/// </summary>
		/// <param name="entity">The entity</param>
		/// <param name="type">The component to remove</param>
		/// <returns>Return true if the component was removed</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveComponent(RawEntity entity, Type type)
		{
			var previousChunk = entityToChunk[entity];
			var componentList = previousChunk.ComponentTypes.Except(new[] {type})
			                                 .ToArray();
			if (componentList.Length == previousChunk.ComponentTypes.Length)
				return false;

			MoveToChunk(entity, GetOrCreateChunk(componentList.ToArray()));
			return true;
		}

		/// <summary>
		/// Remove a component from an entity
		/// </summary>
		/// <param name="entity">The entity</param>
		/// <typeparam name="T">The component to remove</typeparam>
		/// <returns>Return true if the component was removed</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveComponent<T>(RawEntity entity)
			where T : IRevolutionComponent =>
			RemoveComponent(entity, typeof(T));

		/// <summary>
		///     Get the chunk of an entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public RevolutionChunk GetChunk(RawEntity entity)
		{
			return entityToChunk[entity];
		}

		public bool Exists(RawEntity entity) => entityToChunk.ContainsKey(entity);
	}
}