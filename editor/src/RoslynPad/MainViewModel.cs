using System;
using System.Composition;
using System.Reflection;
using RoslynPad.UI;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RoslynPad
{
    [Export(typeof(MainViewModelBase)), Shared]
    public class MainViewModel : MainViewModelBase
    {
        [ImportingConstructor]
        public MainViewModel(IServiceProvider serviceProvider, ITelemetryProvider telemetryProvider, ICommandProvider commands, IApplicationSettings settings, NuGetViewModel nugetViewModel, DocumentFileWatcher documentFileWatcher) : base(serviceProvider, telemetryProvider, commands, settings, nugetViewModel, documentFileWatcher)
        {
        }
        public override void CreateNewDocument()
        {
            var openDocument = GetOpenDocumentViewModel(null);
            openDocument.DefaultCode = @"var ML_STAR=new STARCommand();
//ML_STAR.Init(@""C:\Program Files(x86)\HAMILTON\Methods\Test\SystemEditor3d.lay"", 0, true);
ML_STAR.Init(true);
//ML_STAR.Show3DSystemView();//show 3D deck layout
ML_STAR.Start();
ML_STAR.Initialize();
//write your code here
ML_STAR.End();";
            OpenDocuments.Add(openDocument);
            CurrentOpenDocument = openDocument;
            
        }

        protected override ImmutableArray<Assembly> CompositionAssemblies => base.CompositionAssemblies
            .Add(Assembly.Load(new AssemblyName("RoslynPad.Roslyn.Windows")))
            .Add(Assembly.Load(new AssemblyName("RoslynPad.Editor.Windows")))
            .Add(Assembly.Load(new AssemblyName("Huarui.STARLine")));
        protected override ImmutableArray<Type> TypeNamespaceImports => base.TypeNamespaceImports
            .Add(typeof(Huarui.STARLine.STARCommand));

    }
}