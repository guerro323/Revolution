using System;
using System.Diagnostics;

namespace RevolutionSnapshot.Core.ECS
{
	/// <summary>
	///     A basic version of a revolution entity.
	/// </summary>
	/// <remarks>
	///     For safety, encapsulate a <see cref="RawEntity" /> into an <see cref="RevolutionEntity" />
	/// </remarks>
	public readonly struct RawEntity : IEquatable<RawEntity>
	{
		public readonly uint Id;

		public RawEntity(uint id)
		{
			Id = id;
		}

		public bool Equals(RawEntity other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is RawEntity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (int) Id;
		}

		public override string ToString()
		{
			return $"R-Entity({Id})";
		}
	}

	/// <summary>
	///     A safe entity visualization
	/// </summary>
	[DebuggerDisplay("Entity(Id={Raw.Id})")]
	public readonly struct RevolutionEntity
	{
		public readonly RevolutionWorld World;
		public readonly RawEntity       Raw;

		public bool            IsAlive => World.Exists(Raw);
		public RevolutionChunk Chunk   => World.GetChunk(Raw);

		public RevolutionEntity(RevolutionWorld world, RawEntity raw)
		{
			World = world;
			Raw   = raw;
		}

		public override string ToString()
		{
			return $"Entity(WorldHash={World.GetHashCode()}, Id={Raw.Id})";
		}

		public ref T SetComponent<T>(in T value)
			where T : IRevolutionComponent
		{
			return ref World.SetComponent(Raw, value);
		}

		public ref T GetComponent<T>()
			where T : IRevolutionComponent
		{
			return ref World.GetComponent<T>(Raw);
		}

		public bool RemoveComponent<T>()
			where T : IRevolutionComponent
		{
			return World.RemoveComponent<T>(Raw);
		}
	}

	/// <summary>
	///     The identifier of an entity (used for when you need to identify an entity (eg: an entity from another ECS world))
	/// </summary>
	public struct EntityIdentifier : IEquatable<EntityIdentifier>
	{
		private abstract class Wrapped : IEquatable<Wrapped>
		{
			public abstract bool Equals(Wrapped other);

			public override bool Equals(object obj)
			{
				return Equals((Wrapped) obj);
			}

			public override int GetHashCode()
			{
				throw new Exception("should be overriden");
			}
		}

		private class Wrapped<T> : Wrapped
		{
			public readonly T Value;

			public Wrapped(T value)
			{
				Value = value;
			}

			public override bool Equals(Wrapped other)
			{
				if (other is Wrapped<T> otherSame)
					return otherSame.Value.Equals(Value);
				return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Value);
			}
		}

		private Wrapped wrapped;

		/// <summary>
		///     Set the identity
		/// </summary>
		/// <param name="val"></param>
		/// <typeparam name="T"></typeparam>
		public void Set<T>(T val)
		{
			wrapped = new Wrapped<T>(val);
		}

		/// <summary>
		/// Get the identity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Get<T>()
		{
			return ((Wrapped<T>) wrapped).Value;
		}

		public bool Equals(EntityIdentifier other)
		{
			return Equals(wrapped, other.wrapped);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return Equals((EntityIdentifier) obj);
		}

		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyMemberInGetHashCode
			return wrapped != null ? wrapped.GetHashCode() : 0;
			// ReSharper restore NonReadonlyMemberInGetHashCode
		}
	}
}