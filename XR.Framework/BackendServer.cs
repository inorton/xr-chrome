using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using XR.Server.Http;
using XR.Include;
using XR.Server.Json;

namespace XR.Framework
{
	public class RewriteUriArgs
	{
		public Uri RequestedUri { get; set; }
		public bool Finished { get; set; }
	}

	public delegate void RewriteUriRequestHandler (object sender, RewriteUriArgs args);

	public class BackendServer : HttpServer
	{
		List<string> staticFolders = new List<string> ();
		Dictionary<string,Processor> xriPaths = new Dictionary<string, Processor> ();
		Dictionary<string,string> mimeTypes = new Dictionary<string, string> ();

		public event RewriteUriRequestHandler RewriteUri;

		public BackendServer ()
		{
			RegisterMimeType ("text/plain", ".txt");
			RegisterMimeType ("text/css", ".css");
			RegisterMimeType ("text/html", ".html", ".htm");
			RegisterMimeType ("application/xhtml+xml", ".xhtml", ".xht");
			RegisterMimeType ("application/javascript", ".js");
			RegisterMimeType ("image/jpeg", ".jpg", ".jpeg", ".jpe");
			RegisterMimeType ("image/png", ".png");
			RegisterMimeType ("image/gif", ".gif");
			RegisterMimeType ("application/x-gtar-compressed", ".tgz", ".taz");
			RegisterMimeType ("application/pdf", ".pdf");
			RegisterMimeType ("application/x-debian-package", ".deb");

			UriRequested += HandleWebRequestStart;
			UriRequested += HandleWebRequest;
		}

		public void AddStaticPath (string localpath)
		{
			lock (staticFolders) {
				staticFolders.Add (localpath);
			}
		}

		public void AddXRIPath<T> (string localPath, string serverPath)
			where T : class, new()
		{
			lock (xriPaths) {
				xriPaths [serverPath] = new Processor () { 
					RootDirectory = localPath,
					Context = new T(),
				};
			}
		}

		public void RegisterMimeType (string mimetype, params string[] extensions)
		{
			foreach (string extension in extensions)
				mimeTypes [extension.ToLower ()] = mimetype;
		}

		public string LookupMimeType (string file)
		{
			string lower = file.ToLower ();
			// TODO - feed this from /etc/mime.types
			string type = "application/octet-stream";
			int matchlen = 0;
			foreach (var ext in mimeTypes.Keys) {
				if (lower.EndsWith (mimeTypes [ext])) {
					if (ext.Length > matchlen) {
						type = mimeTypes [ext];
						matchlen = ext.Length;
					}
				}
			}
			return type;
		}

		public void HandleWebRequestStart (object sender, UriRequestEventArgs args)
		{
			if (RewriteUri != null) {
				RewriteUri (sender, new RewriteUriArgs () { RequestedUri = args.Request.Url });
			}
		}

		public void HandleWebRequest (object sender, UriRequestEventArgs args)
		{
			lock (staticFolders) {
				try {
					foreach (var x in staticFolders) {
						var found = Helpers.FindLocalServerPath (args.Request.Url, x);
						if (found != null) {
							var mimetype = LookupMimeType (found);
							args.Handled = true;
							args.SetResponseType (mimetype);
							args.SetResponseState (200);
							// stream the file
							using (var f = File.OpenRead( found )) {
								using (var sr = new BufferedStream( f )) {
									var bb = new byte[4096];
									var count = 0;
									do {
										count = sr.Read (bb, 0, bb.Length);
										if (count == 0)
											break;
										args.ResponsStream.BaseStream.Write (bb, 0, count);
									} while ( true );
								}
							}
							break;
						}
					}

					// either 404 or a file in our xr-includes list
					if (args.Handled == false) {
						foreach (var serverPath in xriPaths.Keys) {
							var proc = xriPaths [serverPath];
							var file = proc.VirtualToLocalPath (args.Request.Url.AbsolutePath);
							if (File.Exists (file)) {
								args.Handled = true;
								args.SetResponseState (200);
								args.SetResponseType (LookupMimeType (file));

								proc.Transform (args.Request.Url.AbsolutePath, args.ResponsStream);
								break;
							}
						}
					}
				} catch (Exception ex) {
					Console.Error.WriteLine (ex);
					throw;
				}
			}
		}
	}
}

