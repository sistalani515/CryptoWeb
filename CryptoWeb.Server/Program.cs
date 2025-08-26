using CryptoWeb.Shared.DI;
using CryptoWeb.Shared.Models.Databases;
namespace CryptoWeb.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.InjectServer(builder.Configuration, "CryptoWeb.Server");
            builder.Services.SetSwagger();
            builder.Services.InjectFaucet();
            builder.Services.AddCFProxy(builder.Configuration!);
            var app = builder.Build();
            //app.MapGet("/", () => "OK");
            app.MapGet("/", () => Results.Ok("Healthy"));

            var dbInit = new AppDbContextInitializer(app.Services);
            dbInit.Initialize();


            app.SetUseSwagger();

            app.MapControllers();

            app.Run();
        }
    }
}
