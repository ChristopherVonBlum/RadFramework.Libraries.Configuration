using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RadFramework.Libraries.Configuration.Patching.Logging;
using RadFramework.Libraries.Configuration.Patching.Plugins;
using RadFramework.Libraries.Configuration.Patching.Plugins.PatchFileMakro;
using RadFramework.Libraries.Configuration.Patching.Plugins.TargetingInformationMakro;
using RadFramework.Libraries.Configuration.Patching.TransformationRunner.Configuration;
using RadFramework.Libraries.Configuration.Patching.TransformationRunner.FileSystem;
using RadFramework.Libraries.Ioc;

namespace RadFramework.Libraries.Configuration.Patching.TransformationRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                InitConfig initConfig;

                Console.WriteLine();

                if (args.Any())
                {
                    initConfig = InitFromCommandLine(args);
                }
                else
                {
                    initConfig = InitFromFile();
                }

                if (initConfig.Debug)
                {
                    Console.WriteLine("Continue? (y/n)");

                    if (Console.ReadKey().KeyChar != 'y')
                    {
                        return -1;
                    }
                }

                return CreateBuilder(initConfig)
                    .Build(initConfig.IncludeContext)
                    ? 0
                    : -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return -1;
            }
        }

        private static InitConfig InitFromFile()
        {
            Console.WriteLine("Initializing from init.json...");

            InitConfig initConfig = JsonConvert.DeserializeObject<InitConfig>(File.ReadAllText("init.json"));

            initConfig.IncludeContext.ResolvedIncludeRoots = Directory.GetDirectories(Path.GetFullPath(initConfig.IncludeContext.IncludeRoot), "$config", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

            return initConfig;
        }


        private static InitConfig InitFromCommandLine(string[] args)
        {
            Console.WriteLine("Initializing from command line...");

            InitConfig initConfig = new InitConfig
            {
                IncludeContext = new IncludeContext
                {
                    ConfigRoot = args[0], // environment lookups
                    IncludeRoot = args[1], // solution root
                    OutputRoot = args[2], // output folder
                    ResolvedIncludeRoots = args.Skip(3).OrderBy(p => p).ToArray() // project include dirs
                }
            };

            return initConfig;
        }

        private static ConfigPatcher CreateBuilder(InitConfig initConfig)
        {
            IServiceProvider container = CreateServiceContainer(initConfig);;

            var patcher = container.GetService(typeof(ConfigPatcher));
            
            return (ConfigPatcher)patcher;
        }

        private static Container CreateServiceContainer(InitConfig initConfig)
        {
            if (Debugger.IsAttached)
            {
                initConfig.Debug = true;
            }

            Container services = new Container();

            services.RegisterSingleton<ILogMessageSink, ConsoleLogMessageSink>();

            services.RegisterSingletonInstance<IEnvironementDescription>(
                (object) new JsonEnvironmentDescriptionConfig(
                    Path.Combine(initConfig.IncludeContext.ConfigRoot, "servers.json"),
                    initConfig.IncludeContext.OutputRoot));

            services.RegisterSingletonInstance<IPatchingTemplateProvider>(
                new PatchingTemplateProvider(
                    Path.Combine(initConfig.IncludeContext.ConfigRoot, "environments"), ".json"));

            services.RegisterSingleton<IPatchCollector, PatchCollector>();
            services.RegisterSingleton<JsonTransformationEngine, JsonTransformationEngine>();

            services.RegisterSingletonInstance(new PluginIncludeConditionDelegate[]
            {
                ConfigPatcher.PluginIncludeConditionDelegate
            });

            services.RegisterSingleton<JsonPatchFileMakroPlugin, JsonPatchFileMakroPlugin>();

            services.RegisterSemiAutomaticSingleton<ITransformationEnginePlugin[]>(c => new ITransformationEnginePlugin[]
            {
                c.Resolve<JsonPatchFileMakroPlugin>(),
                new TargetingInformationMakroPlugin()
            });

            services.RegisterSingletonInstance((IInitConfig)initConfig);
            services.RegisterSingleton<ConfigPatcher>();

            return services;
        }
    }
}
