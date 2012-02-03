using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading;
using Brejc.Common.Props;
using Brejc.DemLibrary;
using Brejc.DemLibrary.Isopleting;
using Brejc.DemLibrary.Srtm;
using Brejc.DemLibrary.ViewfinderPanoramas;
using Brejc.Geometry;
using Brejc.Rasters;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using GroundTruth.Engine;
using GroundTruth.Engine.Contours;
using GroundTruth.Engine.Contours.Ibf2Osm;
using GroundTruth.Engine.Tasks;

namespace GroundTruth
{
    public sealed class Program
    {
        static int Main (string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            log4net.Config.XmlConfigurator.Configure();

            IWindsorContainer windsorContainer = ConfigureWindsor();

            ICollection<IConsoleCommand> commands = windsorContainer.ResolveAll<IConsoleCommand> ();

            ConsoleApp consoleApp = windsorContainer.Resolve<ConsoleApp> (new Hashtable () { { "args", args }, { "commands", commands } });

            Environment.ExitCode = consoleApp.Process();
            return Environment.ExitCode;
        }

        public static IWindsorContainer ConfigureWindsor()
        {
            IWindsorContainer container = new WindsorContainer (new XmlInterpreter (new ConfigResource ("castle")));
            container.Register (Component.For<ConsoleApp> ().LifeStyle.Transient);
            container.Register (
                AllTypes.FromAssembly (typeof (ConsoleApp).Assembly).BasedOn<IConsoleCommand>().WithService.FirstInterface ()
                .LifestyleTransient());
            container.Register (Component.For<ITaskRunner> ().ImplementedBy<MapMaker> ().LifeStyle.Transient);
            container.Register (Component.For<IContoursGenerator> ().ImplementedBy<DefaultContoursGenerator> ().LifeStyle.Transient);
            container.Register (Component.For<IIbf2OsmGenerator> ().ImplementedBy<DefaultIbf2OsmGenerator> ().LifeStyle.Transient);

            //container.Register(
            //    new ComponentRegistration<IAsyncOperationReporting>().ImplementedBy<ConsoleAsyncOperationReporting>()
            //    .Parameters (Parameter.ForKey ("minVerbosity").Eq("Debug"))
            //    .LifeStyle.Transient);
            
            container.Register(
                new ComponentRegistration<IIsopletingAlgorithm>().ImplementedBy<Igor5IsopletingAlgorithm>()
                .LifeStyle.Transient);

            //container.Register (
            //    new ComponentRegistration<IDemSystemConfiguration> ().ImplementedBy<DemSystemConfiguration> ());

            container.Register (Component.For<IRasterSource> ().Named ("SRTM3").ImplementedBy<Srtm3Source> ().LifeStyle.Transient);
            container.Register (Component.For<IRasterSource> ().Named ("SRTM1").ImplementedBy<Srtm1Source> ().LifeStyle.Transient);
            container.Register (Component.For<IRasterSource> ().Named ("AlpsDem1").ImplementedBy<AlpsDem1Source> ().LifeStyle.Transient);

            container.Register(Component.For<Srtm3IndexFetcher>().LifeStyle.Transient);
            container.Register (Component.For<Srtm1IndexFetcher> ().LifeStyle.Transient);
            container.Register (Component.For<ISrtmServerClient> ().ImplementedBy<DefaultSrtmServerClient> ()
                .DependsOn(Parameter.ForKey("SrtmServerUrl").Eq(ConfigurationManager.AppSettings["SrtmServerUrl"]))
                .LifeStyle.Transient);

            //container.Register (Component.For<IRasterDemProvider> ().ImplementedBy<IntelligentDemProvider> ()
            //    .LifeStyle.Transient);

            string[] demSources = ConfigurationManager.AppSettings["DemSources"].Split(',');

            //container.Register (Component.For<IRasterDemDirectory> ().ImplementedBy<RasterDemDirectory> ()
            //    .ServiceOverrides (ServiceOverride.ForKey ("sources").Eq (demSources)));

            SerializersRegistry serializersRegistry = new SerializersRegistry ();
            serializersRegistry.RegisterSerializer<GisColor> (new GisColorPropertyValueSerializer ());
            container.Register(Component.For<ISerializersRegistry>().Instance(serializersRegistry));

            return container;
        }

        private Program()
        {
        }
    }
}
