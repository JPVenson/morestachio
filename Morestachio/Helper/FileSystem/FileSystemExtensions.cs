namespace Morestachio.Helper.FileSystem;

/// <summary>
///		
/// </summary>
public static class FileSystemExtensions
{
	/// <summary>
	///		Registers all necessary components to use the <code>FileSystemService</code>
	/// </summary>
	/// <param name="options"></param>
	/// <param name="config"></param>
	public static IParserOptionsBuilder WithFileSystem(this IParserOptionsBuilder options, Func<FileSystemService> config)
	{
		var fs = config();

		return options
				.WithConstant("FileSystem", fs)
				.WithService(fs)
				.WithFormatters<FileSystemService>()
				.WithFormatters<FileSystemFileService>();
	}

	/// <summary>
	///		Registers all necessary components to use the <code>FileSystemService</code>
	/// </summary>
	/// <param name="options"></param>
	public static IParserOptionsBuilder WithFileSystem(this IParserOptionsBuilder options)
	{
		return options.WithFileSystem(() => new FileSystemService());
	}
}