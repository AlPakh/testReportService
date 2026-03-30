using ConversionService.Infrastructure.DependencyInjection;
using Npgsql;

namespace ConversionService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string baseConnectionString =
                builder.Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            string dbPassword =
                builder.Configuration["DbPassword"]
                ?? throw new InvalidOperationException("Database password is not configured.");

            var csBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
            {
                Password = dbPassword
            };

            builder.Configuration["ConnectionStrings:Postgres"] = csBuilder.ConnectionString;

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}