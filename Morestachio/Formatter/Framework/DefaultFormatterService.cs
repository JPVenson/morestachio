using System.Security.Cryptography;
using System.Text;
using Morestachio.Formatter.Constants;
using Morestachio.Formatter.Predefined;
using Morestachio.Formatter.Predefined.Accounting;
using Morestachio.Formatter.Services;
using Morestachio.Helper.Logging;
using Encoding = Morestachio.Formatter.Constants.Encoding;

namespace Morestachio.Formatter.Framework;

/// <summary>
///		Contains methods for generating a default formatter service
/// </summary>
public static class DefaultFormatterService
{
	static DefaultFormatterService()
	{
		Default = new Lazy<IMorestachioFormatterService>(BuildDefaultMorestachioFormatterService);
	}

	/// <summary>
	///		The default service containing all build in formatter, services and constants
	/// </summary>
	public static Lazy<IMorestachioFormatterService> Default { get; }

	private static IMorestachioFormatterService BuildDefaultMorestachioFormatterService()
	{
		var defaultFormatter = new MorestachioFormatterService(false, null);
		defaultFormatter.AddFromType(typeof(ObjectFormatter));
		defaultFormatter.AddFromType(typeof(Number));
		defaultFormatter.AddFromType(typeof(BooleanFormatter));
		defaultFormatter.AddFromType(typeof(DateFormatter));
		defaultFormatter.AddFromType(typeof(EqualityFormatter));
		defaultFormatter.AddFromType(typeof(LinqFormatter));
		defaultFormatter.AddFromType(typeof(ListExtensions));
		defaultFormatter.AddFromType(typeof(RegexFormatter));
		defaultFormatter.AddFromType(typeof(TimeSpanFormatter));
		defaultFormatter.AddFromType(typeof(StringFormatter));
		defaultFormatter.AddFromType(typeof(RandomFormatter));
		defaultFormatter.AddFromType(typeof(Worktime));
		defaultFormatter.AddFromType(typeof(Money));
		defaultFormatter.AddFromType(typeof(Currency));
		defaultFormatter.AddFromType(typeof(CurrencyHandler));
		defaultFormatter.AddFromType(typeof(CurrencyConversion));
		defaultFormatter.AddFromType(typeof(HtmlFormatter));
		defaultFormatter.AddFromType(typeof(LoggingFormatter));

		defaultFormatter.AddService(typeof(CryptService), new CryptService());
		defaultFormatter.AddFromType<IMorestachioCryptographyService>();
		defaultFormatter.AddFromType<AesCryptography>();

		defaultFormatter.AddService(typeof(HashService), new HashService());
		defaultFormatter.AddFromType(typeof(HashAlgorithm));

		defaultFormatter.Constants.Add("Encoding", typeof(EncodingConstant));
		defaultFormatter.Constants.Add("DateTime", typeof(DateTimeConstant));
		defaultFormatter.Constants.Add("Currencies", typeof(WellKnownCurrencies));
		defaultFormatter.Constants.Add("CurrencyHandler", CurrencyHandler.DefaultHandler);

		defaultFormatter.AddFromType(typeof(EncodingConstant));
		defaultFormatter.AddFromType(typeof(EncoderFallback));
		defaultFormatter.AddFromType(typeof(DateTimeConstant));
		defaultFormatter.AddFromType<Encoding>();

		return defaultFormatter;
	}
}