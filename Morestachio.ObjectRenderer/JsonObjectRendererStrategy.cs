using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;

namespace Morestachio.ObjectRenderer
{
	public class JsonObjectRendererStrategy : IObjectRendererStrategy
	{
		private readonly IStringBuilderInterlaced<NoColor> _stream;

		public JsonObjectRendererStrategy(IStringBuilderInterlaced<NoColor> stream)
		{
			_stream = stream;
		}

		public async Task BeginRenderingAsync()
		{
			_stream.AppendLine("{").Up();
			await Task.CompletedTask;
		}

		public async Task BeginRenderTypeAsync(Type type, IEnumerable<MorestachioFormatterModel> formatter)
		{
			_stream.AppendInterlacedLine($"interface {type.Name} {{").Up();
			await Task.CompletedTask;
		}

		public async Task BeginRenderPropertiesAsync(PropertyInfo[] property)
		{
			await Task.CompletedTask;
		}

		public async Task BeginRenderPropertyAsync(PropertyInfo property, Type extractedType)
		{
			_stream.AppendInterlacedLine($"{property.Name}: {extractedType.Name};").Up();
			await Task.CompletedTask;
		}

		public async Task EndRenderPropertyAsync(PropertyInfo property, Type extractedType)
		{
			await Task.CompletedTask;
		}

		public async Task EndRenderPropertiesAsync(PropertyInfo[] property)
		{
			await Task.CompletedTask;
		}

		public async Task EndRenderTypeAsync(Type type, IEnumerable<MorestachioFormatterModel> formatter)
		{
			foreach (var morestachioFormatterModel in formatter)
			{
				_stream.AppendInterlacedLine(
					$"{morestachioFormatterModel.Name}(" + 
					$"{morestachioFormatterModel.Function.GetParameters().Select(e => $"{e.Name}: {e.ParameterType}").Aggregate((e, f) => e + ", " + f)}):" +
					$"{morestachioFormatterModel.Function.ReturnType}");
			}

			_stream.Down().AppendInterlacedLine($"}}").AppendInterlacedLine("");
			await Task.CompletedTask;
		}

		public async Task EndRenderingAsync()
		{
			_stream.AppendLine().Down().AppendLine("}");
			await Task.CompletedTask;
		}
	}
}