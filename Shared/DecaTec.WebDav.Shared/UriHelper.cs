﻿using System;
using System.Linq;
using System.Net;
using System.Text;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Helper class for handling URIs/URLs.
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// Adds a trailing slash to a URI (only if needed).
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to add the trailing slash when needed.</param>        
        /// <returns>The <see cref="Uri"/> with a trailing slash (only if needed).</returns>
        /// <remarks>This method does not expect the URI to be file. Use an overload of this method when the URI is expected to be a file. </remarks>
        public static Uri AddTrailingSlash(Uri uri)
        {
            return AddTrailingSlash(uri, false);
        }

        /// <summary>
        /// Adds a trailing slash to a URI (only if needed).
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to add the trailing slash when needed.</param>
        /// <param name="expectFile">True, if the function should expect a file at the end of the URI. False if these should be no distinction between files and folders containing a dot in their name.</param>
        /// <returns>The <see cref="Uri"/> with a trailing slash (only if needed).</returns>
        public static Uri AddTrailingSlash(Uri uri, bool expectFile)
        {
            return new Uri(AddTrailingSlash(uri.ToString(), expectFile), UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        ///  Adds a trailing slash to a URL (only if needed).
        /// </summary>
        /// <param name="url">The URL to add the trailing slash when needed.</param>        
        /// <returns>The URL with a trailing slash (only if needed).</returns>
        /// <remarks>This method does not expect the URL to be file. Use an overload of this method when the URL is expected to be a file.</remarks>
        public static string AddTrailingSlash(string url)
        {
            return AddTrailingSlash(url, false);
        }

        /// <summary>
        ///  Adds a trailing slash to a URL (only if needed).
        /// </summary>
        /// <param name="url">The URL to add the trailing slash when needed.</param>
        /// <param name="expectFile">True, if the function should expect a file at the end of the URL. False if these should be no distinction between files and folders containing a dot in their name.</param>
        /// <returns>The URL with a trailing slash (only if needed).</returns>
        public static string AddTrailingSlash(string url, bool expectFile)
        {
            var startsWithSlash = url.StartsWith("/");
            var slashSplit = url.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            for (int i = 0; i < slashSplit.Length; i++)
            {
                sb.Append(slashSplit[i]);

                if (i == 0 && slashSplit[i].Contains(":"))
                    sb.Append("//"); // First has to be a double slash (http://...).
                else if (i != slashSplit.Length - 1)
                    sb.Append("/"); // Do not add a slash at the very end, this is handled further below.
            }

            url = sb.ToString();

            if (startsWithSlash)
                url = "/" + url;

            if (expectFile && slashSplit.Last().Contains("."))
                return url; // It's a file.
            else
                return url + "/"; // Trailing slash not present, add it.
        }

        /// <summary>
        /// Gets a combined <see cref="Uri"/> from two URIs.
        /// </summary>
        /// <param name="uri1">The first <see cref="Uri"/>.</param>
        /// <param name="uri2">The second <see cref="Uri"/>.</param>
        /// <returns>The combined <see cref="Uri"/> from the two URIs specified.</returns>
        public static Uri CombineUri(Uri uri1, Uri uri2)
        {
            return CombineUri(uri1, uri2, false);
        }

        /// <summary>
        /// Gets a combined <see cref="Uri"/> from two URIs.
        /// </summary>
        /// <param name="uri1">The first <see cref="Uri"/>.</param>
        /// <param name="uri2">The second <see cref="Uri"/>.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URIs are not simply combined, but duplicate path segments are removed from the resulting URI. 
        /// As an example, combining the URIs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <returns>The combined <see cref="Uri"/> from the two URIs specified.</returns>
        public static Uri CombineUri(Uri uri1, Uri uri2, bool removeDuplicatePath)
        {
            if (uri1 == null)
                return uri2;

            if (uri1.IsAbsoluteUri && uri2.IsAbsoluteUri && (uri1.Scheme != uri2.Scheme || uri1.Host != uri2.Host))
                throw new ArgumentException("The absolute URIs provided do not have the same host/scheme");

            if (!uri1.IsAbsoluteUri && uri2.IsAbsoluteUri)
                throw new ArgumentException("Cannot combine URIs because uri1 is relative URI and uri2 is absolute URI");

            if (uri1.IsAbsoluteUri && uri2.IsAbsoluteUri)
                return new Uri(uri1, uri2);

            if (!uri1.IsAbsoluteUri && !uri2.IsAbsoluteUri)
                return new Uri(uri1.ToString().TrimEnd('/') + "/" + uri2.ToString().TrimStart('/'), UriKind.Relative);

            var absolutePath1 = WebUtility.UrlDecode(uri1.AbsolutePath.TrimEnd('/'));
            var absolutePath2 = WebUtility.UrlDecode(uri2.ToString().TrimEnd('/'));

            if (uri2.IsAbsoluteUri)
                absolutePath2 = uri2.AbsolutePath;

            if (!string.IsNullOrEmpty(absolutePath2) && absolutePath1.EndsWith(absolutePath2))
                return uri1;

            var uri2Str = WebUtility.UrlDecode(uri2.ToString().Trim('/'));

            if (removeDuplicatePath)
            {
                var splitUri2 = absolutePath2.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (splitUri2.Length > 0)
                {
                    var tmp = string.Empty;
                    var absolutePath2WithoutLastPath = string.Empty;

                    for (int i = splitUri2.Length - 1; i > 0; i--)
                    {
                        tmp = splitUri2[i] + (string.IsNullOrEmpty(tmp) ? tmp : "/" + tmp);
                        var index = absolutePath2.LastIndexOf(tmp);
                        absolutePath2WithoutLastPath = absolutePath2.Remove(index);

                        if (!string.IsNullOrEmpty(absolutePath2WithoutLastPath) && absolutePath1.TrimEnd('/').EndsWith(absolutePath2WithoutLastPath.TrimEnd('/')))
                            uri2Str = tmp;
                    }
                }
            }

            UriBuilder uriBuilder = new UriBuilder(uri1);
            var pathUri1 = WebUtility.UrlDecode(uriBuilder.Path.Trim('/'));
            var pathUri2 = uri2Str;

            if (!string.IsNullOrEmpty(pathUri2))
                pathUri1 = pathUri1.Replace(pathUri2, string.Empty);

            var trailingSlash = uri2.ToString().EndsWith("/") ? "/" : string.Empty;
            pathUri2 = !string.IsNullOrEmpty(pathUri2) ? "/" + pathUri2.Trim('/') : string.Empty;
            uriBuilder.Path = pathUri1.Trim('/') + pathUri2 + trailingSlash;
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Gets a combined URL from two URLs.
        /// </summary>
        /// <param name="url1">The first URL.</param>
        /// <param name="url2">The second URL.</param>
        /// <returns>The combined URL as string.</returns>
        public static string CombineUrl(string url1, string url2)
        {
            return CombineUrl(url1, url2, false);
        }

        /// <summary>
        /// Gets a combined URL from two URLs.
        /// </summary>
        /// <param name="url1">The first URL.</param>
        /// <param name="url2">The second URL.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URLs are not simply combined, but duplicate path segments are removed from the resulting URL. 
        /// As an example, combining the URLs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <returns>The combined URL as string.</returns>
        public static string CombineUrl(string url1, string url2, bool removeDuplicatePath)
        {
            var uri1 = new Uri(url1, UriKind.RelativeOrAbsolute);
            var uri2 = new Uri(url2, UriKind.RelativeOrAbsolute);
            return CombineUri(uri1, uri2, removeDuplicatePath).ToString();
        }

        /// <summary>
        /// Gets a combined <see cref="Uri"/> from two URIs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="uri1">The first <see cref="Uri"/>.</param>
        /// <param name="uri2">The second <see cref="Uri"/>.</param>
        /// <returns>A combined <see cref="Uri"/> from the two URIs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static Uri GetCombinedUriWithTrailingSlash(Uri uri1, Uri uri2)
        {
            return GetCombinedUriWithTrailingSlash(uri1, uri2, false, false);
        }

        /// <summary>
        /// Gets a combined <see cref="Uri"/> from two URIs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="uri1">The first <see cref="Uri"/>.</param>
        /// <param name="uri2">The second <see cref="Uri"/>.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URIs are not simply combined, but duplicate path segments are removed from the resulting URI. 
        /// As an example, combining the URIs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <returns>A combined <see cref="Uri"/> from the two URIs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static Uri GetCombinedUriWithTrailingSlash(Uri uri1, Uri uri2, bool removeDuplicatePath)
        {
            return GetCombinedUriWithTrailingSlash(uri1, uri2, removeDuplicatePath, false);
        }

        /// <summary>
        /// Gets a combined <see cref="Uri"/> from two URIs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="uri1">The first <see cref="Uri"/>.</param>
        /// <param name="uri2">The second <see cref="Uri"/>.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URIs are not simply combined, but duplicate path segments are removed from the resulting URI. 
        /// As an example, combining the URIs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <param name="expectFile">True, if the function should expect a file at the end of the URI. False if these should be no distinction between files and folders containing a dot in their name.</param>
        /// <returns>A combined <see cref="Uri"/> from the two URIs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static Uri GetCombinedUriWithTrailingSlash(Uri uri1, Uri uri2, bool removeDuplicatePath, bool expectFile)
        {
            return AddTrailingSlash(CombineUri(uri1, uri2, removeDuplicatePath), expectFile);
        }

        /// <summary>
        /// Gets a combined URL from two URLs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="url1">The first URL.</param>
        /// <param name="url2">The second URL.</param>
        /// <returns>A combined URL as string from the two URLs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static string GetCombinedUrlWithTrailingSlash(string url1, string url2)
        {
            return GetCombinedUrlWithTrailingSlash(url1, url2, false, false);
        }

        /// <summary>
        /// Gets a combined URL from two URLs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="url1">The first URL.</param>
        /// <param name="url2">The second URL.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URLs are not simply combined, but duplicate path segments are removed from the resulting URL. 
        /// As an example, combining the URLs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <returns>A combined URL as string from the two URLs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static string GetCombinedUrlWithTrailingSlash(string url1, string url2, bool removeDuplicatePath)
        {
            return GetCombinedUrlWithTrailingSlash(url1, url2, removeDuplicatePath, false);
        }

        /// <summary>
        /// Gets a combined URL from two URLs (absolute or relative) with a trailing slash added at the end when needed.
        /// </summary>
        /// <param name="url1">The first URL.</param>
        /// <param name="url2">The second URL.</param>
        /// <param name="removeDuplicatePath">When set to true, the given URLs are not simply combined, but duplicate path segments are removed from the resulting URL. 
        /// As an example, combining the URLs https://myserver.com/webdav and /webdav/myfile.txt will result in https://myserver.com/webdav/myfile.txt (not https://myserver.com/webdav/webdav/myfile.txt).</param>
        /// <param name="expectFile">True, if the function should expect a file at the end of the URL. False if these should be no distinction between files and folders containing a dot in their name.</param>
        /// <returns>A combined URL as string from the two URLs specified with a trailing slash added at the end when needed.</returns>
        /// <remarks>This is a combination of the methods CombineUri and AddTrailingSlash in this class.</remarks>
        public static string GetCombinedUrlWithTrailingSlash(string url1, string url2, bool removeDuplicatePath, bool expectFile)
        {
            var uri1 = new Uri(url1, UriKind.RelativeOrAbsolute);
            var uri2 = new Uri(url2, UriKind.RelativeOrAbsolute);
            return GetCombinedUriWithTrailingSlash(uri1, uri2, removeDuplicatePath, expectFile).ToString();
        }

        /// <summary>
        /// Removes a port from an <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> with port to remove the port from.</param>
        /// <returns>The <see cref="Uri"/> specified without port.</returns>
        public static Uri RemovePort(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                return uri;

            UriBuilder builder = new UriBuilder(uri);
            builder.Port = -1;
            return builder.Uri;
        }

        /// <summary>
        /// Removes a port from an URL.
        /// </summary>
        /// <param name="url">The URL with port to remove the port from.</param>
        /// <returns>The URL specified without port.</returns>
        public static string RemovePort(string url)
        {
            return RemovePort(new Uri(url)).ToString();
        }

        /// <summary>
        /// Sets the port of an <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to set the port.</param>
        /// <param name="port">The port to set the <see cref="Uri"/> to.</param>
        /// <returns>The <see cref="Uri"/> with the port specified.</returns>
        public static Uri SetPort(Uri uri, int port)
        {
            if (!uri.IsAbsoluteUri)
                return uri;

            UriBuilder builder = new UriBuilder(uri);
            builder.Port = port;
            return builder.Uri;
        }

        /// <summary>
        /// Sets the port of an URL.
        /// </summary>
        /// <param name="url">The URL to set the port.</param>
        /// <param name="port">The port to set the URL to.</param>
        /// <returns>The URL with the port specified.</returns>
        public static string SetPort(string url, int port)
        {
            return SetPort(new Uri(url), port).ToString();
        }

        /// <summary>
        /// Retrieves the port from a given <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to get the port from.</param>
        /// <returns>The port of the <see cref="Uri"/>.</returns>
        public static int GetPort(Uri uri)
        {
            UriBuilder builder = new UriBuilder(uri);
            return builder.Port;
        }

        /// <summary>
        /// Retrieves the port from a given URL.
        /// </summary>
        /// <param name="url">The URL to get the port from.</param>
        /// <returns>The port of the URL.</returns>
        public static int GetPort(string url)
        {
            UriBuilder builder = new UriBuilder(url);
            return builder.Port;
        }
    }
}
