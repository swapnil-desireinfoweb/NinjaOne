using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NinjaOne.DataExtractUtility
{
    public class NinjaApi
    {
        #region static fields
        public static String API_HOST = "https://api.ninjarmm.com";
        #endregion

        #region Properties
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        #endregion

        public string GetData(string URL_PATH)
        {
            string response = DoRequest("GET", URL_PATH);
            return response;
        }
        private string DoRequest(string httpMethod, string path, string contentType = null)
        {
            string contentMD5 = null;
            DateTime requestDate = DateTime.UtcNow;
            string stringToSign = GetStringToSign(httpMethod, contentMD5, contentType, requestDate, path);
            string signature = GetSignature(SecretAccessKey, stringToSign);
            string url = API_HOST + path;
            string responseText = null;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = httpMethod;
            request.ContentType = contentType;
            request.Headers.Add("Authorization", "NJ " + AccessKeyId + ":" + signature);
            request.Headers.Add("x-nj-date", RFC1123_DATE_TIME_FORMATTER(requestDate));
            request.ProtocolVersion = HttpVersion.Version11;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
            {
                responseText = reader.ReadToEnd();
            }

            return responseText;
        }
        private string RFC1123_DATE_TIME_FORMATTER(DateTime requestDate)
        {
            var usCulture = new CultureInfo("en-US");
            var str = requestDate.ToString("ddd, dd MMM yyyy HH:mm:ss \'GMT\'", usCulture);
            return str;
        }
        private string GetStringToSign(String httpMethod, String contentMD5, String contentType, DateTime requestDate, String canonicalPath)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((httpMethod + "\n"));
            stringBuilder.Append((contentMD5 != null ? contentMD5 + "\n" : "\n"));
            stringBuilder.Append((contentType != null ? contentType + "\n" : "\n"));
            stringBuilder.Append(RFC1123_DATE_TIME_FORMATTER(requestDate) + "\n");
            stringBuilder.Append(canonicalPath);

            return stringBuilder.ToString();
        }
        private string GetSignature(String secretAccessKey, String stringToSign)
        {
            var enc = Encoding.UTF8;

            var stringToSignBytes = System.Text.Encoding.UTF8.GetBytes(stringToSign);
            string encodedString = System.Convert.ToBase64String(stringToSignBytes).Replace("\n", "").Replace("\r", "");

            HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(secretAccessKey));
            hmac.Initialize();

            byte[] hmacBytes = hmac.ComputeHash(enc.GetBytes(encodedString));

            var signature = System.Convert.ToBase64String(hmacBytes).Replace("\n", "");

            return signature;
        }
    }
}