using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Path = System.IO.Path;

namespace Morestachio.Helper.FileSystem
{
	public class FileSystemFileService
	{
		private readonly FileSystemService _fsService;

		public FileSystemFileService(FileSystemService fsService)
		{
			_fsService = fsService;
		}
		
		[MorestachioFormatter(nameof(AppendAllText), "Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.")]
		public void AppendAllText(string path, 
			string contents, 
			[FormatterValueConverter(typeof(EncodingConverter))]Encoding encoding = null)
		{
			File.AppendAllText(_fsService.GetAbsolutePath(path), contents, encoding ?? Encoding.Default);
		}

		[MorestachioFormatter(nameof(Copy), "Copies an existing file to a new file. Overwriting a file of the same name is not allowed.")]
		public void Copy(string sourceFileName, string destinationFileName, bool overwrite = false)
		{
			File.Copy(_fsService.GetAbsolutePath(sourceFileName), _fsService.GetAbsolutePath(destinationFileName), overwrite);
		}
		
		[MorestachioFormatter(nameof(Create), "Creates or overwrites a file in the specified path.")]
		public void Create(string fileName)
		{
			File.Create(_fsService.GetAbsolutePath(fileName)).Close();
		}

		[MorestachioFormatter(nameof(Delete), "Deletes the specified file.")]
		public void Delete(string fileName)
		{
			File.Delete(_fsService.GetAbsolutePath(fileName));
		}
		
		[MorestachioFormatter(nameof(Exists), "Determines whether the specified file exists.")]
		public bool Exists(string fileName)
		{
			return File.Exists(_fsService.GetAbsolutePath(fileName));
		}
		
		[MorestachioFormatter(nameof(GetCreationTime), "Returns the creation date and time of the specified file or directory.")]
		public DateTime GetCreationTime(string fileName)
		{
			return File.GetCreationTime(_fsService.GetAbsolutePath(fileName));
		}

		[MorestachioFormatter(nameof(GetCreationTimeUtc), "Returns the creation date and time, in coordinated universal time (UTC), of the specified file or directory.")]
		public DateTime GetCreationTimeUtc(string fileName)
		{
			return File.GetCreationTimeUtc(_fsService.GetAbsolutePath(fileName));
		}

		[MorestachioFormatter(nameof(GetLastAccessTime), "Returns the date and time the specified file or directory was last accessed.")]
		public DateTime GetLastAccessTime(string fileName)
		{
			return File.GetLastAccessTime(_fsService.GetAbsolutePath(fileName));
		}

		[MorestachioFormatter(nameof(GetLastAccessTimeUtc), "Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.")]
		public DateTime GetLastAccessTimeUtc(string fileName)
		{
			return File.GetLastAccessTime(_fsService.GetAbsolutePath(fileName));
		}

		[MorestachioFormatter(nameof(GetLastWriteTime), "Returns the date and time the specified file or directory was last written to.")]
		public DateTime GetLastWriteTime(string fileName)
		{
			return File.GetLastAccessTime(_fsService.GetAbsolutePath(fileName));
		}
		
		[MorestachioFormatter(nameof(GetLastWriteTimeUtc), "Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.")]
		public DateTime GetLastWriteTimeUtc(string fileName)
		{
			return File.GetLastAccessTime(_fsService.GetAbsolutePath(fileName));
		}
		
		[MorestachioFormatter(nameof(Move), "Moves a specified file to a new location, providing the options to specify a new file name and to overwrite the destination file if it already exists.")]
		public void Move(string sourceFileName, string destinationFileName)
		{
			File.Move(_fsService.GetAbsolutePath(sourceFileName), _fsService.GetAbsolutePath(destinationFileName));
		}

		[MorestachioFormatter(nameof(ReadAllBytes), "Opens a binary file, reads the contents of the file into a byte array, and then closes the file.")]
		public byte[] ReadAllBytes(string fileName)
		{
			return File.ReadAllBytes(_fsService.GetAbsolutePath(fileName));
		}

		[MorestachioFormatter(nameof(ReadAllText), "Opens a text file, reads all the text in the file, and then closes the file.")]
		public string ReadAllText(string fileName, [FormatterValueConverter(typeof(EncodingConverter))] Encoding encoding = null)
		{
			return File.ReadAllText(_fsService.GetAbsolutePath(fileName), encoding ?? Encoding.Default);
		}

		[MorestachioFormatter(nameof(ReadAllTextLines), "Opens a file, reads all lines of the file with the specified encoding, and then closes the file.")]
		public string[] ReadAllTextLines(string fileName, [FormatterValueConverter(typeof(EncodingConverter))] Encoding encoding = null)
		{
			return File.ReadAllLines(_fsService.GetAbsolutePath(fileName), encoding ?? Encoding.Default);
		}
	}

	/// <summary>
	///		Service for accessing the local File System
	/// </summary>
	public class FileSystemService
	{
		private readonly string _workingDirectory;

		public FileSystemService() : this(Directory.GetCurrentDirectory())
		{
		}

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

		public FileSystemFileService File { get; private set; }
	
	}

	public static class FileSystemExtensions
	{
		public static void RegisterFileSystem(this ParserOptions options, Func<FileSystemService> action)
		{
			var fs = action();
			options.Formatters.AddService<FileSystemService>(fs);
			options.Formatters.AddFromType<FileSystemService>();
			options.Formatters.AddFromType<FileSystemFileService>();
		}
	}
}
