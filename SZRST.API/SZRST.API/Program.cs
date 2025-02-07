using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace SZRST.WebApi
{
    public class Program
    {

        
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
           
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>()
                           .UseUrls("https://localhost:5001"); // Specify the HTTPS URL
             });

    }
}
