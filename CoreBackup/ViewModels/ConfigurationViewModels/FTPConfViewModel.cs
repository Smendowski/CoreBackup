﻿using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
// FOR FTP SERVER
using Application = Avalonia.Application;
using CoreBackup.Models.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CoreBackup.Models.Config;
using CoreBackup.Validators;
using FluentValidation;
using FluentValidation.Results;
using CoreBackup.ViewModels.ConfigurationViewModels;
using CoreBackup.Models.IO;

namespace CoreBackup.ViewModels.ConfigurationViewModels
{
    class FTPConfViewModel : ViewModelBase
    {
        #region Paths
        // SOURCE DIRECTORY PATH - UPLOAD
        private string _uploadPath;
        public string UploadPath
        {
            get => _uploadPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _uploadPath, value);
                FtpConfig.provideUploadPath(value);
            }
        }

        // DESTINATION DIRECTORY PATH - DOWNLOAD
        private string _downloadPath;
        public string DownloadPath
        {
            get => _downloadPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloadPath, value);
                FtpConfig.provideDownloadPath(value);
            }
        }

        // DISPOSABLE UPLOAD - SOURCE FILE PATH
        private string _disposableUploadPath;

        public string DisposableUploadPath
        {
            get => _disposableUploadPath;
            set => this.RaiseAndSetIfChanged(ref _disposableUploadPath, value);
        }

        #endregion

        #region Reactive Commands
        
        private ReactiveCommand<Unit, Unit> BrowseDownloadDirectoryCommand { get; }
        private ReactiveCommand<Unit, Unit> BrowseUploadDirectoryCommand { get; }
        private ReactiveCommand<Unit, Unit> BrowseDisposableFileUploadCommand { get; }

        private ReactiveCommand<Unit, Unit> ConnectFtpCommand { get; }
        private ReactiveCommand<Unit, Unit> RemoteServerActionCommand { get; }

        #endregion

        private FTP FtpClient;
        private FTPConfig FtpConfig;
        public FTPConfViewModel(ref FTP ftpClient, ref FTPConfig ftpConfig)
        {
            this.FtpClient = ftpClient;
            this.FtpConfig = ftpConfig;
            BrowseDownloadDirectoryCommand = ReactiveCommand.Create(BrowseDownloadDirectory);
            BrowseUploadDirectoryCommand = ReactiveCommand.Create(BrowseUploadDirectory);
            BrowseDisposableFileUploadCommand = ReactiveCommand.Create(BrowseDisposableFileUpload);
            RemoteServerActionCommand = ReactiveCommand.Create(FtpAction);
            ConnectFtpCommand = ReactiveCommand.Create(FtpConnect);
            _ftpFiles = new ObservableCollection<string>();
        }

        public void OnSavedConfigurationEvent(object o, ConfigurationEventArgs e)
        {
            Debug.WriteLine("OnSavedConfiguration event successfully raised");
            if(e.DataType == 2)
            {
                Debug.WriteLine("DataType: " + e.DataType + ", Side: " + e.Side);
                if (e.Side == 0)
                {
                    e.ConfigHub.AddLeftSources(FtpConfig);
                }
                else if (e.Side == 1)
                {
                    e.ConfigHub.AddRightSources(FtpConfig);
                }
            }
        }


        private async void BrowseDownloadDirectory()
        {
            DownloadPath = await GetPath(false);
        }

        private async void BrowseUploadDirectory()
        {
            UploadPath = await GetPath(false);
        }

        private async void BrowseDisposableFileUpload()
        {
            DisposableUploadPath = await GetPath(true);
        }

        
        private async Task<string> GetPath(bool DisposableUploadFile)
        {
            string[] resultReturn = null;
            string fullPath = null;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {

                if (!DisposableUploadFile)
                {
                    OpenFolderDialog dialog = new OpenFolderDialog();
                    string result = await dialog.ShowAsync(desktopLifetime.MainWindow);
                    fullPath = result;
                }
                else
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    string[] result = await dialog.ShowAsync(desktopLifetime.MainWindow);
                    resultReturn = result;
                    fullPath = string.Join(" ", resultReturn);
                    dialog.AllowMultiple = true;
                    string[] PathTreeSteps = fullPath.Split('\\');
                    ToUploadFile = PathTreeSteps[PathTreeSteps.Length - 1];
                }

            }
            return fullPath;
        }

        // FTP SERVER CONFIGURATION //
        #region FTP Configuration Fields

        

        private string _username;
        public string UsernameInput
        {
            get => _username;
            set {
                this.RaiseAndSetIfChanged(ref _username, value);
            }
        }

        private string _password;

        public string PasswordInput
        {
            get => _password;
            set {
                this.RaiseAndSetIfChanged(ref _password, value);
            }
        }

        private string _server;

        public string ServerInput
        {
            get => _server;
            set {
                this.RaiseAndSetIfChanged(ref _server, value);
            }
        }

        private int _cBoxSelectedIdx;
        public int CBoxSelectedIdx
        {
            get => _cBoxSelectedIdx;
            set {
                this.RaiseAndSetIfChanged(ref _cBoxSelectedIdx, value);
                // Clear Fields in XAML
                if (_cBoxSelectedIdx == 0)
                {
                    IsUpload = false;
                    IsDownload = true;
                    ListFiles();
                }
                else if (_cBoxSelectedIdx == 1)
                {
                    IsUpload = true;
                    IsDownload = false;
                    ListFiles();
                }
            }
        }

        // True if Upload Option is a ComboBox's choice
        private bool _isUpload;
        public bool IsUpload
        {
            get => _isUpload;
            set => this.RaiseAndSetIfChanged(ref _isUpload, value);
        }

        // True if Download Option is a ComboBox's choice
        private bool _isDownload = true;
        public bool IsDownload
        {
            get => _isDownload;
            set => this.RaiseAndSetIfChanged(ref _isDownload, value);
        }

        // Validation State
        private bool _isLogged;
        public bool IsLogged
        {
            get => _isLogged;
            set => this.RaiseAndSetIfChanged(ref _isLogged, value);
        }


        // Name of the file which is going to be Downloaded
        private string _toDownloadFile;
        public string ToDownloadFile
        {
            get => _toDownloadFile;
            set => this.RaiseAndSetIfChanged(ref _toDownloadFile, value);
        }

        // Name of the file which is going to be Uploaded
        private string _toUploadFile;

        public string ToUploadFile
        {
            get => _toUploadFile;
            set => this.RaiseAndSetIfChanged(ref _toUploadFile, value);
        }

        #endregion

        #region FTP Actions
        // Collection contains the names of files on the server
        private ObservableCollection<string> _ftpFiles;
        public ObservableCollection<string> FtpFiles
        {
            get => _ftpFiles;
            set => this.RaiseAndSetIfChanged(ref _ftpFiles, value);
        }


        // Get All files' names and inject them to local Collection
        private void ListFiles()
        {
            _ftpFiles.Clear();
            FtpClient.GetFileList();
            foreach (var item in FtpClient.directories)
            {
                _ftpFiles.Add(item);
            }
        }

        // Collection contains the validation failures
        private BindingList<string> errors = new BindingList<string>();
        private ObservableCollection<string> _errorMessages = new ObservableCollection<string>();

        public ObservableCollection<string> ErrorMessages
        {
            get => _errorMessages;
            set => this.RaiseAndSetIfChanged(ref _errorMessages, value);
        }

        private void FtpConnect()
        {
            errors.Clear();
            ErrorMessages.Clear();
            FTPConfViewModelValidator validator = new FTPConfViewModelValidator();
            //ConfigurationViewModelValidator validator = new ConfigurationViewModelValidator();
            ValidationResult results = validator.Validate(this);

            if (results.IsValid == false)
            {
                foreach (ValidationFailure failure in results.Errors)
                {
                    errors.Add($"{failure.PropertyName} : {failure.ErrorMessage}");
                    ErrorMessages.Add($"{failure.ErrorMessage}");
                }
            }
            else
            {
                FtpClient.Username = UsernameInput;
                FtpClient.Password = PasswordInput;
                FtpClient.Server = ServerInput;
                
                try
                {
                    IsLogged = FtpClient.ValidateLogging();
                    if (IsLogged)
                    {
                        FtpConfig.provideCredentials(UsernameInput, PasswordInput, ServerInput);
                        ListFiles();
                    }
                    else
                    {
                        ErrorMessages.Add("Access denied - wrong Username / Password / IP");
                    }

                }
                catch (NullReferenceException) { }
                catch (Exception) { }
            }
        }

        // DISPOSABLE  DOWNLOAD or UPLOAD Action
        private void FtpAction()
        {
            if (IsDownload)
            {
                FtpClient.Download(ToDownloadFile, DownloadPath);
            }
            else if (IsUpload)
            {
                FtpClient.Upload(ToUploadFile, DisposableUploadPath);
            }
        }

        #endregion
    }
}
