namespace RadFramework.Libraries.Configuration.Patching.Arguments
{
    public interface IIncludeContext
    {
        string ConfigRoot { get; }
        string IncludeRoot { get; }
        string[] ResolvedIncludeRoots { get; }
        string OutputRoot { get; }
    }
}