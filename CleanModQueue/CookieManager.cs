// CookieManager.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// A simple class to manage cookies for HTTP communication.  It slurps
// up IE cookies, if any are available, so that HttpClient will use them
// for communications to the reddit site.
//
// last saved: <2012-May-05 11:40:19>
// ------------------------------------------------------------------
//
// Copyright (c) 2012 Dino Chiesa
// All rights reserved.
//
// This code is Licensed under the New BSD license.
// See the License.txt file that accompanies this module.
//
// ------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace Dino.Http
{
    public class CookieManager
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookie(string url,
                                                      string name,
                                                      StringBuilder data,
                                                      ref int dataSize);

        public static string RetrieveIECookiesForUrl(string url)
        {
            StringBuilder cookieHeader = new StringBuilder(new String(' ', 256), 256);
            int datasize = cookieHeader.Length;
            if (!InternetGetCookie(url, null, cookieHeader, ref datasize))
            {
                if (datasize < 0)
                    return String.Empty;

                cookieHeader = new StringBuilder(datasize); // resize with new datasize
                InternetGetCookie(url, null, cookieHeader, ref datasize);
            }
            return cookieHeader.ToString();

        }

        public static CookieContainer GetCookieContainerForUrl(string url)
        {
            var uri = new Uri(url);
            return GetCookieContainerForUrl(uri);
        }

        public static CookieContainer GetCookieContainerForUrl(Uri url)
        {
            CookieContainer container = new CookieContainer();
            string cookieHeaders = RetrieveIECookiesForUrl(url.AbsoluteUri);
            if (cookieHeaders.Length > 0)
            {
                try { container.SetCookies(url, cookieHeaders); }
                catch (CookieException) { }
            }
            return container;
        }
    }
}
