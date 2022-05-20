using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Morestachio.Util;

namespace Morestachio.Helper.Localization;

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
		TranslationResources = new List<ITranslationResource>();
		NotFound = new Dictionary<CultureInfo, TextResourceEntity>();
	}

	/// <summary>
	///		The list of <see cref="ITranslationResource"/>
	/// </summary>
	public IList<ITranslationResource> TranslationResources { get; }

	/// <summary>
	///		Adds the resource to the list of resources
	/// </summary>
	/// <param name="resource"></param>
	/// <returns></returns>
	public virtual MorestachioLocalizationService AddResource(ITranslationResource resource)
	{
		TranslationResources.Add(resource);
		return this;
	}

	/// <summary>
	///		The result of the Load method call
	/// </summary>
	public Dictionary<string, TextResourceEntity[]> TextCache { get; }

	/// <summary>
	///		Allows to define text outputs for cultures when a key is not found
	/// </summary>
	public IDictionary<CultureInfo, TextResourceEntity> NotFound { get; set; }

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
	public TextResourceEntity? GetEntryOrNull(string key, CultureInfo culture = null)
	{
		culture = culture ?? CultureInfo.CurrentUICulture;

		if (TextCache.TryGetValue(TransformKey(key), out var res))
		{
			return res.FirstOrDefault(e => e.Lang.Equals(culture));
		}

		if (NotFound.TryGetValue(culture, out var nf))
		{
			return nf;
		}

		return null;
	}

	/// <inheritdoc />
	public async Task<TextResourceEntity?> GetEntryOrLoad(string key, CultureInfo culture = null)
	{
		var entry = GetEntryOrNull(key, culture);
		if (entry != null)
		{
			return entry;
		}

		var transformKey = TransformKey(key);
		foreach (var translationResource in TranslationResources)
		{
			if (await translationResource.GetTranslation(transformKey, culture, out var translation).ConfigureAwait(false))
			{
				return new TextResourceEntity(culture,transformKey,translation, key.Split('/')[0]);
			}
		}

		return null;
	}


	/// <summary>
	///		Get the Translation for the culture (or <see cref="CultureInfo.CurrentUICulture"/> if null)
	/// </summary>
	public object GetTranslationOrNull(string key, CultureInfo culture = null, params object[] arguments)
	{
		var translationOrNull = GetEntryOrNull(key, culture)?.Text;
		if (translationOrNull != null && arguments.Any())
		{
			var strTranslation = translationOrNull.ToString() ?? string.Empty;
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
	///		If transformReferences is set, all occurrences of {{Key}} within an translation will be replaced with the translated key in the same culture
	/// </summary>
	public MorestachioLocalizationService Load(IEnumerable<CultureInfo> cultures,
												bool transformReferences = true)
	{
		var loaderExceptions = new List<Exception>();
		var textResources = new Dictionary<string, List<TextResourceEntity>>();
		foreach (var culture in cultures)
		{
			var resourcesOfCulture = new Dictionary<string, object>();
			foreach (var translationResource in TranslationResources)
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

		return this;
	}

	private static object TransformText(TextResourceEntity textResourceEntity,
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

			if (textPart.Trim('{', '}').StartsWith('!'))
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