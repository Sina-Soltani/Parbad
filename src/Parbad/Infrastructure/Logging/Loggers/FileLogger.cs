using System;
using System.IO;
using Parbad.Utilities;

namespace Parbad.Infrastructure.Logging.Loggers
{
    /// <summary>
    /// An abstract logger that saves log into file.
    /// </summary>
    public abstract class FileLogger : Logger
    {
        protected FileLogger(string physicalFolderPath, string fileExtension)
        {
            PhysicalFolderPath = physicalFolderPath ?? throw new ArgumentNullException(nameof(physicalFolderPath));
            FileExtension = fileExtension ?? throw new ArgumentNullException(nameof(fileExtension));

            FileExtension = FileExtension.ToggleStringAtStart(".", false);
        }

        /// <summary>
        /// Physical folder's path on disk to save logs.
        /// </summary>
        public string PhysicalFolderPath { get; }

        public string FileExtension { get; }

        protected internal override void Write(Log log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var contents = GetFileContents(log);

            var logFileName = GenerateFileName(log);

            var logFilePath = Path.Combine(PhysicalFolderPath, logFileName);

            WriteFile(logFilePath, contents);
        }

        /// <summary>
        /// Writes contents into a file in specific path.
        /// </summary>
        /// <param name="filePath">Path of file to write</param>
        /// <param name="contents">Contents to write into file</param>
        protected internal virtual void WriteFile(string filePath, byte[] contents)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }

            using (var stream = File.OpenWrite(filePath))
            {
                stream.Write(contents, 0, contents.Length);
            }
        }

        /// <summary>
        /// Generates a name for creating file.
        /// </summary>
        protected internal virtual string GenerateFileName(Log log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            return $"Log-{log.OrderNumber}-{log.CreatedOn:yyyy-MM-dd-HH-mm-ss}.{FileExtension}";
        }

        /// <summary>
        /// Gets file's contents as an array of bytes.
        /// </summary>
        protected internal abstract byte[] GetFileContents(Log log);
    }
}