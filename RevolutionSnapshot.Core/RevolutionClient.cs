using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevolutionSnapshot.Core
{
	public class RevolutionClient : IDisposable
	{
		private readonly RevolutionSerializer   serializer;
		private readonly List<RevolutionClient> linkedList;

		protected Task   PrepareTask;
		protected Task   RunningTask;
		protected byte[] WrittenData;
		protected int    DataLength;

		public RevolutionClient(RevolutionSerializer serializer)
		{
			this.serializer = serializer;
			this.linkedList = serializer.Clients;

			PrepareTask = new Task(OnTaskRun);
			RunningTask = new Task(OnTaskRun);
		}

		protected virtual void OnPrepare()
		{
			Array.Clear(WrittenData, 0, WrittenData.Length);
			DataLength = 0;
		}

		protected virtual void OnTaskRun()
		{
			if (DataLength != 0)
				throw new Exception("DataLength should be at 0");
		}

		public void Dispose()
		{
			if (!linkedList.Remove(this))
				throw new Exception("123");
		}
	}
}