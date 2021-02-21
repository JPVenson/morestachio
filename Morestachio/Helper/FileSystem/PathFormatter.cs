using System.IO;
using System.Linq;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.FileSystem
{
	/// <summary>
	///		Allows access to the IFileSystem from within a template
	/// </summary>
	public static class PathFormatter
	{
		/// <summary>Changes the extension of a path string.</summary>
		[MorestachioFormatter("ChangeExtension", "Changes the extension of a path string.")]
		public static string ChangeExtension(string path, string extension)
		{
			return Path.ChangeExtension(path, extension);
		}

		/// <summary>Combines two strings into a path.</summary>
		[MorestachioFormatter("Combine", "Combines two strings into a path.")]
		public static string Combine(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}

		/// <summary>Combines an array of strings into a path</summary>
		[MorestachioFormatter("Combine", "Combines an array of strings into a path")]
		public static string Combine(params object[] paths)
		{
			return Path.Combine(paths.OfType<string>().ToArray());
		}

		/// <summary>Returns the directory information for the specified path string</summary>
		[MorestachioFormatter("GetDirectoryName", "Returns the directory information for the specified path string")]
		public static string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		/// <summary>Returns the extension of the specified path string</summary>
		[MorestachioFormatter("GetExtension", "Returns the extension of the specified path string")]
		public static string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		/// <summary>Returns the file name and extension of the specified path string</summary>
		[MorestachioFormatter("GetFileName", "Returns the file name and extension of the specified path string")]
		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		/// <summary>Returns the file name of the specified path string without the extension.</summary>
		[MorestachioFormatter("GetFileNameWithoutExtension", "Returns the file name of the specified path string without the extension.")]
		public static string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		/// <summary>Gets an array containing the characters that are not allowed in file names.</summary>
		[MorestachioFormatter("GetInvalidFileNameChars", "Gets an array containing the characters that are not allowed in file names.")]
		public static char[] GetInvalidFileNameChars()
		{
			return Path.GetInvalidFileNameChars();
		}

		/// <summary>Gets an array containing the characters that are not allowed in path names</summary>
		[MorestachioFormatter("GetInvalidPathChars", "Gets an array containing the characters that are not allowed in path names")]
		public static char[] GetInvalidPathChars()
		{
			return Path.GetInvalidPathChars();
		}

		/// <summary>Gets the root directory information of the specified path</summary>
		[MorestachioFormatter("GetPathRoot", "Gets the root directory information of the specified path")]
		public static string GetPathRoot(string path)
		{
			return Path.GetPathRoot(path);
		}

		/// <summary>Determines whether a path includes a file name extension</summary>
		[MorestachioFormatter("HasExtension", "Determines whether a path includes a file name extension")]
		public static bool HasExtension(string path)
		{
			return Path.HasExtension(path);
		}

		/// <summary>Gets a value indicating whether the specified path string contains a root</summary>
		[MorestachioFormatter("IsPathRooted", "Gets a value indicating whether the specified path string contains a root")]
		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}
	}
}