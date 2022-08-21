using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OverScriptStandard
{
    public static class Dirs
    {
        static public string OverScriptDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static public string ModulesDir = Path.Combine(OverScriptDir, "modules");
    }

    public enum TypeID : byte
    {
        None, Void, Bool, Byte, Short, Int, Long, Float, Double, Decimal, String, Char, Date, Object, Custom,
        BoolArray, ByteArray, ShortArray, IntArray, LongArray, FloatArray, DoubleArray, DecimalArray, StringArray, CharArray, DateArray, ObjectArray, CustomArray,

        Type, BasicType, CustomType, Array, OfRefType, Empty, HintType, HintArrayType,

        BoolType, ByteType, ShortType, IntType, LongType, FloatType, DoubleType, DecimalType, StringType, CharType, DateType, ObjectType,
        BoolArrayType, ByteArrayType, ShortArrayType, IntArrayType, LongArrayType, FloatArrayType, DoubleArrayType, DecimalArrayType, StringArrayType, CharArrayType, DateArrayType, ObjectArrayType
    }

    public static class ExecPool
    {
        public static int Capacity = 64;
        internal static Executor[] Executors = new Executor[Capacity];
        internal static int LastID = -1;
        internal static int GetVacantID(Executor exec)
        {
            int n = LastID;
            bool fromStart = false;
            do
            {
                n++;
                if (n >= Capacity) { n = 0; fromStart = true; }
                if (n > LastID && fromStart) throw new InvalidOperationException($"The maximum number of executors is used ({Capacity}).");


            } while (Executors[n] != null);

            LastID = n;
            Executors[n] = exec;
            return n;
        }
    }


    public class CodeFile
    {
        public string File;
        public string[] Lines;
        public string Base;
        public int Num;
        public CodeFile(string file, int num, Script script)
        {
            File = file;
            Lines = new string[0];
            Base = script.ScriptDir;
            Num = num;
        }
    }


    public static class StringExtension
    {

        public static int EIndexOf(this string text, string str, int pos=0, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            if (pos < 0 || pos >= text.Length) return -1;

            if(stringComparison!= StringComparison.InvariantCulture)
                return text.IndexOf(str, pos, stringComparison);
            else
            {
                text = RemLocChars(text);
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, str, pos, CompareOptions.IgnoreNonSpace);
            }
                
        }
     
        public static int ELastIndexOf(this string text, string str, int pos=-1, StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            if (pos == -1) pos = text.Length - 1;
            if (pos < 0 || pos >= text.Length) return -1;
            if (stringComparison != StringComparison.InvariantCulture)
                return text.LastIndexOf(str, pos, stringComparison);
            else
            {
                text = RemLocChars(text);
                return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(text, str, pos, CompareOptions.IgnoreNonSpace);
            }
        }


        public static bool EStartsWith(this string str, string value)
        {
         
            str = RemLocChars(str);
            return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(str, value, CompareOptions.IgnoreNonSpace);
        }
    
        const char ZeroChar= (char)0;
        static string RemLocChars(string s)
        {
            return s.Replace(ScriptClass.CodeLocationChar, ZeroChar).Replace(ScriptClass.LM_0, ZeroChar).Replace(ScriptClass.LM_1, ZeroChar);
        }


    }



    class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(string message) : base(ScriptClass.RestoreCode(message)) { }

    }
    public class ScriptLoadingException : Exception
    {
        public ScriptLoadingException(string message) : base(ScriptClass.RestoreCode(message)) { }
    }
    class CustomThrownException : Exception
    {
        public CustomThrownException(string message) : base(message) { }
        public CustomThrownException() : base() { }
    }
    class InvalidByRefArgumentException : Exception
    {
        public InvalidByRefArgumentException(string message) : base(ScriptClass.RestoreCode(message)) { }
    }

    class ExecutingCanceledException : Exception
    {
        public ExecutingCanceledException() : base("Executing is canceled.") { }
        public ExecutingCanceledException(string message) : base(message) { }
        public static Exception GetCanceledException(bool forced, string msg = null)
        {
            return forced ? new ExecutingForciblyCanceledException(msg) : new ExecutingCanceledException(msg);
        }
    }
    class ExecutingForciblyCanceledException : ExecutingCanceledException
    {
        public ExecutingForciblyCanceledException() : base("Executing is forcibly canceled.") { }
        public ExecutingForciblyCanceledException(string message) : base(message) { }
    }
    class FinalizationFailedException : Exception
    {
        public FinalizationFailedException(string message, Exception inner) : base(message, inner) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ImportAttribute : System.Attribute { }





    public static class ExceptionVarName
    {
        public static string TypeName = "exName";
        public static string Message = "exMessage";
        public static string CustomExObj = "exObject";
        public static string Object = "exception";
        public static string StackTrace = "stackTrace";

        public static string NameVarInCustomExClass = "Name";
        public static string MessageVarInCustomExClass = "Message";
    }


}
