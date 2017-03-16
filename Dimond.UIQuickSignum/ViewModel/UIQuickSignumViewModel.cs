using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Dimond.QuickSignum;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.Prism.Commands;
using UIQuickSignum.Model;
using UIQuickSignum.Properties;
using UIQuickSignum.View;
using DataObject = System.Windows.DataObject;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace UIQuickSignum.ViewModel
{
    public class UiQuickSignumViewModel : ViewModelBase, IDisposable
    {
        #region Fields

        private static LicenseRequestView _licenseRequestView;

        private readonly IQuickSignum _quickSignum = new QuickSignum(LicenseRequesAction, ErrorWhenCheckLicense, SucessfulActivating);

        private static void SucessfulActivating()
        {
            MessageBox.Show("Вы успешно активировали программное обеспечение.");
        }

        private static void ErrorWhenCheckLicense(Exception exception)
        {
            if (App.Current != null)
                App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        {
                            var result = MessageBox.Show(exception.Message + "\nВыйти из приложения?",
                                                         "Результат лицензирования",
                                                         MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                                App.Current.Shutdown();

                        }
                    }));
        }

        private static string LicenseRequesAction(string s)
        {
            var returnedOject = App.Current.Dispatcher.Invoke(new Func<string, string>(Target), s);

            return returnedOject.ToString();
        }

        private static string Target(string s)
        {
            _licenseRequestView = new LicenseRequestView(s) { Topmost = true };
            var returnedKey = _licenseRequestView.OpenRequestDialog();
            return returnedKey;
        }

        //private static string LicenseRequesAction(string serialNumber)
        //{

        //    string key = await App.Current.Dispatcher.InvokeAsync(new Func<string, string>((s) => Do(serialNumber).Result));
        //    return key;
        //}

        //async static Task<string> Do(string s)
        //{
        //    var winRequestLicense = new LicenseRequestView(s) { Topmost = true };
        //    var returnedKey = winRequestLicense.OpenRequestDialog();
        //    return returnedKey;
        //}

        private Dictionary<string, string> _certificationList;
        private string _currentDirectory;
        private string _selectedCertificate;
        private List<FileForSign> _filesForSign;
        private FileForSign _selectedFileForSign;
        private bool _signInZip;

        #endregion

        #region Commands

        public ICommand OpenDirectoryCommand
        {
            get { return new RelayCommand(OpenDirectoryExecuteCommand); }
        }

        private void OpenDirectoryExecuteCommand()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var path = dialog.SelectedPath;
                var filesForSign = GetAllFilesFromPathes(new List<string> { path });
                FilesForSign = filesForSign;
            }
        }

        public ICommand OpenFilesCommand
        {
            get { return new RelayCommand(OpenFilesExecuteCommand); }
        }

        private void OpenFilesExecuteCommand()
        {
            var dlg = new OpenFileDialog { Multiselect = true };

            if (dlg.ShowDialog() ?? false)
            {
                var pathes = dlg.FileNames.ToList();
                var filesForSign = GetAllFilesFromPathes(pathes);
                FilesForSign = filesForSign;
            }
        }

        public ICommand DropCommand
        {
            get { return new RelayCommand<DragEventArgs>(DropExecuteCommand); }
        }

        private void DropExecuteCommand(DragEventArgs arg)
        {
            var dropTask = new Task(() =>
            {
                if (arg.Data is DataObject && ((DataObject) arg.Data).ContainsFileDropList())
                {
                    var dropPathes = ((DataObject) arg.Data).GetFileDropList().Cast<string>().ToList();
                    FilesForSign = GetAllFilesFromPathes(dropPathes);
                }
                else
                {
                    MessageBox.Show("Не найдено ни одного файла для подписи");
                }
            });
            dropTask.Start();
        }

        private List<FileForSign> GetAllFilesFromPathes(List<String> pathes)
        {
            if (pathes.Count >= 1)
            {
                var sourceFilePathes = new List<string>();
                foreach (var path in pathes)
                {
                    //папка
                    if (Directory.Exists(path))
                    {
                        sourceFilePathes.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList());
                        CurrentDirectory = path;
                    }
                    //файл
                    else if (File.Exists(path))
                    {
                        sourceFilePathes.Add(path);
                        CurrentDirectory = path;
                    }
                }

                var fileExs = new List<FileEx>();
                foreach (var sourceFilePath in sourceFilePathes)
                {
                    var fileExtension = Path.GetExtension(sourceFilePath);
                    var lowredFileExtension = fileExtension != null ? fileExtension.ToLower() : null;

                    if (lowredFileExtension != null && lowredFileExtension.Equals(QuickSignum.SignExtension))
                        continue;

                    var isZipFile = lowredFileExtension != null && lowredFileExtension.Equals(".zip");
                    if (SignInZip && isZipFile)
                    {
                        var createdFileExs = FileEx.CreateFileExs(sourceFilePath, FileEx.FileType.Zip);
                        fileExs.AddRange(createdFileExs);
                    }
                    else
                    {
                        var createdFileExs = FileEx.CreateFileExs(sourceFilePath, FileEx.FileType.File);
                        fileExs.AddRange(createdFileExs);
                    }
                }


                var filesForSign = fileExs.Select(f => new FileForSign(f))
                                        .Where(f => !f.Extension.ToLowerInvariant().Equals(QuickSignum.SignExtension.ToLowerInvariant()))
                                        .ToList();
                if (filesForSign.Count > 0)
                    return filesForSign;
            }

            MessageBox.Show("Не найдено ни одного файла для подписи");
            return new List<FileForSign>();
        }

        public ICommand UnCanSignCommand
        {
            get { return new RelayCommand<object>(UnCanSignExecuteCommand); }
        }

        private void UnCanSignExecuteCommand(object args)
        {
            var r = ((System.Collections.IList)args).Cast<FileForSign>();
            r.ToList().ForEach(f =>
                {
                    f.CanSign = false;
                });
        }

        public ICommand CanSignCommand
        {
            get { return new RelayCommand<object>(CanSignExecuteCommand); }
        }

        private void CanSignExecuteCommand(object args)
        {
            var r = ((System.Collections.IList)args).Cast<FileForSign>();
            r.ToList().ForEach(f =>
            {
                f.CanSign = true;
            });
        }

        public ICommand CanSignAllThisExtensionCommand
        {
            get { return new RelayCommand(CanSignAllThisExtensionExecuteCommand); }
        }

        private void CanSignAllThisExtensionExecuteCommand()
        {
            FilesForSign.Where(f => f.Extension.Equals(SelectedFileForSign.Extension))
                        .ToList()
                        .ForEach(f => f.CanSign = true);
        }

        public ICommand UnCanSignAllThisExtensionCommand
        {
            get { return new RelayCommand(UnCanSignAllThisExtensionExecuteCommand); }
        }

        private void UnCanSignAllThisExtensionExecuteCommand()
        {
            FilesForSign.Where(f => f.Extension.Equals(SelectedFileForSign.Extension))
                        .ToList()
                        .ForEach(f => f.CanSign = false);
        }

        public ICommand SignCommand
        {
            get
            {
                return new RelayCommand(SignExecuteCommand, () =>
                    !_nowSign &&
                    FilesForSign != null &&
                    FilesForSign.Any(f => f.CanSign) &&
                    _quickSignum.CurrentCert != null);
            }
        }

        private void SignExecuteCommand()
        {
            var task = new Task(SignAllSelectedFiles);
            task.Start();
        }

        private bool _nowSign = false;
        void SignAllSelectedFiles()
        {
            _nowSign = true;
            var errors = new List<string>();
            foreach (var file in FilesForSign.Where(f => f.CanSign))
            {
                try
                {
                    var fileAsArray = file.GetFileAsByteArray();
                    var signAsArray = _quickSignum.Sign(fileAsArray);
                    file.SaveSignature(signAsArray);
                }
                catch (Exception exception)
                {
                    errors.Add(string.Format("При обработке файла: {0}\nвозникла следующая ошибка '{1}'", file.FilePath, exception.Message));
                }
            }
            if (errors.Count == 0)
                App.Current.Dispatcher.Invoke(
                    new Action(() => 
                        MessageBox.Show(App.Current.MainWindow, String.Format("Все файлы успешно подписаны"))));
            else
            {
                App.Current.Dispatcher.Invoke(
                    new Action(() => 
                MessageBox.Show(App.Current.MainWindow, String.Format("Успешно подписано {0} файлов, возникло ошибок {1}, ошибки: {2}", FilesForSign.Count - errors.Count,
                    errors.Count, errors.Aggregate((s, s1) => s + "\n\n" + s1)))));
            }
            _nowSign = false;
        }

        

        public ICommand DonateCommand
        {
            get
            {
                return new RelayCommand<object>(DonateExecuteCommand);
            }
        }

        private void DonateExecuteCommand(object parametr)
        {
            var amount = (int)parametr;
            if (amount < 0 || amount > 1000)
                amount = 100;

            var url = string.Format("{0}{1}", "http://market.2m0nd.ru/QuickSignumDonate?amount=", amount);

            try
            {
                System.Diagnostics.Process.Start("chrome", url);
            }
            catch (Exception excC)
            {
                try
                {
                    System.Diagnostics.Process.Start("opera", url);
                }
                catch (Exception excO)
                {
                    try
                    {
                        System.Diagnostics.Process.Start("firefox", url);
                    }
                    catch (Exception excF)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("iexplore", url);
                        }
                        catch (Exception excIE)
                        {
                            MessageBox.Show(
                                "Скопируйте содержимое запроса ключа активации и запросите ключ на сайте магазина программного обеспечения.");
                        }
                    }
                }
            }
        }

        public ICommand VerefySignCommand
        {
            get
            {
                return new RelayCommand(VerefySignExecuteCommand, () =>
                    !_nowVerify &&
                  FilesForSign != null &&
                  FilesForSign.Any(f => f.CanSign) &&
                  _quickSignum.CurrentCert != null);
            }
        }

        private void VerefySignExecuteCommand()
        {
            var task = new Task(VerifyAllSignatures);
            task.Start();
        }

        private bool _nowVerify = false;
        void VerifyAllSignatures()
        {
            _nowVerify = true;

            var errors = new List<string>();
            int signend = 0;
            int unsigned = 0;
            foreach (var file in FilesForSign.Where(f => f.CanSign))
            {
                try
                {
                    var signatureArray = file.GetSignatureAsByteArray();
                    if (signatureArray == null)
                    {
                        file.IsSign = false;
                        unsigned++;
                        continue;
                    }

                    var sourceArray = file.GetFileAsByteArray();
                    file.IsSign = _quickSignum.Verify(sourceArray, signatureArray);
                    if (file.IsSign)
                        signend++;
                    else
                        unsigned++;
                }
                catch (Exception exception)
                {
                    errors.Add(string.Format("При обработке файла: {0}\nвозникла следующая ошибка '{1}'", file.FilePath, exception.Message));
                }
            }
            if (errors.Count == 0)
                App.Current.Dispatcher.Invoke(
                    new Action(() => 
                MessageBox.Show(App.Current.MainWindow, String.Format("Все подписи успешно проверены, подписано {0} файлов, без подписи {1} файлов", signend, unsigned))));
            else
            {
                App.Current.Dispatcher.Invoke(
                    new Action(() => 
                MessageBox.Show(App.Current.MainWindow, String.Format("Подписано {0} файлов, не подписано {1}, возникло ошибок {2}, ошибки: {3}", signend, unsigned,
                    errors.Count, errors.Aggregate((s, s1) => "\n" + s + "\n\n" + s1)))));
            }

            _nowVerify = false;
        }

        public ICommand ViewCertDitailCommand
        {
            get { return new DelegateCommand(ViewCertDitailExecuteCommand); }
        }

        private void ViewCertDitailExecuteCommand()
        {
            var parrent = new WindowInteropHelper(App.Current.MainWindow).Handle;
            _quickSignum.ViewCertificateInformation(parrent);
        }

        public ICommand SelectAllCommand
        {
            get { return new RelayCommand(SelectAllExecuteCommand); }
        }

        private void SelectAllExecuteCommand()
        {
            FilesForSign.ForEach(f => f.CanSign = true);
        }

        public ICommand DeselectAllCommand
        {
            get { return new RelayCommand(DeselectAllExecuteCommand); }
        }

        private void DeselectAllExecuteCommand()
        {
            FilesForSign.ForEach(f => f.CanSign = false);
        }

        #endregion

        #region Properties

        public bool SignInZip
        {
            get { return _signInZip; }
            set
            {
                if (value.Equals(_signInZip)) return;
                _signInZip = value;

                if (FilesForSign != null)
                    FilesForSign = GetAllFilesFromPathes(FilesForSign.Select(sign => sign.SourceFilePath).Distinct().ToList());

                RaisePropertyChanged(() => SignInZip);
            }
        }

        public FileForSign SelectedFileForSign
        {
            get { return _selectedFileForSign; }
            set
            {
                if (Equals(value, _selectedFileForSign)) return;
                _selectedFileForSign = value;
                RaisePropertyChanged(() => SelectedFileForSign);
            }
        }

        public List<FileForSign> FilesForSign
        {
            get { return _filesForSign ?? new List<FileForSign>(); }
            set
            {
                if (Equals(value, _filesForSign)) return;
                _filesForSign = value;
                RaisePropertyChanged(() => FilesForSign);
            }
        }

        public string SelectedCertificate
        {
            get { return _selectedCertificate; }
            set
            {
                if (value == _selectedCertificate) return;
                _selectedCertificate = value;
                _quickSignum.SetCurrentCertBySerial(value);
                RaisePropertyChanged(() => SelectedCertificate);
            }
        }

        public String CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                if (value == _currentDirectory) return;
                _currentDirectory = value;
                RaisePropertyChanged(() => CurrentDirectory);
            }
        }

        public Dictionary<string, string> CertificationList
        {
            get { return _certificationList; }
            set
            {
                if (Equals(value, _certificationList)) return;
                _certificationList = value;
                RaisePropertyChanged(() => CertificationList);
            }
        }

        #endregion

        #region .ctor

        public UiQuickSignumViewModel()
        {
            CertificationList = _quickSignum.CertificateCollection;
            ApplyConfigs();

            if (string.IsNullOrWhiteSpace(CurrentDirectory))
                return;

            var extension = Path.GetExtension(CurrentDirectory);
            if (!string.IsNullOrWhiteSpace(extension) && extension.ToLowerInvariant().Equals(QuickSignum.SignExtension.ToLowerInvariant()))
                return;
        }

        #endregion

        #region Config load and save

        private void ApplyConfigs()
        {
            SelectedCertificate = CertificationList.FirstOrDefault(c => c.Key == Settings.Default.CertSerialNumber).Key;
            CurrentDirectory = Settings.Default.LastDirectoryPath;
            if (Settings.Default.OpenedFiles != null)
                FilesForSign = GetAllFilesFromPathes(Settings.Default.OpenedFiles.FilePathes);
            SignInZip = Settings.Default.SignInZip;
        }

        private void SaveConfigs()
        {
            var r = new List<string>();
            Settings.Default.CertSerialNumber = SelectedCertificate;
            Settings.Default.LastDirectoryPath = CurrentDirectory;
            Settings.Default.OpenedFiles = new OpenedFiles() { FilePathes = FilesForSign.Select(sign => sign.SourceFilePath).Distinct().ToList() };
            Settings.Default.SignInZip = SignInZip;
            Settings.Default.Save();
        }

        #endregion

        #region Implement IDisposable

        public void Dispose()
        {
            SaveConfigs();
            _quickSignum.Dispose();
        }

        #endregion
    }
}
