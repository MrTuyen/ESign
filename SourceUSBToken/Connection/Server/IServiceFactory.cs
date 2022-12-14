namespace WebSockets.Server
{
    /// <summary>
    /// Implement this to decide what connection to use based on the http header
    /// </summary>
    public interface IServiceFactory
    {
        IService CreateInstance(ConnectionDetails connectionDetails);
    }
}
