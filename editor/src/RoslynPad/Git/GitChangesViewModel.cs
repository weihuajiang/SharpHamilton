using LibGit2Sharp;
using Microsoft.CodeAnalysis;
using RoslynPad.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public class GitChangesViewModel : IOpenDocumentViewModel
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public GitChangesViewModel(string path, string status, bool isFolder=false)
        {
            Path = path;
            Status = status;
            IsFolder = isFolder;
            Name = System.IO.Path.GetFileName(path);
            DocumentId = DocumentId.CreateNewId(ProjectId.CreateNewId());
        }
        public DocumentViewModel? Document => null;
        public Action<GitChangesViewModel, string>? CommitCommand { get; set; }
        public Action<GitChangesViewModel, string>? IgnoreCommand { get; set; }
        public DocumentId DocumentId { get; }

        public string Title { get;  set; } = "";

        public bool IsDirty => false;
        public bool IsFolder { get; private set; } = false;
        public string Name { get; private set; }
        public string Path { get; set; }
        public string Status { get; set; }
        public ObservableCollection<GitChangesViewModel> Children
        {
            get;
        } = new ObservableCollection<GitChangesViewModel>();

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
    }
}
