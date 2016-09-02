namespace Itinero.API.FileMonitoring
{
    /// <summary>
    /// A non-generic abstract representation of a files monitor.
    /// </summary>
    public interface IFilesMonitor
    {
        /// <summary>
        /// Starts monitoring.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        void Stop();
    }
}
