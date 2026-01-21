using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TimescaleManager.Mappers;
using Domain.RepositoryAbstractions;
using TimescaleManager.ServiceAbstractions;
using TimescaleManager.Services;
using TimescaleManager.Settings;

namespace TimescaleManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var applicationSettings = builder.Configuration.Get<ApplicationSettings>() 
                ?? throw new NullReferenceException();
            builder.Services.AddSingleton(applicationSettings).AddDbContext<DatabaseContext>(
                optionsBuilder => optionsBuilder.UseNpgsql(applicationSettings.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IFileRepository, FileRepository>();
            builder.Services.AddScoped<IValueRepository, ValueRepository>();

            //Services
            builder.Services.AddTransient<IFileService, FileService>();
            builder.Services.AddTransient<IValueService, ValueService>();
            builder.Services.AddTransient<IResultService, ResultService>();

            //Mappers
            builder.Services.AddTransient<ITimescaleValueMapper, TimescaleValueMapper>();
            builder.Services.AddTransient<ITimescaleResultMapper, TimescaleResultMapper>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //Очищаем датабазу при запуске
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();
                }
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
