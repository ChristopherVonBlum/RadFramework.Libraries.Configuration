﻿namespace RadFramework.Libraries.Configuration.Patching.Arguments
{
    public class IncludeContext : IIncludeContext
    {
        public string ConfigRoot { get; set; }
        public string IncludeRoot { get; set; }
        public string[] ResolvedIncludeRoots { get; set; }
        public string OutputRoot { get; set; }
    }
}