using System.IO;

namespace LibTool
{
    static class Config
    {
        public enum RelativePathOptions
        {
            Root,
            File
        }

        public static bool Override { get; set; } = false;

        public static bool DefaultInRoot { get; set; } = true;

        public static string RootPath { get; set; } = Directory.GetCurrentDirectory();

        public static RelativePathOptions RelativePath { get; set; } = RelativePathOptions.File;
    }
}
