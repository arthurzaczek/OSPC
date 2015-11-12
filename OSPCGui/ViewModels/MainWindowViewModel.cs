using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSPC;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using OSPC.Tokenizer;

namespace OSPCGui.ViewModels
{
    public class MainWindowViewModel : ViewModel, IProgressReporter
    {
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
                    var comparer = new Comparer(Configuration, (IProgressReporter)this);
                    var compareResult = comparer.Compare(Submissions.Select(i => i.Submission).ToArray());
                    var result = OSPCResult.Create(compareResult);
                    var friends = new OSPC.FriendFinder(Configuration);
                    friends.Find(result, compareResult);

                    var html = new OSPC.Reporter.Html.HtmlReporter(HtmlReportPath);
                    html.Create(result);
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(html.OutPath, "index.html"));

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
            foreach(var s in SelectedSubmissions.ToList())
            {
                Submissions.Remove(s);
            }
        }

        void IProgressReporter.Start()
        {
            throw new NotImplementedException();
        }

        void IProgressReporter.Progress(double p)
        {
            throw new NotImplementedException();
        }

        void IProgressReporter.End()
        {
            throw new NotImplementedException();
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

        private ObservableCollection<SubmissionViewModel> _submissions = new ObservableCollection<SubmissionViewModel>();
        public ObservableCollection<SubmissionViewModel> Submissions
        {
            get
            {
                return _submissions;
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
        #endregion
    }
}
