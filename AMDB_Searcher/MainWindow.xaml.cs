using AMDB_Searcher.Model;
using AMDB_Searcher.Model.Command;
using AMDB_Searcher.Model.Connections;
using AMDB_Searcher.Model.Constant;
using AMDB_Searcher.Model.Constants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AMDB_Searcher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Server server;
        private readonly MyDb myDb;
        private Device selectedDevice;
        private ObservableCollection<Device> devices;
        private List<string> users;
        private string currentUser;
        private string currentDomain;
        private string userInput;
        private int choice;
        public string hint;
        public bool IsSearched { get; private set; }

        #region PropertyChanged
        public int Choice
        {
            get
            {
                return this.choice;
            }
            set
            {
                if (this.choice == value)
                    return;

                this.choice = value;
                this.OnPropertyChanged(nameof(this.Choice));
            }
        }

        public string Hint
        {
            get
            {
                return this.hint;
            }
            set
            {
                if (this.hint == value)
                    return;

                this.hint = value;
                this.OnPropertyChanged(nameof(this.Hint));
            }
        }

        public string CurrentUser
        {
            get
            {
                return this.currentUser;
            }
            set
            {
                if (this.currentUser == value)
                    return;

                this.currentUser = value;
                this.OnPropertyChanged(nameof(this.CurrentUser));
            }
        }

        public string CurrentDomain
        {
            get
            {
                return this.currentDomain;
            }
            set
            {
                if (this.currentDomain == value)
                    return;

                this.currentDomain = value;
                this.OnPropertyChanged(nameof(this.CurrentDomain));
            }
        }

        public string UserInput
        {
            get
            {
                return this.userInput;
            }
            set
            {
                if (this.userInput == value)
                    return;

                this.userInput = value;
                this.OnPropertyChanged(nameof(this.UserInput));
            }
        }

        public Device SelectedDevice
        {
            get
            {
                return this.selectedDevice;
            }
            set
            {
                if (this.selectedDevice == value)
                    return;

                this.selectedDevice = value;
                this.OnPropertyChanged(nameof(this.SelectedDevice));

                this.ShowFormatedText();
            }
        }

        public ObservableCollection<Device> Devices
        {
            get
            {
                return this.devices;
            }
            set
            {
                if (this.devices == value)
                    return;

                this.devices = value;
                this.OnPropertyChanged(nameof(this.Devices));
            }
        }

        public List<string> Users
        {
            get
            {
                return this.users;
            }
            set
            {
                if (this.users == value)
                    return;

                this.users = value;
                this.OnPropertyChanged(nameof(this.Users));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged is null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Commands
        public ICommand ClickTextBoxCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        #endregion

        #region Constructor
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.ClickTextBoxCommand = new RelayCommand(this.ClickTextBoxCommandExecuted);
            this.SearchCommand = new RelayCommand(this.SearchCommandExecuted, this.SearchCommandCanExecute);
            this.UserInput = string.Empty;
            this.Choice = 0;
            this.myDb = new MyDb();
            this.server = new Server(Credentials.dbServer);
            this.Devices = new ObservableCollection<Device>();
            this.CurrentUser = Environment.UserName;
            this.CurrentDomain = Environment.UserDomainName;
            this.Users = new List<string>();
            this.Hint = DefaultValues.userInputGreeting;
        }
        #endregion

        #region Methods
        public void ShowFormatedText()
        {
            Paragraph p = new Paragraph();
            this.myRichTextBox.Document.Blocks.Clear();
            string text = string.Empty;

            if (this.SelectedDevice?.Comment != null)
                text = this.SelectedDevice.Comment;

            string[] words = text.Split(' ');
            string[] userWords = this.UserInput.Split(' ');

            foreach (string word in words)
            {
                string res = $"{word} ";
                bool isMatchFound = false;

                foreach (var userWord in userWords)
                {
                    if (word.ToLower().Contains(userWord.ToLower()))
                    {
                        p.Inlines.Add(new Bold(new Run(res)));
                        isMatchFound = true;
                        break;
                    }
                }

                if (!isMatchFound)
                    p.Inlines.Add(new Run(res));
            }

            this.myRichTextBox.Document.Blocks.Add(p);
        }

        private void SetUserInputFocus()
        {
            this.myUserInputTextBox.Focus();
            this.myUserInputTextBox.SelectAll();
        }

        private void SetListBoxItemFocus()
        {
            this.myListBox.Focus();
            this.myListBox.SelectedIndex = 0;
        }

        private bool CheckRunAccess()
        {
            foreach (string dbUser in this.Users)
            {
                if (dbUser == this.CurrentUser && this.CurrentDomain == Credentials.domain)
                    return true;
            }

            return false;
        }
        #endregion

        #region MainWindowLoaded
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.SetUserInputFocus();
            this.Users = this.myDb.GetUserAccessList();

            if (!this.server.IsAvailable)
            {
                MessageBox.Show("The server does not respond or is not available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }

            if (!this.CheckRunAccess())
            {
                MessageBox.Show("Access is denied.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
        }
        #endregion

        #region Commands
        public void ClickTextBoxCommandExecuted(object obj)
        {
            this.SetUserInputFocus();
        }

        public void SearchCommandExecuted(object obj)
        {
            this.IsSearched = true;
            this.Devices.Clear();
            List<Device> resultSearch = this.myDb.Search(this.UserInput, this.Choice);

            if (resultSearch is null)
                return;

            foreach (var device in resultSearch)
            {
                this.Devices.Add(device);
            }

            this.IsSearched = false;

            if (this.Devices.Count > 0)
                this.SetListBoxItemFocus();
            else
                this.SetUserInputFocus();
        }

        public bool SearchCommandCanExecute(object obj)
        {
            if (this.IsSearched || this.UserInput.Trim().Length < 1 || !this.server.IsAvailable)
                return false;
            else
                return true;
        }
        #endregion
    }
}

