using Microsoft.Extensions.Configuration;

namespace Morestachio.Configuration.Transform
{
	public class MorestachioConfigSection : MorestachioConfig, IConfigurationSection
	{
		private readonly IConfigurationSection _section;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="section"></param>
		/// <param name="options"></param>
		public MorestachioConfigSection(IConfigurationSection section, MorestachioConfigOptions options) : base(section, options)
		{
			_section = section;
		}

		/// <inheritdoc />
		public string Key
		{
			get { return _section.Key; }
		}

		/// <inheritdoc />
		public string Path
		{
			get { return _section.Path; }
		}

		/// <inheritdoc />
		public string Value
		{
			get { return CheckAndTransformValue(Key, _section.Value, Options).Value; }
			set { _section.Value = value; }
		}
	}
}