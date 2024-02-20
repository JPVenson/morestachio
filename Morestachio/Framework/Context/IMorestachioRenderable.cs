using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Morestachio.Framework.Context
{
	/// <summary>
	///		Will use the <see cref="IMorestachioRender.RenderToString"/> method to represent this object in a template if rendering is requested.
	/// </summary>
	public interface IMorestachioRender
	{
#if Span
		/// <summary>
		///		Renders this object for display in a Template.
		/// </summary>
		/// <returns></returns>
		ReadOnlySpan<char> RenderToString();
#else
		/// <summary>
		///		Renders this object for display in a Template.
		/// </summary>
		/// <returns></returns>
		string RenderToString();
#endif
	}

	/// <summary>
	///		Will use the <see cref="IMorestachioRenderAsync.RenderToString"/> method to represent this object in a template if rendering is requested.
	/// </summary>
	public interface IMorestachioRenderAsync
	{
		/// <summary>
		///		Renders this object for display in a Template.
		/// </summary>
		/// <returns></returns>
		StringPromise RenderToString();
	}
}