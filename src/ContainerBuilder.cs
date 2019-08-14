namespace Stator
{
    public abstract class ContainerBuilder
    {
        public List<ContainerRegistration> Registrations { get; private set; }

        public ContainerBuilder()
        {
            this.Registrations = new List<ContainerRegistration>();
        }

        public ContainerBuilder Add(ContainerRegistration registration)
        {
            Registrations.Add(registration);
            return this;
        }
    }
}