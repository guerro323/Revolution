using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Collections.Pooled;

namespace RevolutionSnapshot.Core.ECS
{
	/// <summary>
	///     A component array contains the components of an entity chunk (<see cref="RevolutionChunk" />)
	/// </summary>
	public struct ComponentArray : IDisposable
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
				throw new InvalidOperationException("not the same type");
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
		public Span<T> GetSpan<T>()
		{
			return ((Wrapped<T>) wrapped).Span;
		}

		private abstract class Wrapped : IDisposable
		{
			public abstract int GetLength();

			public abstract void Add();
			public abstract void RemoveAt(int   index);
			public abstract void CopyTo(int     source, Wrapped other, int destination);
			public abstract void CopyTo(Wrapped other);

			public abstract void Dispose();
		}

		private class Wrapped<T> : Wrapped
		{
			private readonly PooledList<T> values = new PooledList<T>();

			public Span<T> Span
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => values.Span;
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
				var genOther = (Wrapped<T>) other;
				RevolutionType<T>.Copy(values.Span.Slice(source, 1), genOther.values.Span.Slice(destination, 1));
			}

			public override void CopyTo(Wrapped other)
			{
				var genOther = (Wrapped<T>) other;
				genOther.values.Clear();
				RevolutionType<T>.Copy(values.Span, genOther.values.AddSpan(values.Count));
			}

			public override void Dispose()
			{
				values.Dispose();
			}
		}

		public void Dispose()
		{
			wrapped?.Dispose();
		}
	}
}