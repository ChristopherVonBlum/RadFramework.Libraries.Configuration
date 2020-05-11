namespace RadFramework.Abstractions.Configuration
{
    public interface IConfigurationProvider
    {
        TSection GetSection<TSection>();
    }
}