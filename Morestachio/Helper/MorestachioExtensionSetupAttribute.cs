using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Helper
{
	/// <summary>
	///		Contains setup instructions for the used service
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class MorestachioExtensionSetupAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="description"></param>
		public MorestachioExtensionSetupAttribute(string description)
		{
			Description = description;
		}

		/// <summary>
		///		
		/// </summary>
		public string Description { get; set; }
	}
}
