using System;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MessagePack;
using NUnit.Framework;
using RevolutionSnapshot.Core;
using RevolutionSnapshot.Core.ECS;

namespace RevolutionSnapshot.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			using var serializer = new RevolutionSerializer();
			using var client     = serializer.CreateClient();
			//using var referSynchronizer = serializer.ReferSynchronizer<BasicSynchronize<Component1>>();

			var ent = serializer.World.CreateEntity();
			ent.SetComponent(new Component1());

			serializer.SerializeForAll();
		}

		[Test]
		public void TestWorldClone()
		{
			using var worldSource = new RevolutionWorld();
			var       entSource   = worldSource.CreateEntity();
			var       compSource  = entSource.SetComponent(new Component1 {Value = 42});

			using var clone = worldSource.Clone();
			// Since it is a clone, the entity indexes are the sames.
			Assert.AreEqual(compSource.Value, clone.GetComponent<Component1>(entSource.Raw).Value);
		}

		[Test]
		public void TestMillionClone()
		{
			using var worldSource = new RevolutionWorld();
			var       sw          = new Stopwatch();
			sw.Restart();
			for (var i = 0; i != 1_000_000; i++)
			{
				worldSource.CreateEntity();
			}

			sw.Stop();
			Console.WriteLine($"Time to create entities in source: {sw.Elapsed.TotalMilliseconds}ms");

			sw.Restart();
			using var clone = worldSource.Clone();
			sw.Stop();
			Console.WriteLine($"Time to clone: {sw.Elapsed.TotalMilliseconds}ms");

			Assert.AreEqual(worldSource.Chunks.Length, clone.Chunks.Length);
			Assert.AreEqual(worldSource.Chunks[0].AsArray.Length, clone.Chunks[0].AsArray.Length);
		}

		[Test]
		public void TestIdentifier()
		{
			using var world = new RevolutionWorld();
			const int id    = 8;

			// without ID
			var entityWithoutIdentifier = world.CreateEntity();
			Assert.AreEqual(default(RevolutionEntity), world.GetEntityFromIdentifier(id));

			var entityWithIdentifier = world.CreateIdentEntity(id);
			Assert.AreNotEqual(entityWithoutIdentifier, world.GetEntityFromIdentifier(id));
			Assert.AreEqual(entityWithIdentifier, world.GetEntityFromIdentifier(id));
		}

		[Test]
		public void TestComponent()
		{
			using var world  = new RevolutionWorld();
			var       entity = world.CreateEntity();

			ref var hold = ref world.SetComponent(entity.Raw, new Component1 {Value = 42});

			hold.Value = 43;
			Assert.AreEqual(world.GetComponent<Component1>(entity.Raw).Value, 43);

			world.SetComponent(entity.Raw, new Component1 {Value = 41});
			Assert.AreEqual(world.GetComponent<Component1>(entity.Raw).Value, 41);
			Assert.AreEqual(hold.Value, 41);
			
			Assert.IsTrue(world.RemoveComponent<Component1>(entity.Raw));
		}

		struct Component1 : ISnapshotComponent<Component1>
		{
			public int Value;

			public void Interpolate(in Component1 nextState, float factor)
			{
				Value = (int) MathHelper.Lerp(Value, nextState.Value, factor);
			}
		}
	}
}