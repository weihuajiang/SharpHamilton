﻿using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.BraceMatching;
using RoslynPad.Roslyn.Diagnostics;
using RoslynPad.Roslyn.QuickInfo;
using System;
using System.Threading;
using System.Threading.Tasks;
using RoslynPad.Roslyn.AutomaticCompletion;
#if AVALONIA
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Input;
using ImageSource = Avalonia.Media.Drawing;
using ModifierKeys = Avalonia.Input.InputModifiers;
using TextCompositionEventArgs = Avalonia.Input.TextInputEventArgs;
using RoutingStrategy = Avalonia.Interactivity.RoutingStrategies;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
#endif

namespace RoslynPad.Editor
{
    public class RoslynCodeEditor : CodeTextEditor
    {
        private readonly TextMarkerService _textMarkerService;
        private BraceMatcherHighlightRenderer? _braceMatcherHighlighter;
        private ContextActionsRenderer? _contextActionsRenderer;
        private IClassificationHighlightColors? _classificationHighlightColors;
        private IRoslynHost? _roslynHost;
        private DocumentId? _documentId;
        private IQuickInfoProvider? _quickInfoProvider;
        private IBraceMatchingService? _braceMatchingService;
        private IBraceCompletionProvider? _braceCompletionProvider;
        private CancellationTokenSource? _braceMatchingCts;
        private RoslynHighlightingColorizer? _colorizer;

        public RoslynCodeEditor()
        {
            _textMarkerService = new TextMarkerService(this);
            TextArea.TextView.BackgroundRenderers.Add(_textMarkerService);
            TextArea.TextView.LineTransformers.Add(_textMarkerService);
            TextArea.Caret.PositionChanged += CaretOnPositionChanged;
            TextArea.TextEntered += OnTextEntered;
        }

        public bool IsBraceCompletionEnabled
        {
            get { return this.GetValue(IsBraceCompletionEnabledProperty); }
            set { this.SetValue(IsBraceCompletionEnabledProperty, value); }
        }

        public static readonly StyledProperty<bool> IsBraceCompletionEnabledProperty =
            CommonProperty.Register<RoslynCodeEditor, bool>(nameof(IsBraceCompletionEnabled), defaultValue: true);

        public static readonly StyledProperty<ImageSource> ContextActionsIconProperty = CommonProperty.Register<RoslynCodeEditor, ImageSource>(
            nameof(ContextActionsIcon), onChanged: OnContextActionsIconChanged);

        private static void OnContextActionsIconChanged(RoslynCodeEditor editor, CommonPropertyChangedArgs<ImageSource> args)
        {
            if (editor._contextActionsRenderer != null)
            {
                editor._contextActionsRenderer.IconImage = args.NewValue;
            }
        }

        public ImageSource ContextActionsIcon
        {
            get => this.GetValue(ContextActionsIconProperty);
            set => this.SetValue(ContextActionsIconProperty, value);
        }

        public static readonly RoutedEvent CreatingDocumentEvent = CommonEvent.Register<RoslynCodeEditor, CreatingDocumentEventArgs>(nameof(CreatingDocument), RoutingStrategy.Bubble);

        public event EventHandler<CreatingDocumentEventArgs> CreatingDocument
        {
            add => AddHandler(CreatingDocumentEvent, value);
            remove => RemoveHandler(CreatingDocumentEvent, value);
        }

        protected virtual void OnCreatingDocument(CreatingDocumentEventArgs e)
        {
            RaiseEvent(e);
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (IsBraceCompletionEnabled && e.Text.Length == 1 && _braceCompletionProvider != null && _roslynHost != null &&
                _documentId != null && _roslynHost.GetDocument(_documentId) is Document document)
            {
                _braceCompletionProvider.TryComplete(document, CaretOffset);
            }
        }

        public DocumentId Initialize(IRoslynHost roslynHost, IClassificationHighlightColors highlightColors, string workingDirectory, string documentText)
        {
            _roslynHost = roslynHost ?? throw new ArgumentNullException(nameof(roslynHost));
            _classificationHighlightColors = highlightColors ?? throw new ArgumentNullException(nameof(highlightColors));

            _braceMatcherHighlighter = new BraceMatcherHighlightRenderer(TextArea.TextView, _classificationHighlightColors);

            _quickInfoProvider = _roslynHost.GetService<IQuickInfoProvider>();
            _braceMatchingService = _roslynHost.GetService<IBraceMatchingService>();
            _braceCompletionProvider = _roslynHost.GetService<IBraceCompletionProvider>();

            var avalonEditTextContainer = new AvalonEditTextContainer(Document) { Editor = this };

            var creatingDocumentArgs = new CreatingDocumentEventArgs(avalonEditTextContainer, ProcessDiagnostics);
            OnCreatingDocument(creatingDocumentArgs);

            _documentId = creatingDocumentArgs.DocumentId ??
                roslynHost.AddDocument(new DocumentCreationArgs(avalonEditTextContainer, workingDirectory,
                    args => ProcessDiagnostics(args), text => avalonEditTextContainer.UpdateText(text)));

            AppendText(documentText);
            Document.UndoStack.ClearAll();
            AsyncToolTipRequest = OnAsyncToolTipRequest;

            RefreshHighlighting();

            _contextActionsRenderer = new ContextActionsRenderer(this, _textMarkerService) { IconImage = ContextActionsIcon };
            _contextActionsRenderer.Providers.Add(new RoslynContextActionProvider(_documentId, _roslynHost));

            var completionProvider = new RoslynCodeEditorCompletionProvider(_documentId, _roslynHost);
            completionProvider.Warmup();

            CompletionProvider = completionProvider;

            return _documentId;
        }

