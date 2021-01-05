using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Morestachio.Formatter.Framework
{
	/// <inheritdoc />
	public class MultiFormatterInfoCollection : IReadOnlyList<MultiFormatterInfo>
	{
		private readonly IReadOnlyList<MultiFormatterInfo> _source;
		
		/// <inheritdoc />
		public MultiFormatterInfoCollection(IEnumerable<MultiFormatterInfo> source)
		{
			_source = source.ToArray();
			SourceObject = source.FirstOrDefault(e => e.IsSourceObject);
			ParamsArgument = source.FirstOrDefault(e => e.IsRestObject);
			NonParamsArguments = this.Except(new[] {ParamsArgument}).ToArray();
			MandetoryArguments = this.Where(e => !e.IsRestObject && !e.IsOptional && !e.IsSourceObject && !e.IsInjected).ToArray();
		}

		/// <inheritdoc />
		public IEnumerator<MultiFormatterInfo> GetEnumerator()
		{
			return _source.GetEnumerator();
		}
		
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _source.GetEnumerator();
		}
		
		/// <inheritdoc />
		public int Count
		{
			get { return _source.Count; }
		}

		/// <inheritdoc />
		public MultiFormatterInfo this[int index]
		{
			get { return _source[index]; }
		}

		/// <summary>
		///		Sets the name of an Parameter.
		/// </summary>
		/// <returns></returns>
		public MultiFormatterInfoCollection SetName(string parameterName, string templateParameterName)
		{
			var multiFormatterInfo = this.FirstOrDefault(e => e.Name.Equals(parameterName));
			if (multiFormatterInfo == null)
			{
				return this;
			}

			multiFormatterInfo.Name = templateParameterName;
			return this;
		}

		/// <summary>
		///		The Source argument
		/// </summary>
		
		public MultiFormatterInfo SourceObject { get; private set; }

		/// <summary>
		///		The Params argument
		/// </summary>
		 
		public MultiFormatterInfo ParamsArgument { get; private set; }

		/// <summary>
		///		All arguments that are not an params argument
		/// </summary>
		public IReadOnlyList<MultiFormatterInfo> NonParamsArguments { get; private set; }

		/// <summary>
		///		All not-default arguments
		/// </summary>
		public IReadOnlyList<MultiFormatterInfo> MandetoryArguments { get; private set; }

		/// <summary>
		///		When called and the last parameter is an object array, it will be used as an params parameter.
		///		This is quite helpful as you cannot annotate Lambdas.
		/// </summary>
		/// <returns></returns>
		public MultiFormatterInfoCollection LastIsParams()
		{
			if (ParamsArgument != null)
			{
				return this;
			}

			ParamsArgument = this.LastOrDefault();
			
			if (ParamsArgument == null)
			{
				return this;
			}

			if (ParamsArgument.ParameterType == typeof(object[]))
			{
				ParamsArgument.IsRestObject = true;
			}

			NonParamsArguments = this.Except(new[] {ParamsArgument}).ToArray();
			return this;
		}
	}
}