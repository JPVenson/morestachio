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
		/// <summary>
		///		Gets if this object is sealed
		/// </summary>
		bool IsSealed { get; }

		/// <summary>
		///		Seals this object
		/// </summary>
		void Seal();
	}
}
