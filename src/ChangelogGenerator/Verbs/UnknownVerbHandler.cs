using UnityContainerAttributeRegistration.Attribute;


namespace ChangelogGenerator.Verbs
{
    [RegisterType]
    internal class UnknownVerbHandler : IVerbHandler
    {
        void IVerbHandler.Run()
        {
        }
    }
}
