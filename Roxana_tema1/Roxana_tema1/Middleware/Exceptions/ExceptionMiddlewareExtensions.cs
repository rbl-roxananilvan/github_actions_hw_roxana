namespace Roxana_tema1.Middleware.Exceptions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler();
        }
            
    }
}
