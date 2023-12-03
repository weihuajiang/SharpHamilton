using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad.UI
{
    public interface IOpenDocumentViewModel
    {
        Task AutoSave();
        DocumentViewModel? Document { get; }
        Task<SaveResult> Save(bool promptSave);
        DocumentId DocumentId { get; }
        void Close();
        string Title { get; }
        bool IsDirty { get; }
    }
}
