using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using NUtilities.Serialization;

namespace NUtilities.Web
{
    public class MultiPartMessage
    {
      public MultipartMessageType type { get; set; }
      public string name { get; set; }
      public string message { get; set; }
      public string fileName { get; set; }
      public string mimeType { get; set; }
    }

    public enum MultipartMessageType
    {
      File,
      FormData,
    }

    public class WebHttpClient : IWebHttpClient
    {
        private string _boundary = Guid.NewGuid().ToString().Replace("-", "");
        private string _baseUri = String.Empty;
        private NetworkCredential _credentials = null;
        private WebProxy _proxy = null;

        private const string NEW_LINE = "\r\n";
        private const int TIMEOUT = 300000;

        public WebHttpClient(string baseUri)
            : this(baseUri, String.Empty, String.Empty, String.Empty)
        {
        }

        public WebHttpClient(string baseUri, string userName, string password)
            : this(baseUri, userName, password, String.Empty)
        {
        }

        public WebHttpClient(string baseUri, string userName, string password, string domain)
        {
            _baseUri = baseUri;

            if (userName != String.Empty && userName != null && domain != String.Empty && domain != null)
            {
                _credentials = new NetworkCredential(userName, password, domain);
            }
            else if (userName != String.Empty && userName != null)
            {
                _credentials = new NetworkCredential(userName, password);
            }
        }

        public WebHttpClient(string baseUri, string proxyUserName, string proxyPassword, string proxyDomain, string proxyHost, int proxyPort)
        {
          _baseUri = baseUri;

          if (proxyHost != String.Empty && proxyHost != null)
          {
            if (proxyUserName != String.Empty && proxyUserName != null && proxyDomain != String.Empty && proxyDomain != null)
            {
              _proxy = new WebProxy(proxyHost, proxyPort);
              _proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword, proxyDomain);
              ;
            }
            else if (proxyUserName != String.Empty && proxyUserName != null)
            {
              _proxy = new WebProxy(proxyHost, proxyPort);
              _proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
            }
          }
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials, string proxyHost, int proxyPort, NetworkCredential proxyCredentials)
        {
          _baseUri = baseUri;

          _credentials = credentials;

          if (proxyHost != String.Empty && proxyHost != null)
          {
            _proxy = new WebProxy(proxyHost, proxyPort);
            _proxy.Credentials = proxyCredentials;
          }
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials, WebProxy webProxy)
        {
          _baseUri = baseUri;

          _credentials = credentials;
          _proxy = webProxy;
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials)
        {
            this._baseUri = baseUri;

            this._credentials = credentials;
        }

        /// <summary>
        /// HttpUtility.UrlEncode does not encode alpha-numeric characters such as _ - . ' ( ) * and !
        /// This function encodes these characters to create a fully encoded uri
        /// </summary>
        private static string FullEncodeUri(string semiEncodedUri)
        {
            string fullEncodedUri = string.Empty;

            fullEncodedUri = semiEncodedUri.Replace("'", "%22");
            fullEncodedUri = fullEncodedUri.Replace("(", "%28");
            fullEncodedUri = fullEncodedUri.Replace(")", "%29");
            fullEncodedUri = fullEncodedUri.Replace("+", "%20");

            return fullEncodedUri;
        }

        public static string GetUri(string relativeUri)
        {
            string semiEncodedUri = string.Empty;
            string encodedUri = string.Empty;
            //.Net does not allow certain characters in a uri, hence the uri has to be encoded 
            semiEncodedUri = HttpUtility.UrlEncode(relativeUri);

            //encode alpha-numberic characters that are not encoded by HttpUtility.UrlEncode
            encodedUri = FullEncodeUri(semiEncodedUri);

            return encodedUri;
        }

        private void PrepareCredentials(WebRequest request)
        {
          if (_credentials == null)
          {
            request.Credentials = CredentialCache.DefaultCredentials;
          }
          else
          {
            request.Credentials = _credentials;
          }
          
          if (_proxy != null)
          {
            request.Proxy = _proxy;
          }
        }

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
          if (Convert.ToBoolean(ConfigurationManager.AppSettings["IgnoreSslErrors"]))
          {
            // allow any certificate...
            return true;
          }
          else
          {
            return policyErrors == SslPolicyErrors.None;
          }
        }

        public T Get<T>(string relativeUri)
        {
            return Get<T>(relativeUri, true);
        }

        public T Get<T>(string relativeUri, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + GetUri(relativeUri);

                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
              
                request.Method = "Get";
                request.Timeout = TIMEOUT;

                // allows for validation of SSL conversations
                //ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                //  ValidateRemoteCertificate
                //);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                T entity = XmlUtility.DeserializeFromStream<T>(response.GetResponseStream(), useDataContractSerializer);

                return entity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP GET request on" + uri + ".", exception);
            }
        }

        public string GetMessage(string relativeUri)
        {
            string uri = _baseUri + relativeUri;  
            try
            {
                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
                
                request.Method = "Get";
                request.Timeout = TIMEOUT;

                // allows for validation of SSL conversations
                //ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                //  ValidateRemoteCertificate
                //);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string message = XmlUtility.SerializeFromStream(response.GetResponseStream());

                return message;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while executing HTTP GET request on" + uri + ".", exception);
            }
        }

        public R Post<T, R>(string relativeUri, T requestEntity)
        {
            return Post<T, R>(relativeUri, requestEntity, true);
        }

        public R Post<T, R>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + GetUri(relativeUri);

                MemoryStream stream = XmlUtility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                R responseEntity = XmlUtility.DeserializeFromStream<R>(response.GetResponseStream(), useDataContractSerializer);

                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public T PostMessage<T>(string relativeUri, string requestMessage, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri; // GetUri(relativeUri);
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                T responseEntity = XmlUtility.DeserializeFromStream<T>(response.GetResponseStream(), useDataContractSerializer);

                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public void PostMultipartMessage(string relativeUri, List<MultiPartMessage> requestMessages)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentType = "multipart/form-data; boundary=" + _boundary;
                request.Method = "POST";
                request.Timeout = TIMEOUT;
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);

                foreach (MultiPartMessage requestMessage in requestMessages)
                {
                  writer.Write("--" + _boundary + NEW_LINE);

                  if (requestMessage.type == MultipartMessageType.File)
                  {
                    writer.Write("Content-Disposition: file; name=\"{1}\"; filename=\"{2}\"{3}", requestMessage.mimeType, requestMessage.name, requestMessage.fileName, NEW_LINE);
                    writer.Write("Content-Type: {0}; {1}", requestMessage.mimeType, NEW_LINE);
                  }
                  else
                  {
                    writer.Write("Content-Disposition: form-data; name=\"{0}\"{1}", requestMessage.name, NEW_LINE);
                  }
                  writer.Flush();

                  writer.Write(NEW_LINE + requestMessage.message);
                  writer.Write(NEW_LINE);
                  writer.Write("--{0}--{1}", _boundary, NEW_LINE);
                  writer.Flush();
                }

                PrepareCredentials(request);

                request.ContentLength = stream.Length;
                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                stream.Close();

                //TODO: return based on http response status
                if (response.StatusCode.ToString().Equals("OK"))
                {

                }
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string Post<T>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + GetUri(relativeUri);

                MemoryStream stream = XmlUtility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseMessage = XmlUtility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }
    }
}
