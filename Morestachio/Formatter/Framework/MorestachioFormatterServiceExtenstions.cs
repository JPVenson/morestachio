using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Add Extensions for easy runtime added Functions
	/// </summary>
	[PublicAPI]
	public static class MorestachioFormatterServiceExtensions
	{
		#region Action Overloads

		public static MultiFormatterInfoCollection AddSingle(this MorestachioFormatterService service, Action function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T>(this MorestachioFormatterService service, Action<T> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T, T1>(this MorestachioFormatterService service, Action<T, T1> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		#endregion

		#region Function Overloads

		public static MultiFormatterInfoCollection AddSingle<T>(this MorestachioFormatterService service, Func<T> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T, T1>(this MorestachioFormatterService service, Func<T, T1> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T, T1, T2>(this MorestachioFormatterService service, Func<T, T1, T2> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T, T1, T2, T3>(this MorestachioFormatterService service, Func<T, T1, T2, T3> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		public static MultiFormatterInfoCollection AddSingle<T, T1, T2, T3, T4>(this MorestachioFormatterService service, Func<T, T1, T2, T3, T4> function, [CanBeNull]string name = null)
		{
			return service.AddSingle((Delegate)function, name);
		}

		#endregion
	}
}
