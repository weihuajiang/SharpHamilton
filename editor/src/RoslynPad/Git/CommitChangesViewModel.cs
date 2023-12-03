﻿using Microsoft.CodeAnalysis;
using RoslynPad.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    public class CommitChangesViewModel : IOpenDocumentViewModel, IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public CommitChangesViewModel(string path, string status, bool isFolder = false)
        {
            Path = path;
            Status = status;
            IsFolder = isFolder;
            Name = System.IO.Path.GetFileName(path);
            DocumentId = DocumentId.CreateNewId(ProjectId.CreateNewId());
        }
        public MainViewModel MainViewModel { get; set; }
        public DocumentViewModel? Document => null;
        public DocumentId DocumentId { get; }
        public string CommitId { get; set; } = "";
        public string Title { get; set; } = "";

        public bool IsDirty => false;
        public bool IsFolder { get; private set; } = false;
        public string Name { get; private set; }
        public string Path { get; set; }
        public string Status { get; set; }
        public ObservableCollection<CommitChangesViewModel> Children
        {
            get;
        } = new ObservableCollection<CommitChangesViewModel>();

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
            foreach (var i in Children)
                i.Dispose();
            Children.Clear();
        }
    }
}
