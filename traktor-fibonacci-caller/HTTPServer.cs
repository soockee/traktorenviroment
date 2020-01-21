// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
// Modified by Simon Stockhause

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;


namespace Httpserver
{
    class SimpleHTTPServer
    {
        private readonly string[] _indexFiles = { 
            "index.html", 
            "index.htm", 
            "default.html", 
            "default.htm" 
        };
        
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
            #region extension to MIME type list
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            {".zip", "application/zip"},
            #endregion
        };
        private static readonly HttpClient httpclient = new HttpClient();
        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;
        string fibo_ip;
        string fibo_port;
    
        public int Port
        {
            get { return _port; }
            private set { }
        }
    
        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public SimpleHTTPServer(string path, int port)
        {
            this.Initialize(path, port);
        }
    
        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        public SimpleHTTPServer(string path)
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(path, port);
        }
    
        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }
    
        private void Listen()
        {
            
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
                _listener.Start();
                while (true)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        Process(context);
                    }
                    catch (Exception ex)
                    {
    
                    }
                }
        }
    
        internal class JsonObject
        {       
            public int N { get; set; }
        }
        private void Process(HttpListenerContext context)
        {
            using (var scope = Program.tracer.BuildSpan("Process Context").StartActive())
            {
                Console.WriteLine("Reading Request");
                var body = new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();       
                Console.WriteLine(body);
                var json = JsonSerializer.Deserialize<JsonObject>(body);
                Console.WriteLine("JSON.N: " + json.N);                     
                Console.WriteLine("Starting Request to Server: " +fibo_ip+":"+fibo_port);
                string response = SendRequest(fibo_ip,fibo_port,json);
                Console.WriteLine("Building Response");
                context = BuildRespose(context, response);
                Console.WriteLine("Closing Response");
                context.Response.Close();
            }
        }
        
    
        private void Initialize(string path, int port)
        {
        
                this._rootDirectory = path;
                this._port = port;
                fibo_ip = Environment.GetEnvironmentVariable("FIBO_IP");
                fibo_port = Environment.GetEnvironmentVariable("FIBO_PORT");
                _serverThread = new Thread(this.Listen);
                _serverThread.Start();
 
        }
        private HttpListenerContext BuildRespose(HttpListenerContext context, string message) 
        {
            byte[] b = Encoding.UTF8.GetBytes(message);
            context.Response.StatusCode = 200;
            context.Response.KeepAlive = false;
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            return context;
        }
        private string SendRequest(string serverip,string serverport, JsonObject jsonobject) 
        {
            var url = serverip + ":" + serverport;
            var content = new StringContent(jsonobject.ToString(), Encoding.UTF8, "application/json");
            Console.WriteLine("Sending Content: " + content.ReadAsStringAsync().Result;
            var response = SimpleHTTPServer.httpclient.PostAsync(url,content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseContent);
            return responseContent;
        }
    }
}