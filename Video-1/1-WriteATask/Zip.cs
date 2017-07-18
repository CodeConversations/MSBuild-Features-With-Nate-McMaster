using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;

namespace Zipper.Tasks
{
    public class Zip : Microsoft.Build.Utilities.Task, ICancelableTask
    {
        private bool _cancelled;

        [Required]
        public string Directory { get; set; }

        [Required]
        public string ZipPath { get; set; }

        [Output]
        public int FilesAdded { get; set; }

        public void Cancel()
        {
            _cancelled = true;
        }

        public override bool Execute()
        {
#if DEBUG
            // In Visual Studio or Visual Studio Code, you can add a breakpoint to this file.
            // Then, run MSBuild and use the "Attach to Process" feature to attach to the process
            // ID that this prints to the console.

            // Obviously, remove this when you're finished debugging as it will wait indefinitely
            // for the debugger to attach.
            System.Console.WriteLine("PID = " + System.Diagnostics.Process.GetCurrentProcess().Id);
            while (!System.Diagnostics.Debugger.IsAttached && !_cancelled);
#endif

            var dirInfo = new DirectoryInfo(Directory);
            if (!dirInfo.Exists)
            {
                Log.LogError("Directory {0} does not exist", Directory);
                return false;
            }

            var zipPath = Path.IsPathRooted(ZipPath)
                ? ZipPath
                : Path.Combine(System.IO.Directory.GetCurrentDirectory(), ZipPath);

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(zipPath));

            var count = 0;
            using (var stream = new FileStream(zipPath, FileMode.Create))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    count++;
                    var entryPath = file.FullName.Substring(dirInfo.FullName.Length).Replace('\\', '/');
                    Log.LogMessage("Added '{0}' to archive", entryPath);
                    zip.CreateEntryFromFile(file.FullName, entryPath);
                }
            }

            FilesAdded = count;
            Log.LogMessage(MessageImportance.High, "Created {0}", zipPath);

            return true;
        }
    }
}
