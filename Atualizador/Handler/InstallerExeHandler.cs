using System.Diagnostics;
using System.Text;

namespace UpdaterService.Handler
{
    public class InstallerExeHandler
    {
        private StringBuilder commandResult = new StringBuilder();
        public string ComandResult
        {
            get
            {
                return commandResult.ToString();
            }
        }

        private StringBuilder commandErrors = new StringBuilder();
        public string CommandErrors
        {
            get
            {
                return commandErrors.ToString();
            }
        }

        public InstallerExeHandler(string cmd, string[] arguments)
        {
            try
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo(cmd);
                process.StartInfo.Arguments = string.Join(" ", arguments);

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.OutputDataReceived += p_OutputDataReceived;
                process.ErrorDataReceived += p_ErrorDataReceived;

                process.Start();


                
                commandResult.Append(process.StandardOutput.ReadToEnd());
                commandErrors.Append(process.StandardError.ReadToEnd());

                process.WaitForExit();

                process.Close();
            }
            catch (Exception err)
            {
                commandErrors.AppendLine($"Error: {err.Message}");
            }
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            commandErrors.AppendLine(e.Data);
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null)
                return;
            commandResult.AppendLine(e.Data);
        }
    }
}
