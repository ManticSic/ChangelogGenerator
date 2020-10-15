using System;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator
{
    [RegisterType(typeof(IEnvironmentAbstraction))]
    internal class EnvironmentAbstraction : IEnvironmentAbstraction
    {
        public void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
    }
}
