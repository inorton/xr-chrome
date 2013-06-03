using System;
using System.IO;
using System.Linq;

namespace XR.Framework
{
	public class Helpers
	{
		static string osdirsep = System.IO.Path.DirectorySeparatorChar.ToString ();
		static string webdirsep = System.IO.Path.DirectorySeparatorChar.ToString ();
		public static string OsPathJoin (params string[] path)
		{
			return SepJoin (osdirsep, path);
		}

		public static string WebPathJoin (params string[] path)
		{
			return SepJoin (webdirsep, path);
		}

		public static string SepJoin (string sep, params string[] list)
		{
			return string.Join (sep, list);
		}

		public static string[] GetPathComponents (Uri link)
		{
			return link.AbsolutePath.Split (webdirsep.ToCharArray ());
		}

		/// <summary>
		/// Finds the local server path.
		/// </summary>
		/// <returns>The local server path.</returns>
		/// <param name="link">Link to find</param>
		/// <param name="localSearchDirectory">The root directory of the server.</param>
		public static string FindLocalServerPath (Uri link, string localSearchDirectory)
		{
			
			if (string.IsNullOrEmpty (localSearchDirectory)) {
				localSearchDirectory = ".";
			}
			
			var parts = Helpers.GetPathComponents (link);
			if (parts.Contains (".."))
				return null; // trigger a 404
			var check = Helpers.OsPathJoin (parts);
			check = check.TrimStart (new char[]{ Path.DirectorySeparatorChar });
			
			var full = 
				Helpers.OsPathJoin (localSearchDirectory, check);
			
			if (Directory.Exists (full) || File.Exists (full)) {
				return full;
			}
			
			return null;
		}
	}
}

