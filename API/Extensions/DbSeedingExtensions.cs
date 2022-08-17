using API.Data;
using API.Data;

namespace API.Extensions
{
    public static class DbSeedingExtensions
    {
        public static IApplicationBuilder UseSeedDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<DataContext>();
                Seed.SeedUsers(context);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return app;
        }
    }
}
