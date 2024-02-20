using System.IO;
using Morestachio.Formatter.Framework;

namespace Morestachio.Helper.FileSystem;

/// <summary>
///		Service for accessing the local File System.
/// </summary>
/// <remarks>
///		This service allows the access to the underlying FileSystem.
///		It is not available in the standard configuration and must be first enabled via <see cref="FileSystemExtensions.WithFileSystem(Morestachio.IParserOptionsBuilder,System.Func{Morestachio.Helper.FileSystem.FileSystemService})"/>
/// </remarks>
[MorestachioExtensionSetup("Must be enabled with FileSystemExtensions.RegisterFileSystem before available")]
[ServiceName("FileSystem")]
public class FileSystemService
{
	private readonly string _workingDirectory;

	/// <summary>
	///		Creates a new FileSystemService that is rooted on <code>Directory.GetCurrentDirectory()</code>
	/// </summary>
	public FileSystemService() : this(Directory.GetCurrentDirectory())
	{
	}

	/// <summary>
	///		Creates a new FileSystemService that is rooted on the workingDirectory
	/// </summary>
	/// <param name="workingDirectory"></param>
	public FileSystemService(string workingDirectory)
	{
		_workingDirectory = workingDirectory;
		File = new FileSystemFileService(this);
	}

	internal string GetAbsolutePath(string path)
	{
		if (Path.IsPathRooted(path))
		{
			return path;
		}

		return Path.Combine(_workingDirectory, path);
	}

	/// <summary>
	///		File specific methods for use with the $services collection
	/// </summary>
	public FileSystemFileService File { get; private set; }
}