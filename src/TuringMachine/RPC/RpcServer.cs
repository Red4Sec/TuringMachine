using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Compression;
using System.Linq;
using System.Net;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;

namespace TuringMachine.RPC
{
    public class RpcServer : IDisposable
    {
        /// <summary>
        /// Host
        /// </summary>
        private IWebHost _host;

        /// <summary>
        /// EndPoint
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// Server
        /// </summary>
        public FuzzerServer Server { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endPoint">EndPoint</param>
        /// <param name="server">Server</param>
        public RpcServer(IPEndPoint endPoint, FuzzerServer server)
        {
            EndPoint = endPoint;
            Server = server;
        }

        /// <summary>
        /// Start
        /// </summary>
        public void Start()
        {
            Stop();

            _host = WebHost.CreateDefaultBuilder()
                .UseKestrel(options => options.Listen(EndPoint.Address, EndPoint.Port, listenOptions =>
            {
                // TODO: HTTPS
            }))
           .Configure(app =>
           {
               app.UseResponseCompression();
               app
                .UseRouting()
                .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });
           })
           .ConfigureServices(services =>
           {
               services.AddSingleton(this);

               services
                   .AddMvcCore()
                   .AddControllersAsServices()
                   .AddNewtonsoftJson(c =>
                   {
                       SerializationHelper.Configure(c.SerializerSettings);
                   });

               services.AddResponseCompression(options =>
               {
                   // options.EnableForHttps = false;
                   options.Providers.Add<GzipCompressionProvider>();
                   options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
               });

               services.Configure<GzipCompressionProviderOptions>(options =>
               {
                   options.Level = CompressionLevel.Fastest;
               });
           })
           .Build();

            _host.Start();
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            if (_host == null) return;

            using (var task = _host.StopAsync())
            {
                task.Wait();
            }
            _host.Dispose();
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
