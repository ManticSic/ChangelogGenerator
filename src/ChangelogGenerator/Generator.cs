using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

using ChangelogGenerator.Logging;
using ChangelogGenerator.Verbs;
using ChangelogGenerator.Verbs.New;

using CommandLine;

using Octokit;
using Octokit.Internal;

using Unity;

using UnityContainerAttributeRegistration;
using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator
{
    [RegisterType]
    internal class Generator
    {
        private const string ProductName = "ChangelogGenerator";

        private readonly string[]        args;
        private readonly IUnityContainer container;

        public Generator(string[] args, IUnityContainer container)
        {
            this.args      = args;
            this.container = container;
        }

        private static void Main(string[] args)
        {
            IUnityContainer container = new UnityContainerPopulator().Populate();

            container.RegisterType<IFileSystem, FileSystem>();

            container.RegisterInstance<IEnvironmentAbstraction>(new EnvironmentAbstraction(Environment.Exit));
            container.RegisterInstance("stdout", Console.Out);
            container.RegisterInstance("stderr", Console.Error);
            container.RegisterInstance(args);

            container.RegisterFactory<IGitHubClient>(GitHubClientFactory);

            Generator generator = container.Resolve<Generator>();
            generator.Run();
        }

        private static IGitHubClient GitHubClientFactory(IUnityContainer container)
        {
            Options            options            = container.Resolve<Options>();
            ProductHeaderValue productHeaderValue = GetProductHeaderValue();

            if(!String.IsNullOrWhiteSpace(options.Token))
            {
                Credentials      credentials     = new Credentials(options.Token);
                ICredentialStore credentialStore = new InMemoryCredentialStore(credentials);

                return new GitHubClient(productHeaderValue, credentialStore);
            }

            return new GitHubClient(productHeaderValue);
        }

        private static ProductHeaderValue GetProductHeaderValue()
        {
            string productName = GetProductName();
            string versionInfo = GetVersionInfo();

            return new ProductHeaderValue(productName, versionInfo);
        }

        private static string GetProductName()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyProductAttribute assemblyTitle = assembly.GetCustomAttributes<AssemblyProductAttribute>()
                                                             .FirstOrDefault();

            return assemblyTitle != null
                       ? assemblyTitle.Product
                       : assembly.GetName()
                                 .Name;
        }

        private static string GetVersionInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyInformationalVersionAttribute assemblyInformationalVersion = assembly
                                                                                .GetCustomAttributes<
                                                                                     AssemblyInformationalVersionAttribute>()
                                                                                .FirstOrDefault();

            return assemblyInformationalVersion != null
                       ? assemblyInformationalVersion.InformationalVersion
                       : assembly.GetName()
                                 .Version?.ToString();
        }

        private void Run()
        {
            Parser.Default.ParseArguments<NewOptions, object>(args)
                  .WithParsed<NewOptions>(HandleNew)
                  .WithNotParsed(HandleNotParsed);
        }

        private void HandleNotParsed(IEnumerable<Error> errors)
        {
            IVerbHandler handler = container.Resolve<UnknownVerbHandler>();

            handler.Run();
        }

        private void HandleNew(NewOptions options)
        {
            container.RegisterInstance(options);
            container.RegisterInstance<Options>(options);
            IVerbHandler handler = container.Resolve<NewHandler>();

            handler.Run();
        }
    }
}
