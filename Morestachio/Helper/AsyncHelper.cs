using System.Threading.Tasks;

namespace Morestachio.Helper
{
	/// <summary>
	/// A Helper class to run Asynchronous functions from synchronous ones
	/// </summary>
	public static class AsyncHelper
	{
		/// <summary>
		///     Unpacks the task.
		/// </summary>
		/// <returns></returns>
		public static async Task<object> UnpackFormatterTask(this object maybeTask)
		{
			if (maybeTask is Task task)
			{
				await task;

				if (task is Task<object> task2)
				{
					return task2.Result;
				}

				var taskType = task.GetType();
				if (taskType != typeof(Task))
				{
					return typeof(Task<>)
						.MakeGenericType(taskType.GenericTypeArguments[0])//this must be done for an strange behavior with async's calls in .net core
						.GetProperty(nameof(Task<object>.Result))
						.GetValue(task);
				}

				return maybeTask;
			}

			return maybeTask;
		}
	}
}