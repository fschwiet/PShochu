using System;
using System.IO;
using System.Text;

namespace PShochu
{
    public class PsakeUtil
    {
        public static string GetPowershellCommand(string moduleLocation, string scriptPath, string taskName)
        {
            var psakeScriptPath = new FileInfo(scriptPath).FullName;

            StringBuilder arguments = new StringBuilder();
            arguments.Append(String.Format(@"import-module ""{0}"";", moduleLocation));
            arguments.Append(String.Format(@"invoke-psake ""{0}"" {1};", psakeScriptPath, taskName));

            return "powershell -NoProfile -Noninteractive -EncodedCommand " + Base64Encode(arguments.ToString());
        }

        public static string Base64Encode(string input)
        {
            var bytes = System.Text.Encoding.Unicode.GetBytes(input);
            return System.Convert.ToBase64String(bytes);
        }
    }
}