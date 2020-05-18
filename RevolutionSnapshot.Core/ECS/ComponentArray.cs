using System;
using System.Runtime.CompilerServices;

namespace RevolutionSnapshot.Core.ECS
{
	/// <summary>
	///     A component array contains the components of an entity chunk (<see cref="RevolutionChunk" />)
	/// </summary>
	public class ComponentArray
	{
		public readonly Type Type;

		private readonly Wrapped wrapped;

		/// <summary>
		///     Initialize a component array from a type
		/// </summary>
		/// <param name="type"></param>
		public ComponentArray(Type type)
		{
			Type = type;

			wrapped = (Wrapped) Activator.CreateInstance(typeof(Wrapped<>).MakeGenericType(type));
		}

		/// <summary>
		///     Length of the wrapped array.
		/// </summary>
		public int Length => wrapped.GetLength();

		/// <summary>
		///     Add an entity to the array
		/// </summary>
		/// <param name="entity"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(RawEntity entity)
		{
			wrapped.Add();
		}

		/// <summary>
		///     Remove a component at an index
		/// </summary>
		/// <param name="index"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			wrapped.RemoveAt(index);
		}

		/// <summary>
		///     Copy a component value to another component array
		/// </summary>
		/// <param name="source">The source index from our array</param>
		/// <param name="other">The other array where values will be copied to</param>
		/// <param name="destination">The destination index of the other array</param>
		public void CopyTo(int source, ComponentArray other, int destination)
		{
			if (other.Type != Type)
				throw new InvalidOperationException("not the same type");
			wrapped.CopyTo(source, other.wrapped, destination);
		}

		/// <summary>
		///     Copy the values to another array (if the length match)
		/// </summary>
		/// <param name="other"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void CopyTo(ComponentArray other)
		{
			if (other.Type != Type)
				throw new InvalidOperationException("not the same time");
			if (other.Length != Length)
				throw new InvalidOperationException("the length does not match");
			wrapped.CopyTo(other.wrapped);
		}

		/// <summary>
		///     Get the readable array. (Will throw if <see cref="T" /> was incorrect)
		/// </summary>
		/// <typeparam name="T">The type of the component</typeparam>
		/// <returns>Component array of <see cref="T" /></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] GetArray<T>()
		{
			return ((Wrapped<T>) wrapped).AsArray;
		}

		private abstract class Wrapped
		{
			public abstract int GetLength();

			public abstract void Add();
			public abstract void RemoveAt(int   index);
			public abstract void CopyTo(int     source, Wrapped other, int destination);
			public abstract void CopyTo(Wrapped other);
		}

		private class Wrapped<T> : Wrapped
		{
			private readonly ArrayList<T> values = new ArrayList<T>();

			public T[] AsArray
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => values._items;
			}

			public override int GetLength()
			{
				return values.Count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void Add()
			{
				values.Add(default);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void RemoveAt(int index)
			{
				values.RemoveAt(index);
			}

			public override void CopyTo(int source, Wrapped other, int destination)
			{
				((Wrapped<T>) other).values[destination] = values[source];
			}

			public override void CopyTo(Wrapped other)
			{
				var genOther = (Wrapped<T>) other;
				values._items.CopyTo(genOther.values._items.AsMemory());
			}
		}
	}
}