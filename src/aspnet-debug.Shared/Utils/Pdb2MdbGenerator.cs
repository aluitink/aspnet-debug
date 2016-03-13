using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using aspnet_debug.Shared.Logging;
using log4net;

namespace aspnet_debug.Shared.Utils
{
    public class Pdb2MdbGenerator
    {
        private readonly ILog _logger = Log.GetLogger(typeof (Pdb2MdbGenerator));

        internal void GeneratePdb2Mdb(string directoryName)
        {
            _logger.DebugFormat("Directory: {0}", directoryName);
            IEnumerable<string> files =
                Directory.GetFiles(directoryName, "*.dll")
                    .Concat(Directory.GetFiles(directoryName, "*.exe"))
                    .Where(x => !x.Contains("vshost"));
            _logger.DebugFormat("Files: {0}", files.Count());

            var dirInfo = new DirectoryInfo(directoryName);

            Parallel.ForEach(files, file =>
            {
                try
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    string pdbFile = Path.Combine(Path.GetDirectoryName(file), fileNameWithoutExt + ".pdb");
                    if (File.Exists(pdbFile))
                    {
                        _logger.DebugFormat("Generate mdp for: {0}", file);
                        var procInfo = new ProcessStartInfo(MonoUtils.GetPdb2MdbPath(), Path.GetFileName(file));
                        procInfo.WorkingDirectory = dirInfo.FullName;
                        procInfo.UseShellExecute = false;
                        procInfo.CreateNoWindow = true;
                        Process proc = Process.Start(procInfo);
                        proc.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to generate mdp for " + file, ex);
                }
            });

            _logger.Debug("Transformed Debuginformation pdb2mdb");
        }
    }
}