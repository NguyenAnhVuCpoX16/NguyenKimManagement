using NKCManagement.Interface;
using NKCManagement.Utils;

namespace NKCManagement
{
    public static class AppStatic
    {
        public static bool WorkerMode { get; set; } = false;
        public static List<IAiService> AI { get; set; } = GetAI();
        public static AIConfig AIKey { get; set; } = new();

        public static string AIPlatform { get;set; } = "Groq";

        public static int AIProcess {  get; set; } = 0;

        private static List<IAiService> GetAI()
        {
            var assemblies = AssembliesUtil.GetAspNetAssemblies();
            return assemblies.GetInstances<IAiService>().ToList();
        }
    }

    public class AIConfig
    {
        public string ChatGpt { get; set; } = string.Empty;
        public string Groq { get; set; } = string.Empty;
        public string Gemini { get; set; } = string.Empty;
    }
}
