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
using TuringMachine.RPC.Controllers;

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
		/// HTTPS Certificate File
		/// </summary>
		public string HTTPSCertFile { get; }

		/// <summary>
		/// HTTPS Certificate Password
		/// </summary>
		public string HTTPSCertificatePassword { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="endPoint">EndPoint</param>
		/// <param name="server">Server</param>
		/// <param name="httpsCertFile">HTTPS certificate file</param>
		/// <param name="httpsPassword">HTTPS certificate password</param>
		public RpcServer(IPEndPoint endPoint, FuzzerServer server, string httpsCertFile, string httpsPassword)
		{
			EndPoint = endPoint;
			Server = server;
			HTTPSCertFile = httpsCertFile;
			HTTPSCertificatePassword = httpsPassword;
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
				if (string.IsNullOrEmpty(HTTPSCertFile)) return;

				listenOptions.UseHttps(HTTPSCertFile, HTTPSCertificatePassword, httpsConnectionAdapterOptions =>
				{
					//if (trustedAuthorities is null || trustedAuthorities.Length == 0)
					//	return;
					//httpsConnectionAdapterOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
					//httpsConnectionAdapterOptions.ClientCertificateValidation = (cert, chain, err) =>
					//{
					//	if (err != SslPolicyErrors.None)
					//		return false;
					//	X509Certificate2 authority = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
					//	return trustedAuthorities.Contains(authority.Thumbprint);
					//};
				});
			}))
		   .Configure(app =>
		   {
			   app.UseSwagger();
			   app.UseSwaggerUI(c =>
			   {
				   c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuringMachine API V1");
			   });

			   app.UseResponseCompression();
			   app.UseRouting();
			   app.UseEndpoints(endpoints => endpoints.MapControllers());
		   })
		   .ConfigureServices(services =>
		   {
			   services.AddSingleton(this);
			   services.AddSingleton<ConfigsController, ConfigsController>();
			   services.AddSingleton<ConnectionsController, ConnectionsController>();
			   services.AddSingleton<InputsController, InputsController>();

			   services.AddSwaggerGen(c =>
			   {
				   c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "TuringMachine API", Version = "v1" });
			   });

			   services
				   .AddMvcCore()
				   .AddApiExplorer()
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
