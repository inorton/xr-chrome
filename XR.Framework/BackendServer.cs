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

	public class RequestRoute
	{
		public string Rootdir { get; set; }
		public string VirtualPathFile { get; set; }
		public Regex Match { get; set; }
		public Type ContextType { get; set; }
	}

	public class BackendServer : HttpServer
	{
		public string TemplateRoot { get; set; }
		List<RequestRoute> requestRoutes = new List<RequestRoute> ();
		List<string> staticFolders = new List<string> ();

		Dictionary<string,string> mimeTypes = new Dictionary<string, string> ();

		public BackendServer ()
		{
			TemplateRoot = "templates";
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

		public void AddDynamicPath<T> (string rootDir, string vPathFile, Regex vPathMatch)
			where T : RequestContextBase, new()
		{
			lock (requestRoutes) {
				requestRoutes.Add (new RequestRoute () { 
					ContextType = typeof(T),
					Rootdir = rootDir, 
					VirtualPathFile = vPathFile, 
					Match = vPathMatch });
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

		public virtual void HandleWebRequestStart (object sender, UriRequestEventArgs args)
		{
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
						foreach (var route in requestRoutes) {
							if (route.Match.IsMatch (args.Request.Url.AbsolutePath)) {
								var context = Activator.CreateInstance (route.ContextType) as RequestContextBase;
								if (context.Load (args)) {
									var proc = new Processor () { 
										RootDirectory = TemplateRoot,
										Context = context };
									proc.Transform (route.VirtualPathFile, args.ResponsStream);
								}
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

