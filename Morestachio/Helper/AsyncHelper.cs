using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
using StringPromise = System.Threading.Tasks.ValueTask<string>;
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
using BoolPromise = System.Threading.Tasks.ValueTask<bool>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
using StringPromise = System.Threading.Tasks.Task<string>;
using ObjectPromise = System.Threading.Tasks.Task<object>;
using BoolPromise = System.Threading.Tasks.Task<bool>;
#endif

namespace Morestachio.Helper
{
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

		/// <summary>
		///		Wraps the object to ether an TaskT or an ValueTaskT
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Promise FakePromise()
		{
#if ValueTask
			await new ValueTask();
#else
			await Task.CompletedTask;
#endif
		}

		//		/// <summary>
		//		///		Wraps the object to ether an TaskT or an ValueTaskT
		//		/// </summary>
		//		/// <param name="data"></param>
		//		/// <returns></returns>
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static ContextObjectPromise ToPromise(this ContextObject data)
		//		{
		//#if ValueTask
		//			return new ContextObjectPromise(data);
		//#else
		//			return Promise.FromResult(data);
		//#endif
		//		}

		//		/// <summary>
		//		///		Wraps the object to ether an TaskT or an ValueTaskT
		//		/// </summary>
		//		/// <param name="data"></param>
		//		/// <returns></returns>
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static StringPromise ToPromise(this string data)
		//		{
		//#if ValueTask
		//			return new StringPromise(data);
		//#else
		//			return Promise.FromResult(data);
		//#endif
		//		}

		//		/// <summary>
		//		///		Wraps the object to ether an TaskT or an ValueTaskT
		//		/// </summary>
		//		/// <param name="data"></param>
		//		/// <returns></returns>
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static ObjectPromise ToPromise(this object data)
		//		{
		//#if ValueTask
		//			return new ObjectPromise(data);
		//#else
		//			return Task.FromResult(data);
		//#endif
		//		}


		/// <summary>
		///		Wraps the object to ether an TaskT or an ValueTaskT
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ValueTask
		public static ValueTask<T> ToPromise<T>(this T data)
#else
		public static Task<T> ToPromise<T>(this T data)
#endif
		{
#if ValueTask
			return new ValueTask<T>(data);
#else
			return Task.FromResult(data);
#endif

		}

		/// <summary>
		///		Wraps the object to ether an TaskT or an ValueTaskT
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if ValueTask
		public static ValueTask<T> EmptyPromise<T>() where T : class
#else
		public static Task<T> EmptyPromise<T>()
#endif
		{
#if ValueTask
			return new ValueTask<T>((T)null);
#else
			return Task.FromResult<T>(default);
#endif

		}

		//		/// <summary>
		//		///		Wraps the object to ether an TaskT or an ValueTaskT
		//		/// </summary>
		//		/// <param name="data"></param>
		//		/// <returns></returns>
		//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//		public static BoolPromise ToPromise(this bool data)
		//		{
		//#if ValueTask
		//			return new BoolPromise(data);
		//#else
		//			return Task.FromResult(data);
		//#endif
		//		}


		/// <summary>
		///     Unpacks the task.
		/// </summary>
		/// <returns></returns>
		public static async ObjectPromise UnpackFormatterTask(this object maybeTask)
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

#if ValueTask
			if (maybeTask is ValueTask valTask)
			{
				await valTask;
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
}