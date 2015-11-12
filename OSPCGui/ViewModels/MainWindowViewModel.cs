using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSPC;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using OSPC.Tokenizer;
using System.Diagnostics;

namespace OSPCGui.ViewModels
{
    public class MainWindowViewModel : ViewModel, IProgressReporter
    {
        private readonly System.Windows.Threading.Dispatcher _uiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public MainWindowViewModel()
        {
            this.Configuration = new OSPC.Configuration();
            LoadUserSettings();

            this.Tokenizer = new OSPC.Tokenizer.CLikeTokenizer();
        }

        #region CheckCommand
        private ICommandViewModel _checkCommand;
        public ICommandViewModel CheckCommand
        {
            get
            {
                if (_checkCommand == null)
                {
                    _checkCommand = new SimpleCommandViewModel("Check", "Start the check", Check, CanCheck);
                }
                return _checkCommand;
            }
        }

        public bool CanCheck()
        {
            return _submissions.Count > 0 && !IsRunning;
        }

        public async void Check()
        {
            if (!CanCheck()) return;

            IsRunning = true;
            try
            {
                await Task.Run(() =>
                {
                    var watch = Stopwatch.StartNew();
                    var comparer = new Comparer(Configuration, (IProgressReporter)this);

                    AddMessage(string.Format("Comparing {0} files ", Submissions.Count));
                    var compareResult = comparer.Compare(Submissions.Select(i => i.Submission).ToArray());
                    AddMessage(string.Format("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds));

                    AddMessage("Creating statistics");
                    var result = OSPCResult.Create(compareResult);
                    var friends = new OSPC.FriendFinder(Configuration);
                    friends.Find(result, compareResult);
                    AddMessage(string.Format("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds));

                    AddMessage("Creating reports");
                    var html = new OSPC.Reporter.Html.HtmlReporter(HtmlReportPath, (IProgressReporter)this);
                    html.Create(result);
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(html.OutPath, "index.html"));
                    AddMessage(string.Format("  finished in total {0:n2} sec.", watch.Elapsed.TotalSeconds));

                    SaveUserSettings();
                });
            }
            finally
            {
                IsRunning = false;
            }
        }              
        #endregion

        #region UserSettings
        public void LoadUserSettings()
        {
            this.Configuration.MIN_MATCH_LENGTH = Properties.Settings.Default.MIN_MATCH_LENGTH;
            this.Configuration.MAX_MATCH_DISTANCE = Properties.Settings.Default.MAX_MATCH_DISTANCE;
            this.Configuration.MIN_COMMON_TOKEN = Properties.Settings.Default.MIN_COMMON_TOKEN;
            this.Configuration.MIN_FRIEND_FINDER_SIMILARITY = Properties.Settings.Default.MIN_FRIEND_FINDER_SIMILARITY;
            this.HtmlReportPath = Properties.Settings.Default.HtmlReportPath;
        }

        public void SaveUserSettings()
        {
            Properties.Settings.Default.MIN_MATCH_LENGTH = this.Configuration.MIN_MATCH_LENGTH;
            Properties.Settings.Default.MAX_MATCH_DISTANCE = this.Configuration.MAX_MATCH_DISTANCE;
            Properties.Settings.Default.MIN_COMMON_TOKEN = this.Configuration.MIN_COMMON_TOKEN;
            Properties.Settings.Default.MIN_FRIEND_FINDER_SIMILARITY = this.Configuration.MIN_FRIEND_FINDER_SIMILARITY;
            Properties.Settings.Default.HtmlReportPath = this.HtmlReportPath;

            Properties.Settings.Default.Save();
        }
        #endregion

        #region AddFilesCommand
        private ICommandViewModel _addFilesCommand;
        public ICommandViewModel AddFilesCommand
        {
            get
            {
                if (_addFilesCommand == null)
                {
                    _addFilesCommand = new SimpleCommandViewModel("Add", "Add files", AddFiles);
                }
                return _addFilesCommand;
            }
        }

        private void AddFiles()
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = true,
                ShowReadOnly = false,
                ValidateNames = true,
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var f in dialog.FileNames)
                {
                    var s = new Submission(f, Tokenizer);
                    s.Parse();
                    _submissions.Add(new SubmissionViewModel(s));
                }
            }
        }
        #endregion

        #region RemoveFilesCommand
        private ICommandViewModel _removeFilesCommand;
        public ICommandViewModel RemoveFilesCommand
        {
            get
            {
                if (_removeFilesCommand == null)
                {
                    _removeFilesCommand = new SimpleCommandViewModel("Remove", "Remove selected files", RemoveFiles, CanRemoveFiles);
                }
                return _removeFilesCommand;
            }
        }

        private bool CanRemoveFiles()
        {
            return SelectedSubmissions.Count > 0;
        }

        private void RemoveFiles()
        {
            foreach (var s in SelectedSubmissions.ToList())
            {
                Submissions.Remove(s);
            }
        }
        #endregion

        #region Properties
        private Configuration _configuration;
        public Configuration Configuration
        {
            get
            {
                return _configuration;
            }
            set
            {
                if (_configuration != value)
                {
                    _configuration = value;
                    OnPropertyChanged("Configuration");
                }
            }
        }

        private ObservableCollection<SubmissionViewModel> _submissions;
        public ObservableCollection<SubmissionViewModel> Submissions
        {
            get
            {
                if(_submissions == null)
                {
                    _submissions = new ObservableCollection<SubmissionViewModel>();
                    _submissions.CollectionChanged += (s, e) => OnPropertyChanged("SubmissionCountAsText");
                }
                return _submissions;
            }
        }

        public string SubmissionCountAsText
        {
            get
            {
                return string.Format("{0} files", Submissions.Count);
            }
        }

        private ObservableCollection<SubmissionViewModel> _selectedSubmissions = new ObservableCollection<SubmissionViewModel>();
        public ObservableCollection<SubmissionViewModel> SelectedSubmissions
        {
            get
            {
                return _selectedSubmissions;
            }
        }

        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        public ObservableCollection<string> Messages
        {
            get
            {
                return _messages;
            }
        }

        private void AddMessage(string msg)
        {
            _uiDispatcher.Invoke(() => { Messages.Add(msg); });
        }

        public ITokenizer Tokenizer { get; private set; }

        private string _hmlReportPath = ".\\report";
        public string HtmlReportPath
        {
            get
            {
                return _hmlReportPath;
            }
            set
            {
                if (_hmlReportPath != value)
                {
                    _hmlReportPath = value;
                    OnPropertyChanged("HtmlReportPath");
                }
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged("IsRunning");
                }
            }
        }

        private int _progress;
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        #endregion

        #region IProgressReporter
        void IProgressReporter.Start()
        {
            Progress = 0;
        }

        void IProgressReporter.Progress(double p)
        {
            Progress = (int)(100.0 * p);
        }

        void IProgressReporter.End()
        {
            Progress = 100;
        }
        #endregion
    }
}
