using Castle.Windsor;

namespace PillarAPI
{
    public static class DependencyInjection
    {
        public static IWindsorContainer BootstrapContainer()
        {
            IWindsorContainer container= new WindsorContainer();
            //container.Register(Classes.FromThisAssembly().InSameNamespaceAs<PillarInitializer>().WithService.DefaultInterfaces().LifestyleTransient());
            //container.Register(Classes.FromThisAssembly().InNamespace("Apache.NMS").WithService.DefaultInterfaces().LifestyleTransient());
            return container;
        }
    }
}
