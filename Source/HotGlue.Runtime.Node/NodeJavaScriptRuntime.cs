using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HotGlue.Runtime.Node
{
    public class NodeJavaScriptRuntime : IJavaScriptRuntime
    {
        private StringBuilder libraries;
        private string _temp;
        private string _node;

        public NodeJavaScriptRuntime()
        {
            libraries = new StringBuilder();
            var builder = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _node = Path.Combine(Path.GetDirectoryName(builder.Path), "node.exe");
            _temp = Path.GetTempFileName();
        }

        public void LoadLibrary(string code)
        {
            using (var fw = new StreamWriter(_temp))
            {
                fw.WriteLine(code);
                fw.WriteLine(@"
var code = '';
var stdin = process.openStdin();

stdin.on('data', function(buffer) {
  if (buffer) code += buffer.toString();
});

stdin.on('end', function() {
  console.log(hotglue_compile(code));
});
");
            }
        }

        public string Execute(string functionName, params object[] args)
        {
            ProcessStartInfo start = new ProcessStartInfo(_node, _temp);
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.ErrorDialog = false;

            var process = Process.Start(start);
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;
                output.AppendLine(e.Data);
            };
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null) return;
                error.AppendLine(e.Data);
            };
            process.BeginErrorReadLine();

            StreamWriter sw = process.StandardInput;
            sw.WriteLine(args[0]);
            sw.Close();

            process.WaitForExit();
            process.Close();

            if (error.Length > 0)
            {
                throw new Exception(error.ToString());
            }

            return output.ToString();
        }
    }
}
