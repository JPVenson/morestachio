using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.ParserErrors;

namespace Morestachio.Framework
{
	///// <summary>
	/////		Engine class for Text Compiling
	///// </summary>
	//public class TextEngine
	//{
	//	public TokenPair[] Tokenize(ParserOptions parsingOptions, IList<IMorestachioError> errors)
	//	{
	//		if (parsingOptions == null)
	//		{
	//			throw new ArgumentNullException(nameof(parsingOptions));
	//		}
			
	//		if (parsingOptions.Template == null)
	//		{
	//			throw new ArgumentNullException(nameof(parsingOptions), "The template is null");
	//		}
	//		return Tokenizer.Tokenize(parsingOptions, errors, null).ToArray();
	//	}
		
	//	public MorestachioDocumentInfo Parse(ParserOptions parsingOptions, Queue<TokenPair> tokens)
	//	{
	//		if (parsingOptions == null)
	//		{
	//			throw new ArgumentNullException(nameof(parsingOptions));
	//		}

	//		if (parsingOptions.SourceFactory == null)
	//		{
	//			throw new ArgumentNullException(nameof(parsingOptions), "The given Stream is null");
	//		}

	//		return new MorestachioDocumentInfo(parsingOptions, Parser.Parse(tokens, parsingOptions));
	//	}

		
	//	public MorestachioDocumentInfo TokenizeAndParse(ParserOptions parsingOptions)
	//	{
	//		var errors = new List<IMorestachioError>();
	//		var profiler = new PerformanceProfiler(parsingOptions.ProfileExecution);
	//		Queue<TokenPair> tokens;
	//		using (profiler.Begin("Tokenize"))
	//		{
	//			tokens = new Queue<TokenPair>(Tokenizer.Tokenize(parsingOptions, errors, profiler));
	//		}

	//		//if there are any errors do not parse the template
	//		MorestachioDocumentInfo documentInfo;
	//		if (errors.Any())
	//		{
	//			documentInfo = new MorestachioDocumentInfo(parsingOptions, null, errors);
	//		}
	//		else
	//		{
	//			documentInfo = Parse(parsingOptions, tokens);
	//		}
	//		documentInfo.Profiler = profiler;
	//		return documentInfo;
	//	}
	//}
}
