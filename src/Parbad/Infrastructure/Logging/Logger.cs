namespace Parbad.Infrastructure.Logging
{
    /// <summary>
    /// Abstract logger that logs payment operations.
    /// </summary>
    public abstract class Logger
    {
        /// <summary>
        /// Writes log.
        /// </summary>
        protected internal abstract void Write(Log log);

        /// <summary>
        /// Reads logs from specific page number with specific page size.
        /// </summary>
        protected internal abstract LogReadContext Read(int pageNumber, int pageSize);
    }
}