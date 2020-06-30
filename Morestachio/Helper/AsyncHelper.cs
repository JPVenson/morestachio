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

				if (task.GetType() != typeof(Task))
				{
					return typeof(Task<>)
						.MakeGenericType(task.GetType().GenericTypeArguments)
						.GetProperty(nameof(Task<object>.Result))
						.GetValue(task);
				}

				return maybeTask;
			}

			return maybeTask;
		}
	}
}