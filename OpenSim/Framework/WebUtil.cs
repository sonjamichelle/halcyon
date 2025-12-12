using System;
using System.Collections.Specialized;
using System.Text;
using OpenMetaverse.StructuredData;
using System.IO;
using System.Net;
using System.Net.Http;

namespace OpenSim.Framework
{
    public static class WebUtil
    {
        public static HttpClient GetNewGlobalHttpClient(int timeout)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(timeout);
            return client;
        }

        public static byte[] OSDToBytes(OSDMap map, bool includeBinary = true)
        {
            return Encoding.UTF8.GetBytes(OSDParser.SerializeJsonString(map));
        }

        public static OSDMap QueryStringToMap(string query)
        {
            OSDMap map = new OSDMap();
            if (string.IsNullOrEmpty(query))
                return map;

            NameValueCollection nvc = System.Web.HttpUtility.ParseQueryString(query);
            foreach (string key in nvc.AllKeys)
                map[key] = OSD.FromString(nvc[key]);
            return map;
        }

        public static string PostToService(string uri, string data)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                return client.UploadString(uri, data);
            }
        }

        public static OSDMap PostToServiceCompressed(string uri, OSDMap args, int timeout)
        {
            byte[] data = OSDToBytes(args, true);
            string resp = PostToServiceCompressedRaw(uri, data, timeout);
            return (OSDMap)OSDParser.DeserializeJson(resp);
        }

        public static string PostToServiceCompressedRaw(string uri, byte[] data, int timeout)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Timeout = timeout;
            request.ContentType = "application/json";
            using (var reqStream = request.GetRequestStream())
                reqStream.Write(data, 0, data.Length);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var respStream = response.GetResponseStream())
            using (var reader = new StreamReader(respStream))
                return reader.ReadToEnd();
        }

        public static OSDMap PostToService(string uri, OSDMap args, int timeout, bool compress)
        {
            if (compress)
                return PostToServiceCompressed(uri, args, timeout);

            string resp = PostToService(uri, OSDParser.SerializeJsonString(args));
            return (OSDMap)OSDParser.DeserializeJson(resp);
        }

        public static string PutToService(string uri, string data)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                return client.UploadString(uri, "PUT", data);
            }
        }

        public static OSDMap PutToServiceCompressed(string uri, OSDMap args, int timeout)
        {
            byte[] data = OSDToBytes(args, true);
            string resp = PutToServiceCompressedRaw(uri, data, timeout);
            return (OSDMap)OSDParser.DeserializeJson(resp);
        }

        public static OSDMap PutToService(string uri, OSDMap args, int timeout)
        {
            string resp = PutToService(uri, OSDParser.SerializeJsonString(args));
            return (OSDMap)OSDParser.DeserializeJson(resp);
        }

        public static string PutToServiceCompressedRaw(string uri, byte[] data, int timeout)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "PUT";
            request.Timeout = timeout;
            request.ContentType = "application/json";
            using (var reqStream = request.GetRequestStream())
                reqStream.Write(data, 0, data.Length);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var respStream = response.GetResponseStream())
            using (var reader = new StreamReader(respStream))
                return reader.ReadToEnd();
        }

        public static string ServiceOSDRequest(string uri, string method, OSDMap map)
        {
            string payload = OSDParser.SerializeJsonString(map);
            if (method == "POST")
                return PostToService(uri, payload);
            if (method == "PUT")
                return PutToService(uri, payload);
            return string.Empty;
        }

        public static OSDMap ServiceOSDRequest(string uri, OSDMap args, string method, int timeout, bool compress, bool async, bool throwOnError)
        {
            switch (method.ToUpperInvariant())
            {
                case "POST":
                    return PostToServiceCompressed(uri, args, timeout);
                case "PUT":
                    return PutToServiceCompressed(uri, args, timeout);
                case "DELETE":
                    // Minimal DELETE support: just send empty body.
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(uri);
                        request.Method = "DELETE";
                        request.Timeout = timeout;
                        using (var response = (HttpWebResponse)request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string resp = reader.ReadToEnd();
                            if (string.IsNullOrEmpty(resp))
                                return new OSDMap();
                            return (OSDMap)OSDParser.DeserializeJson(resp);
                        }
                    }
                    catch
                    {
                        return new OSDMap();
                    }
                default:
                    return new OSDMap();
            }
        }
    }
}
