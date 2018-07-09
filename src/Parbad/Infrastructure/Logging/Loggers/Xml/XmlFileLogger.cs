using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parbad.Utilities;

namespace Parbad.Infrastructure.Logging.Loggers.Xml
{
    /// <inheritdoc />
    /// <summary>
    /// A logger that saves logs as XML files.
    /// </summary>
    public class XmlFileLogger : FileLogger
    {
        /// <summary>
        /// Initializes XmlFileLogger
        /// </summary>
        /// <param name="physicalFolderPath">Physical folder's path on disk to save XML files.</param>
        public XmlFileLogger(string physicalFolderPath) : base(physicalFolderPath, "xml")
        {
        }

        protected internal override byte[] GetFileContents(Log log)
        {
            return XmlHelper.Serialize(log);
        }

        protected internal override LogReadContext Read(int pageNumber, int pageSize)
        {
            var files = Directory.GetFiles(PhysicalFolderPath, "*.xml");

            if (files.Length == 0)
            {
                return LogReadContext.Empty;
            }

            int skip = (pageNumber - 1) * pageSize;

            var selectedFiles = files.Skip(skip).Take(pageSize);

            var logs = new List<Log>();

            foreach (var file in selectedFiles)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Log log;

                    if (XmlHelper.TryDeserialize(stream, out log))
                    {
                        logs.Add(log);
                    }
                }
            }

            return new LogReadContext(files.Length, logs);
        }
    }
}