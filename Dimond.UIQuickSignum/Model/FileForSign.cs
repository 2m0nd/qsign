using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace UIQuickSignum.Model
{
    public class FileForSign : Microsoft.Practices.Prism.ViewModel.NotificationObject
    {
        #region Fields

        private bool _canSign = true;
        private bool _isSign;

        #endregion

        #region .ctors

        public FileForSign(FileEx fileEx)
        {
            FileEx = fileEx;
        }

        #endregion

        #region Binding propertiess

        [DisplayName(@"Подписать?")]
        public bool CanSign
        {
            get { return _canSign; }
            set
            {
                _canSign = value;
                RaisePropertyChanged(() => CanSign);
            }
        }

        [DisplayName(@"Подписан")]
        public bool IsSign
        {
            get { return _isSign; }
            set
            {
                if (value.Equals(_isSign)) return;
                _isSign = value;
                RaisePropertyChanged(() => IsSign);
            }
        }

        [DisplayName(@"Тип файла")]
        public string Extension
        {
            get { return GetExtension(); }
        }

        private string GetExtension()
        {
            var extension = Path.GetExtension(FilePath ?? string.Empty) ?? string.Empty;
            return extension.ToUpper();
        }

        [DisplayName(@"Путь к файлу")]
        public string FilePath
        {
            get { return FileEx.ToString(); }
        }

        #endregion

        private FileEx FileEx { get; set; }

        public string SourceFilePath { get { return FileEx.SourcePath; } }

        public void SaveSignature(byte[] signature)
        {
            FileEx.SaveSignature(signature);
            this.IsSign = true;
        }

        public byte[] GetFileAsByteArray()
        {
            return FileEx.GetFileAsByteArray();
        }
        public byte[] GetSignatureAsByteArray()
        {
            return FileEx.GetSignatureAsByteArray();
        }
    }
}
