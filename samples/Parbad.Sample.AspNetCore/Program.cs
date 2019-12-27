using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Parbad.Sample.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Run();
        }

        public static IHost CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
                .Build();
    }
}
