using Topshelf;

namespace MusCat.WinServiceRadio
{
    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<MainService>(s =>
                {
                    s.ConstructUsing(name => new MainService());
                    s.WhenStarted(svc => svc.Start());
                    s.WhenStopped(svc => svc.Stop());
                });

                x.RunAsLocalSystem();
                x.SetDescription("MusCat Radio Service WebAPI selfhosting Windows Service Example");
                x.SetDisplayName("MusCat Radio Service Example");
                x.SetServiceName("MusCatRadioService");
            });

            return (int)exitCode;
        }
    }
}
