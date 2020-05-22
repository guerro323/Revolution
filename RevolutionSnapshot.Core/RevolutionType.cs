using System;

namespace RevolutionSnapshot.Core
{
	internal static class RtClone<T1>
		where T1 : ICloneable<T1>
	{
		public static Func<T1, T1> Action;

		static RtClone()
		{
			Action = t => t.Clone();
		}
	}
	
	public static class RevolutionType<T>
	{
		public static bool IsCloneable  { get; }
		public static bool IsDisposable { get; }

		private readonly static Func<T, T> CloneAction;

		static RevolutionType()
		{
			IsCloneable  = typeof(ICloneable<T>).IsAssignableFrom(typeof(T));
			IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T));

			if (IsCloneable)
			{
				CloneAction = (Func<T, T>) typeof(RtClone<>).MakeGenericType(typeof(T)).GetField("Action").GetValue(null);
			}
		}

		public static void Copy(Span<T> from, Span<T> to)
		{
			if (from.Length != to.Length)
				throw new InvalidOperationException("not same length");

			if (!IsCloneable)
			{
				from.CopyTo(to);
				return;
			}
			
			var length = from.Length;
			for (var i = 0; i != length; i++)
			{
				to[i] = CloneAction(from[i]);
			}
		}
	}
}