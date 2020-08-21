using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Contains the translations for multiple cultures
	/// </summary>
	public class MorestachioLocalizationService : IMorestachioLocalizationService
	{
		/// <summary>
		/// 
		/// </summary>
		public MorestachioLocalizationService()
		{
			TextCache = new Dictionary<string, TextResourceEntity[]>();
		}

		/// <summary>
		///		The result of the Load method call
		/// </summary>
		public Dictionary<string, TextResourceEntity[]> TextCache { get; set; }

		static readonly Regex RefRegex = new Regex(@"([{][^\d}]+[}]+)");

		/// <summary>
		///		Can be overwritten to change the key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public virtual string TransformKey(string key)
		{
			return key.ToUpper();
		}

		/// <summary>
		///		Gets the stored <see cref="TextResourceEntity"/> that matches the key and the culture (or <see cref="CultureInfo.CurrentUICulture"/> if null)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public TextResourceEntity? GetEntryOrNull(string key, CultureInfo culture = null)
		{
			culture = culture ?? CultureInfo.CurrentUICulture;
			return TextCache[TransformKey(key)]?.FirstOrDefault(e => e.Lang == culture);
		}

		/// <summary>
		///		Get the Translation for the culture (or <see cref="CultureInfo.CurrentUICulture"/> if null)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object GetTranslationOrNull(string key, CultureInfo culture = null, params object[] arguments)
		{
			var translationOrNull = GetEntryOrNull(key, culture)?.Text;
			if (translationOrNull != null && arguments.Any())
			{
				var strTranslation = translationOrNull.ToString();
				for (int i = 0; i < arguments.Length; i++)
				{
					strTranslation = strTranslation.Replace("{" + i + "}", arguments[i].ToString());
				}

				translationOrNull = strTranslation;
			}
			return translationOrNull;
		}

		/// <summary>
		///		Loads the Resources into the <see cref="TextCache"/>.
		///		If <see cref="transformReferences"/> is set, all occurrences of {{Key}} within an translation will be replaced with the translated key in the same culture
		/// </summary>
		/// <param name="resources"></param>
		/// <param name="cultures"></param>
		/// <param name="transformReferences"></param>
		public void Load(IEnumerable<ITranslationResource> resources,
			IEnumerable<CultureInfo> cultures,
			bool transformReferences = true)
		{
			var loaderExceptions = new List<Exception>();
			var textResources = new Dictionary<string, List<TextResourceEntity>>();
			foreach (var culture in cultures)
			{
				var resourcesOfCulture = new Dictionary<string, object>();
				foreach (var translationResource in resources)
				{
					var translationKeys = translationResource.Get(culture);
					foreach (var translationKey in translationKeys)
					{
						resourcesOfCulture[TransformKey(translationKey.Key)] = translationKey.Value;
					}
				}

				var localResources = new List<TextResourceEntity>();
				foreach (var textResource in resourcesOfCulture)
				{
					var key = textResource.Key;
					var keyParts = key.Split('/');
					localResources.Add(new TextResourceEntity(culture, key, textResource.Value, keyParts[0]));
				}

				var transformedResources = localResources
					.Select(localResource =>
					{
						var txt = localResource.Text;
						if (transformReferences)
						{
							txt = TransformText(localResource, localResources, loaderExceptions);
						}
						return new TextResourceEntity(
							localResource.Lang,
							localResource.Key,
							txt,
							localResource.Page);
					})
					.ToList();

				foreach (var group in transformedResources.GroupBy(e => e.Page))
				{
					List<TextResourceEntity> cache;
					if (!textResources.ContainsKey(group.Key))
					{
						textResources[group.Key] = cache = new List<TextResourceEntity>();
					}
					else
					{
						cache = textResources[group.Key];
					}
					cache.AddRange(group.ToArray());
				}
			}


			foreach (var textResource in textResources)
			{
				TextCache[textResource.Key] = textResource.Value.ToArray();
			}
		}

		private object TransformText(TextResourceEntity textResourceEntity,
			IEnumerable<TextResourceEntity> fromResources,
			IList<Exception> loaderExceptions,
			Stack<string> transformationChain = null)
		{
			transformationChain = transformationChain ?? new Stack<string>();

			if (!(textResourceEntity.Text is string text))
			{
				return textResourceEntity.Text;
			}

			foreach (Match match in RefRegex.Matches(text))
			{
				var textPart = match.Value.Trim();

				if (textPart.Trim('{', '}').StartsWith("!"))
				{
					text = textPart.Replace("!", "");
					continue;
				}

				var transformationResource = fromResources.FirstOrDefault(e =>
					e.Lang == textResourceEntity.Lang &&
					e.Key.Equals(textPart.ToUpper().Trim('{', '}')));
				if (transformationResource.Key == null)
				{
					loaderExceptions.Add(new InvalidOperationException($"{textResourceEntity.Lang.Name}: The requested Transformation in '{textResourceEntity.Key.ToUpper()}' for '{match.Value}' could not found"));
					return "";
				}

				if (transformationChain.Contains(transformationResource.Key))
				{
					loaderExceptions.Add(new InvalidOperationException($"{textResourceEntity.Lang.Name}: Endless requsion detected for: " + transformationChain.Aggregate((e, f) => e + " | " + f)));
					return "";
				}

				transformationChain.Push(transformationResource.Key);

				var textReplacement = TransformText(transformationResource, fromResources, loaderExceptions, transformationChain);

				transformationChain.Pop();

				//foreach (var textTransformationOperator in textPart.Split('|').Skip(1))
				//{
				//	var transformations = TextTransformations[textResourceEntity.Lang];
				//	var transformation = transformations.First(e => e.Key.Equals(textTransformationOperator));
				//	textPart = transformation.Value(textPart);
				//}

				text = text.Replace(textPart, textReplacement?.ToString());
			}

			return text;
		}
	}
}
