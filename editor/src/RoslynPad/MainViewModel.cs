using System;
using System.Composition;
using System.Reflection;
using RoslynPad.UI;
using System.Collections.Immutable;
using System.Threading.Tasks;
using RoslynPad.Utilities;
using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace RoslynPad
{
    [Export(typeof(MainViewModelBase)), Shared]
    public class MainViewModel : MainViewModelBase, IDisposable
    {
        bool gitInited = false;

        public bool IsGitInited
        {
            get { return gitInited; }
            set { SetProperty<bool>(ref gitInited, value); }
        }
        public IDelegateCommand InitGitCommand { get; }
        public IDelegateCommand CommitCommand { get; }
        public IDelegateCommand BrachHistoryCommand { get; }
        Repository? repository;

        [ImportingConstructor]
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public MainViewModel(IServiceProvider serviceProvider, ITelemetryProvider telemetryProvider, ICommandProvider commands, IApplicationSettings settings, NuGetViewModel nugetViewModel, DocumentFileWatcher documentFileWatcher) : base(serviceProvider, telemetryProvider, commands, settings, nugetViewModel, documentFileWatcher)
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        {
            InitGitCommand = commands.CreateAsync(InitGit);
            CommitCommand = commands.Create(Commit);
            CheckGit();
        }
        public override void EditUserDocumentPath()
        {
            base.EditUserDocumentPath();
            CheckGit();
        }
        void CheckGit()
        {
            if (repository != null)
            {
                repository.Dispose();
                repository = null;
            }
            var path = DocumentRoot.Path;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                var name = Repository.Discover(path);
                if (!string.IsNullOrEmpty(name))
                {
                    repository = new Repository(name);
                    LoadIgnore();
                    AddIgnore();
                    IsGitInited = true;
                }
            }
            else
                IsGitInited = false;
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

        public async Task InitGit()
        {
            await Task.Run(() =>
            {
                var path = DocumentRoot.Path;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    var name = Repository.Discover(path);
                    if (string.IsNullOrEmpty(name))
                    {
                        //copy .ignore and load .ignore
                        var ignorePath = Path.Combine(DocumentRoot.Path, ".gitignore");
                        if (!File.Exists(ignorePath))
                        {
                            using (var stream = this.GetType().Assembly.GetManifestResourceStream("RoslynPad.Resources..gitignore"))
                            {
                                var bytes = new byte[2048];
                                int read;
                                using (var writer = new FileStream(ignorePath, FileMode.Create))
                                {
                                    while ((read = stream.Read(bytes, 0, bytes.Length)) > 0)
                                        writer.Write(bytes, 0, read);
                                }
                            }
                        }
                        Repository.Init(path);
                        repository = new Repository(path);
                        var user = new Signature(Environment.UserName, Environment.UserName + "@annymous", DateTimeOffset.Now);
                        LoadIgnore();
                        AddIgnore();
                        Commands.Stage(repository, "*");
                        repository.Commit("initial", user, user);
                    }
                }
            });
            IsGitInited = true;
        }
        List<string> ignores = new List<string>()
        {"*.autosave.csx", ".vs", "RoslynPad.json", "RoslynPad.nuget.config"
        };
        void LoadIgnore()
        {
            var path = Path.Combine(DocumentRoot.Path, ".gitignore");
            if (File.Exists(path))
            {
                ignores.Clear();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("#")) continue;
                        ignores.Add(line);
                    }
                }
            }
        }
        void AddIgnore()
        {
            if (repository == null) return;
            repository.Ignore.AddTemporaryRules(ignores);
        }
        public void Commit()
        {
            if (repository != null)
            {
                var changes = GetChanges(repository);
                if (changes.Children.Count == 0)
                {
                    Console.WriteLine("no changes");
                    return;
                }
                OpenDocuments.Add(changes);
                CurrentOpenDocument = changes;
            }
        }
        void GitOpen(GitChangesViewModel document, string path)
        {
        }
        async void GitCommit(GitChangesViewModel document, string comment)
        {
            if (repository == null) return;
            Commands.Stage(repository, "*");
            var user = new Signature(Environment.UserName, Environment.UserName + "@annymous", DateTimeOffset.Now);
            repository.Commit(comment, user, user);
            await CloseDocument(document);
        }
        async void GitIgnore(GitChangesViewModel document, string path)
        {
            if (repository == null) return;
            ignores.Add(path);
            var ignorepath = Path.Combine(DocumentRoot.Path, ".gitignore");
            if (File.Exists(ignorepath))
            {
                ignores.Clear();
                using (StreamWriter writer = new StreamWriter(ignorepath, true))
                {
                    writer.WriteLine(path);
                }
            }
            AddIgnore();
            var changes = GetChanges(repository);
            document.Children.Clear();
            foreach (var i in changes.Children)
                document.Children.Add(i);
            if(document.Children.Count==0) await CloseDocument(document);
        }
        private GitChangesViewModel GetChanges(Repository repo)
        {
            var models = new GitChangesViewModel("", "");
            models.CommitCommand = GitCommit;
            models.IgnoreCommand = GitIgnore;
            models.Title = "Commit Changes";
            var changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.WorkingDirectory);
            foreach (var i in changes)
            {
                string path = Path.GetDirectoryName(i.Path);
                Console.WriteLine(path);
                if(string.IsNullOrEmpty(path))
                    models.Children.Add(new GitChangesViewModel(i.Path, (i.Status+"").Substring(0,1)));
                else
                {
                    var pathModel = models;
                    var paths = path.Split('\\', '/');
                    foreach(var p in paths)
                    {
                        GitChangesViewModel? child=null;
                        foreach (var c in pathModel.Children)
                        {
                            if (c.IsFolder && c.Name == p)
                                child = c;
                        }
                        if (child == null)
                        {
                            child = new GitChangesViewModel(p, "", true);
                            pathModel.Children.Add(child);
                        }
                        pathModel = child;
                    }
                    pathModel.Children.Add(new GitChangesViewModel(i.Path, (i.Status + "").Substring(0, 1)));
                }
            }
            return models;
        }
        private GitChangesViewModel ShowChanges(Repository repo, Commit commit)
        {
            var models = new GitChangesViewModel("", "");
            Tree commitTree = commit.Tree;
            var parentCommit = commit.Parents.FirstOrDefault();
            if (parentCommit == null)
            {
                foreach(var i in commitTree)
                {
                    if(i.TargetType!= TreeEntryTargetType.Tree)
                    {
                        models.Children.Add(new GitChangesViewModel(i.Path, "A"));
                    }
                }
                return models;
            }
            Tree parentCommitTree = parentCommit.Tree;
            var changes = repo.Diff.Compare<TreeChanges>(parentCommitTree, commitTree);

            foreach (TreeEntryChanges treeEntryChanges in changes)
            {
                models.Children.Add(new GitChangesViewModel(treeEntryChanges.Path, (treeEntryChanges.Status + "").Substring(0, 1)));
            }
            return models;
        }
        public void ViewBrachHistory()
        {

        }

        public void Dispose()
        {
            if(repository!=null)
            {
                repository.Dispose();
                repository = null;
            }
        }
    }
}