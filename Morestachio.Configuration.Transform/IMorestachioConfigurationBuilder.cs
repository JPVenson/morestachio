using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	/// <summary>
	///		A morestachio config builder
	/// </summary>
	public interface IMorestachioConfigurationBuilder : IConfigurationBuilder
	{
		MorestachioConfigOptions Options { get; set; }
	}
}