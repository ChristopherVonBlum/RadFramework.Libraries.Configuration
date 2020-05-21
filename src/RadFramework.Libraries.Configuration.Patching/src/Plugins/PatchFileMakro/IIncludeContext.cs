namespace RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro
{
    public interface IIncludeContext
    {
        string ConfigRoot { get; }
        string IncludeRoot { get; }
        string[] ResolvedIncludeRoots { get; }
        string OutputRoot { get; }
    }
}