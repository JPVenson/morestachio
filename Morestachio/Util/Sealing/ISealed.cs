using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Util.Sealing
{
	/// <summary>
	///		Defines an object that can be sealed and no longer be modified
	/// </summary>
	public interface ISealed
	{
		bool IsSealed { get; }

		void Seal();
	}
}
