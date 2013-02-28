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

            StreamWriter sw = process.StandardInput;
            sw.WriteLine(args[0]);
            sw.Close();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            process.Close();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            return output;
        }
    }
}
