using System.Threading.Tasks;
using Morestachio.Document.Contracts;

namespace Morestachio.Helper;

/// <summary>
/// A Helper class to run Asynchronous functions from synchronous ones
/// </summary>
public static class AsyncHelper
{
	/// <summary>
	///		Wraps the object to ether an TaskT or an ValueTaskT
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ItemExecutionPromise ToPromise(this IEnumerable<DocumentItemExecution> data)
	{
#if ValueTask
		return new ItemExecutionPromise(data);
#else
			return Promise.FromResult(data);
#endif
	}

#if ValueTask
	private static readonly ValueTask _done = new ValueTask();
#endif

	/// <summary>
	///		Wraps the object to ether an TaskT or an ValueTaskT
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Promise FakePromise()
	{
#if ValueTask
		return _done;
#else
			return Task.CompletedTask;
#endif
	}
#if ValueTask
	/// <summary>
	///		Wraps the object to an
	/// <see cref="ValueTask{T}"/>
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueTask<T> ToPromise<T>(this T data)
	{
		return new ValueTask<T>(data);
	}
#else
		/// <summary>
		///		Wraps the object to an
		///<see cref="Task{T}"/>
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<T> ToPromise<T>(this T data)
		{
			return Task.FromResult(data);
		}
#endif
	/// <summary>
	///		Wraps the object to ether an TaskT or an ValueTaskT
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ValueTask
	public static ValueTask<T> EmptyPromise<T>() where T : class
	{
		return new ValueTask<T>((T)null);
	}
#else
		public static Task<T> EmptyPromise<T>()
		{
			return Task.FromResult<T>(default);
		}
#endif
	/// <summary>
	///     Unpacks the task.
	/// </summary>
	/// <returns></returns>
	public static async ObjectPromise UnpackFormatterTask(this object maybeTask)
	{
		if (maybeTask is Task task)
		{
			if (!task.IsCompleted)
			{
				await task;
			}

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

#if ValueTask
		if (maybeTask is ValueTask valTask)
		{
			if (!valTask.IsCompleted)
			{
				await valTask;
			}
			var taskType = valTask.GetType();
			if (taskType != typeof(ValueTask))
			{
				return typeof(ValueTask<>)
					.MakeGenericType(taskType.GenericTypeArguments[0])//this must be done for an strange behavior with async's calls in .net core
					.GetProperty(nameof(ValueTask<object>.Result))
					.GetValue(valTask);
			}
		}
		if (maybeTask is ValueTask<object> objValTask)
		{
			return await objValTask;
		}
#endif

		return maybeTask;
	}
}