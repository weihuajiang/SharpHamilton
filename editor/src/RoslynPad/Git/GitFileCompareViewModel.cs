using LibGit2Sharp;
using Microsoft.CodeAnalysis;
using RoslynPad.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public enum CompareAction
    {
        None,
        Blank,
        Added,
        Deleted
    }
    public class CompareLine
    {
        public CompareLine(CompareAction type, int number, string content)
        {
            Type = type;
            Content = content;
            Number = number;
        }
        public CompareAction Type { get; }
        public string Content { get; }
        public int Number { get; }

    }
    public class CompareDocuemnt : List<CompareLine>
    {
        public string Title { get; set; } = "";
        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach(var i in this)
                {
                    var type = i.Type;
                    if (type == CompareAction.Blank)
                        sb.AppendLine();
                    else
                        sb.AppendLine(i.Content);
                }
                return sb.ToString();
            }
        }
    }
    public class GitFileCompareViewModel : IOpenDocumentViewModel, IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public GitFileCompareViewModel(string path, CompareDocuemnt newDoc, CompareDocuemnt oldDoc)
        {
            Path = path;
            DocumentId = DocumentId.CreateNewId(ProjectId.CreateNewId());
            NewDocument = newDoc;
            OldDocument = oldDoc;
        }
        public MainViewModel MainViewModel { get; set; }
        public DocumentViewModel? Document => null;
        public DocumentId DocumentId { get; }

        public string Title { get; set; } = "";

        public bool IsDirty => false;
        public string Path { get; set; }
        public CompareDocuemnt NewDocument { get; }
        public CompareDocuemnt OldDocument { get; }

        public Task AutoSave()
        {
            return Task.Run(() =>
            {

            });
        }

        public void Close()
        {
        }

        public Task<SaveResult> Save(bool promptSave)
        {
            return Task<SaveResult>.Run(() =>
            {
                return SaveResult.DontSave;
            });
        }
        public void Dispose()
        {
        }

        public static GitFileCompareViewModel? CompareFile(Repository repository, string path)
        {
            var lastFile = repository.Head.Tip.Tree[path];
            if (lastFile == null) return null;
            var current = repository.ObjectDatabase.CreateBlob(path);
            var compare = repository.Diff.Compare(lastFile.Target as Blob, current);
            var patch = compare.Patch;
            var docs = PareComparePatch(patch);
            docs.Item1.Title = path;
            docs.Item2.Title = path + ";HEAD";
            return new GitFileCompareViewModel(path, docs.Item1, docs.Item2);
        }
        static Tuple<CompareDocuemnt, CompareDocuemnt> PareComparePatch(string patch)
        {
            Console.WriteLine(patch);
            var lines = patch.Split(new string[] {"\r\n", "\n" }, StringSplitOptions.None);
            CompareDocuemnt newDoc = new CompareDocuemnt();
            CompareDocuemnt oldDoc = new CompareDocuemnt();
            int newLine = 0;
            int oldLine = 0;
            int newAdd = 0;
            int oldAdd = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;
                char type = lines[i][0];
                string content = lines[i].Substring(1);
                if (type == ' ')
                {
                    var diff = newAdd - oldAdd;
                    for (int j = 0; j < diff; j++)
                        oldDoc.Add(new CompareLine(CompareAction.Blank, 0, ""));
                    for (int j = 0; j < -diff; j++)
                        newDoc.Add(new CompareLine(CompareAction.Blank, 0, ""));

                    newAdd = 0;
                    oldAdd = 0;
                    newLine++;
                    newDoc.Add(new CompareLine(CompareAction.None, newLine, content));
                    oldLine++;
                    oldDoc.Add(new CompareLine(CompareAction.None, oldLine, content));
                }
                else if (type == '+')
                {
                    newAdd++;
                    newLine++;
                    newDoc.Add(new CompareLine(CompareAction.Added, newLine, content));
                }
                else if (type == '-')
                {
                    oldAdd++;
                    oldLine++;
                    oldDoc.Add(new CompareLine(CompareAction.Deleted, oldLine, content));
                }
            }
            //Console.WriteLine(oldDoc.Text);
            //Console.WriteLine(newDoc.Text);
            return new Tuple<CompareDocuemnt, CompareDocuemnt>(newDoc, oldDoc);
        }
    }
}