        public void RefreshHighlighting()
        {
            if (_colorizer != null)
            {
                TextArea.TextView.LineTransformers.Remove(_colorizer);
            }

            if (_documentId != null && _roslynHost != null && _classificationHighlightColors != null)
            {
                _colorizer = new RoslynHighlightingColorizer(_documentId, _roslynHost, _classificationHighlightColors);
                TextArea.TextView.LineTransformers.Insert(0, _colorizer);
            }
        }

        private async void CaretOnPositionChanged(object sender, EventArgs eventArgs)
        {
            if (_roslynHost == null || _documentId == null || _braceMatcherHighlighter == null)
            {
                return;
            }

            _braceMatchingCts?.Cancel();

            if (_braceMatchingService == null) return;

            var cts = new CancellationTokenSource();
            var token = cts.Token;
            _braceMatchingCts = cts;

            var document = _roslynHost.GetDocument(_documentId);
            if (document == null)
            {
                return;
            }

            var text = await document.GetTextAsync().ConfigureAwait(false);
            var caretOffset = CaretOffset;
            if (caretOffset <= text.Length)
            {
                var result = await _braceMatchingService.GetAllMatchingBracesAsync(document, caretOffset, token).ConfigureAwait(true);
                _braceMatcherHighlighter.SetHighlight(result.leftOfPosition, result.rightOfPosition);
            }
        }

        private void TryJumpToBrace()
        {
            if (_braceMatcherHighlighter == null) return;

            var caret = CaretOffset;

            if (TryJumpToPosition(_braceMatcherHighlighter.LeftOfPosition, caret) ||
                TryJumpToPosition(_braceMatcherHighlighter.RightOfPosition, caret))
            {
                ScrollToLine(TextArea.Caret.Line);
            }
        }

        private bool TryJumpToPosition(BraceMatchingResult? position, int caret)
        {
            if (position != null)
            {
                if (position.Value.LeftSpan.Contains(caret))
                {
                    CaretOffset = position.Value.RightSpan.End;
                    return true;
                }

                if (position.Value.RightSpan.Contains(caret) || position.Value.RightSpan.End == caret)
                {
                    CaretOffset = position.Value.LeftSpan.Start;
                    return true;
                }
            }

            return false;
        }

        private async Task OnAsyncToolTipRequest(ToolTipRequestEventArgs arg)
        {
            if (_roslynHost == null || _documentId == null || _quickInfoProvider == null)
            {
                return;
            }

            // TODO: consider invoking this with a delay, then showing the tool-tip without one
            var document = _roslynHost.GetDocument(_documentId);
            if (document == null)
            {
                return;
            }

            var info = await _quickInfoProvider.GetItemAsync(document, arg.Position, CancellationToken.None).ConfigureAwait(true);
            if (info != null)
            {
                arg.SetToolTip(info.Create());
            }
        }

        protected void ProcessDiagnostics(DiagnosticsUpdatedArgs args)
        {
            if (this.GetDispatcher().CheckAccess())
            {
                ProcessDiagnosticsOnUiThread(args);
                return;
            }

            this.GetDispatcher().InvokeAsync(() => ProcessDiagnosticsOnUiThread(args));
        }

        private void ProcessDiagnosticsOnUiThread(DiagnosticsUpdatedArgs args)
        {
            _textMarkerService.RemoveAll(marker => Equals(args.Id, marker.Tag));

            if (args.Kind != DiagnosticsUpdatedKind.DiagnosticsCreated)
            {
                return;
            }

            foreach (var diagnosticData in args.Diagnostics)
            {
                if (diagnosticData.Severity == DiagnosticSeverity.Hidden || diagnosticData.IsSuppressed)
                {
                    continue;
                }

                var marker = _textMarkerService.TryCreate(diagnosticData.TextSpan.Start, diagnosticData.TextSpan.Length);
                if (marker != null)
                {
                    marker.Tag = args.Id;
                    marker.MarkerColor = GetDiagnosticsColor(diagnosticData);
                    marker.ToolTip = diagnosticData.Message;
                }
            }
        }

        private static Color GetDiagnosticsColor(DiagnosticData diagnosticData)
        {
            switch (diagnosticData.Severity)
            {
                case DiagnosticSeverity.Info:
                    return Colors.LimeGreen;
                case DiagnosticSeverity.Warning:
                    return Colors.DodgerBlue;
                case DiagnosticSeverity.Error:
                    return Colors.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticData));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.HasModifiers(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.OemCloseBrackets:
                        TryJumpToBrace();
                        break;
                }
            }
        }
    }

    public class CreatingDocumentEventArgs : RoutedEventArgs
    {
        public CreatingDocumentEventArgs(AvalonEditTextContainer textContainer, Action<DiagnosticsUpdatedArgs> processDiagnostics)
        {
            TextContainer = textContainer;
            ProcessDiagnostics = processDiagnostics;
            RoutedEvent = RoslynCodeEditor.CreatingDocumentEvent;
        }

        public AvalonEditTextContainer TextContainer { get; }

        public Action<DiagnosticsUpdatedArgs> ProcessDiagnostics { get; }

        public DocumentId? DocumentId { get; set; }
    }
}
