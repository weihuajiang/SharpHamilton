using System;
using System.ComponentModel;
using System.Composition;
using System.IO;
using Newtonsoft.Json;

namespace RoslynPad.UI
{
    public interface IApplicationSettings : INotifyPropertyChanged
    {
        void LoadFrom(string path);

        bool SendErrors { get; set; }
        bool EnableBraceCompletion { get; set; }
        string? LatestVersion { get; set; }
        string? WindowBounds { get; set; }
        string? DockLayout { get; set; }
        string? WindowState { get; set; }
        double EditorFontSize { get; set; }
        string? DocumentPath { get; set; }
        bool SearchFileContents { get; set; }
        bool SearchUsingRegex { get; set; }
        bool OptimizeCompilation { get; set; }
        int LiveModeDelayMs { get; set; }
        bool SearchWhileTyping { get; set; }
        string DefaultPlatformName { get; set; }
    }

    [Export(typeof(IApplicationSettings)), Shared]
    internal class ApplicationSettings : NotificationObject, IApplicationSettings
    {
        private const int LiveModeDelayMsDefault = 2000;
        private const int EditorFontSizeDefault = 12;

        private readonly ITelemetryProvider _telemetryProvider;
        private string? _path;

        private bool _sendErrors;
        private string? _latestVersion;
        private string? _windowBounds;
        private string? _dockLayout;
        private string? _windowState;
        private double _editorFontSize = EditorFontSizeDefault;
        private string? _documentPath;
        private bool _searchFileContents;
        private bool _searchUsingRegex;
        private bool _optimizeCompilation;
        private int _liveModeDelayMs = LiveModeDelayMsDefault;
        private bool _searchWhileTyping;
        private bool _enableBraceCompletion = true;
        private string _defaultPlatformName;

        [ImportingConstructor]
        public ApplicationSettings(ITelemetryProvider telemetryProvider)
        {
            _telemetryProvider = telemetryProvider;
            _defaultPlatformName = string.Empty;
        }

        public void LoadFrom(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            LoadSettings(path);

            _path = path;
        }

        public bool SendErrors
        {
            get => _sendErrors;
            set => SetProperty(ref _sendErrors, value);
        }

        public bool EnableBraceCompletion
        {
            get => _enableBraceCompletion;
            set => SetProperty(ref _enableBraceCompletion, value);
        }

        public string? LatestVersion
        {
            get => _latestVersion;
            set => SetProperty(ref _latestVersion, value);
        }

        public string? WindowBounds
        {
            get => _windowBounds;
            set => SetProperty(ref _windowBounds, value);
        }

        public string? DockLayout
        {
            get => _dockLayout;
            set => SetProperty(ref _dockLayout, value);
        }

        public string? WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public double EditorFontSize
        {
            get => _editorFontSize;
            set => SetProperty(ref _editorFontSize, value);
        }

        public string? DocumentPath
        {
            get => _documentPath;
            set => SetProperty(ref _documentPath, value);
        }

        public bool SearchFileContents
        {
            get => _searchFileContents;
            set => SetProperty(ref _searchFileContents, value);
        }

        public bool SearchUsingRegex
        {
            get => _searchUsingRegex;
            set => SetProperty(ref _searchUsingRegex, value);
        }

        public bool OptimizeCompilation
        {
            get => _optimizeCompilation;
            set => SetProperty(ref _optimizeCompilation, value);
        }

        public int LiveModeDelayMs
        {
            get => _liveModeDelayMs;
            set => SetProperty(ref _liveModeDelayMs, value);
        }

        public bool SearchWhileTyping
        {
            get => _searchWhileTyping;
            set => SetProperty(ref _searchWhileTyping, value);
        }

        public string DefaultPlatformName
        {
            get => _defaultPlatformName;
            set => SetProperty(ref _defaultPlatformName, value);
        }

        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            SaveSettings();
        }

        private void LoadSettings(string path)
        {
            if (!File.Exists(path))
            {
                LoadDefaultSettings();
                return;
            }

            try
            {
                var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
                using (var reader = File.OpenText(path))
                {
                    serializer.Populate(reader, this);
                }
            }
            catch (Exception e)
            {
                LoadDefaultSettings();
                _telemetryProvider.ReportError(e);
            }
        }

        private void LoadDefaultSettings()
        {
            SendErrors = true;
            EditorFontSize = EditorFontSizeDefault;
            LiveModeDelayMs = LiveModeDelayMsDefault;
        }

        private void SaveSettings()
        {
            if (_path == null) return;

            try
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                using (var writer = File.CreateText(_path))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                _telemetryProvider.ReportError(e);
            }
        }
    }
}