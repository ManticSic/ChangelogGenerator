using System;

using Unity;

using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator
{
    internal class EnvironmentAbstraction : IEnvironmentAbstraction
    {
        private readonly Action<int> exitAction;

        public EnvironmentAbstraction(Action<int> exitAction)
        {
            this.exitAction = exitAction;
        }

        public void Exit(int exitCode)
        {
            exitAction(exitCode);
        }
    }
}
