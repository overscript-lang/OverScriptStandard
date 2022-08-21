using System;
using System.IO;
using System.Linq;
using OverScriptStandard;

namespace OverScript
{
    class Program
    {

        static System.Reflection.Assembly ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        static public string OverScriptDir = Path.GetDirectoryName(ExecutingAssembly.Location);
        static public string ModulesDir = Path.Combine(Program.OverScriptDir, "modules");

        static int LoadingStatusCurPos = 0;

        static void Main(string[] args)
        {
            Console.Title = "OverScript";
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhException);
            ExecPool.Capacity = 1;

            string scriptFile;
            string[] scriptArgs;

            if (args.Length > 0)
            {
                scriptFile = Path.GetFullPath(args[0]);
                scriptArgs = args.Skip(1).ToArray();
            }
            else
            {
                Console.WriteLine("Welcome to OverScript v" + ExecutingAssembly.GetName().Version + ". See more at overscript.org.");
                Console.Write("Enter app file: ");
                string cl = Console.ReadLine();
                if (cl.Length < 1) throw new FileLoadException("App file not specified.");


                var scriptToRun = ScriptClass.GetFileAndArgs(cl);
                scriptFile = scriptToRun.file;
                scriptArgs = scriptToRun.args;
            }


            string code = System.IO.File.ReadAllText(scriptFile, System.Text.Encoding.UTF8);



            Console.CursorVisible = false;
            Console.Clear();

            string loadingStr = $"{Path.GetFileName(scriptFile)} loading";
            LoadingStatusCurPos = loadingStr.Length + 1;
            if (LoadingStatusCurPos >= Console.WindowWidth) LoadingStatusCurPos = Console.WindowWidth - 1;

            Console.Write(loadingStr + " ...");


            Script script = new Script(scriptFile, LoadingProgressChanged);
            Console.Clear();

            string scriptName = script.AppInfo["Name"];
            Console.Title = scriptName;

            string currentCulture;
            script.AppInfo.TryGetValue("CurrentCulture", out currentCulture);
            if (currentCulture != null)
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(currentCulture);



            Console.CursorVisible = true;

            Executor exec = new Executor(script);
            exec.Execute(scriptArgs);
        }

        public static void LoadingProgressChanged(object sender, int step)
        {
            Console.SetCursorPosition(LoadingStatusCurPos, 0);

            Console.WriteLine(step < 0 ? "completed." : $"[{new string('#', step).PadRight(Script.LoadingSteps, '-')}]");
        }


        static void UnhException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            string s = "";

            if (e.Data.Contains(ExceptionVarName.TypeName))
                s += (e.Data[ExceptionVarName.TypeName] ?? "Null_exception_name") + ": " + (e.Data[ExceptionVarName.Message] ?? "Null_exception_message") + Environment.NewLine;
            else
            {
                s = e.GetType().Name + ": " + e.Message;
                Exception ie = e.InnerException;
                while (ie != null)
                {
                    s += " ---> " + ie.GetType().Name + ": " + ie.Message;
                    ie = ie.InnerException;
                }
                s += Environment.NewLine;
            }
            if (e.Data.Contains(ExceptionVarName.StackTrace)) s += e.Data[ExceptionVarName.StackTrace] + Environment.NewLine;

            Console.WriteLine(s);
            Console.WriteLine("Interpreter stack trace:");
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Environment.Exit(Environment.ExitCode);
        }

    }
}
