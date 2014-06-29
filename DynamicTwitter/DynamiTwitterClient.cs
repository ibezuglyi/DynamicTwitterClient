using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DynamicTwitter
{
    public class DynamiTwitterClient : DynamicObject
    {
        private readonly string consumerKey;
        private readonly string consumerSecret;
        private const string ApiBaseUrl = "https://api.twitter.com";
        private readonly IList<string> pathList;
        private const string MimeType = ".json";
        private const string Version = "1.1";

        public DynamiTwitterClient(string consumerKey, string consumerSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            pathList = new List<string>();
        }

        public DynamiTwitterClient(IList<string> pathList, string consumerKey, string consumerSecret)
            : this(consumerKey, consumerSecret)
        {
            this.pathList = pathList;
        }

        private string GetBearerToken()
        {
            JToken token = JObject.Parse(GetBearerTokenJson(consumerKey, consumerSecret));
            return token.SelectToken("access_token").Value<string>();
        }
        private static string Base64Encode(string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }
        private static string GetBearerTokenJson(string consumerKey, string consumerSecret)
        {
            var webrequest = CreateRequest("/oauth2/token");
            webrequest.Headers.Add("Authorization", "Basic " +
                                                    GetBasicAuthToken(consumerKey, consumerSecret));
            WriteRequest(webrequest, "grant_type=client_credentials");

            return ReadResponse(webrequest);
        }
        private static WebRequest CreateRequest(string url)
        {
            var webrequest = WebRequest.Create(ApiBaseUrl + url);
            ((HttpWebRequest)webrequest).UserAgent = "timacheson.com";
            return webrequest;
        }

        private static string GetBasicAuthToken(string consumerKey, string consumerSecret)
        {
            return Base64Encode(consumerKey + ":" + consumerSecret);
        }

        private static void WriteRequest(WebRequest webrequest, string postData)
        {
            webrequest.Method = WebRequestMethods.Http.Post;
            webrequest.ContentType = "application/x-www-form-urlencoded";

            byte[] postDataBytes = Encoding.UTF8.GetBytes(postData);

            webrequest.ContentLength = postDataBytes.Length;

            using (var requestStream = webrequest.GetRequestStream())
            {
                requestStream.Write(postDataBytes, 0, postDataBytes.Length);
                requestStream.Close();
            }
        }

        private static string ReadResponse(WebRequest webrequest)
        {
            using (var responseStream = webrequest.GetResponse().GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        return responseReader.ReadToEnd();
                    }
                }
            }

            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            pathList.Add(binder.Name);
            result = new DynamiTwitterClient(pathList, consumerKey, consumerSecret);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            AppendMimeType();
            var url = BuildUrl(binder, args);
            var token = GetBearerToken();
            result = ExecuteRequest(url, token);
            pathList.Clear();
            return true;
        }

        private string BuildUrl(InvokeMemberBinder binder, object[] args)
        {
            string url;
            if (args == null || !args.Any())
            {
                url = string.Format("/{0}/{1}/{2}", Version, string.Join("/", pathList), binder.Name);
            }
            else
            {
                url = string.Format("/{0}/{1}?{2}={3}", Version, string.Join("/", pathList), binder.Name, args.First());
            }
            return url;
        }

        private string ExecuteRequest(string url, string token)
        {
            var request = CreateRequest(url);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Method = WebRequestMethods.Http.Get;
            return ReadResponse(request);
        }

        private void AppendMimeType()
        {
            var index = Math.Max(0, pathList.Count - 1);
            pathList[index] = pathList[index] + MimeType;
        }


    }
}