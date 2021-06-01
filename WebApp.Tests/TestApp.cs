using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApp.Tests
{
    class TestApp : Application
    {
        public static readonly string WebRoot = Path.Combine(RootDirectory, "wwwroot");

        /// <summary>
        /// Entry Point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
            => await (new TestApp()).Run(args);

        /// <summary>
        /// Prepare WebHost for the Application.
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="App"></param>
        /// <returns></returns>
        protected override IWebHostBuilder Prepare(Initialization Host, IWebHostBuilder App)
        {
            Host.Using((C, X) =>
            {
                X.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(WebRoot),
                    RequestPath = ""
                });
            });

            App.UseWebRoot("wwwroot");
            App.UseContentRoot("wwwroot");

            return App.UseUrls("http://*:80");
        }
    }
}
