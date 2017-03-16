using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Dimond.QuickSignum
{
	public interface IQuickSignum
	{
	    void Dispose();

	    X509Certificate2 CurrentCert { get; }

		/// <summary>
		/// Метод получения списка личных сертификатов пользователя
		/// </summary>
		/// <returns>Возвращает список личных сертификатов</returns>
		X509Certificate2Collection GetCurrentUserMyCerts();

		/// <summary>
		/// Метод получения личного сертификата пользователя по имени сертификата
		/// </summary>
		/// <returns>Возвращает личный сертификат</returns>
		X509Certificate2 GetCurrentUserMyCertByName(String certName);

		/// <summary>
		/// Метод получения личного сертификата пользователя по имени сертификата
		/// </summary>
		/// <returns>Возвращает личный сертификат</returns>
		X509Certificate2 GetCurrentUserMyCertBySerial(String certSerial);


	    void SetCurrentCertBySerial(string certName);

		/// <summary>
		/// Метод подписи масива данных
		/// </summary>
		/// <param name="docByteArray">Байтовый массив данных, которые требуется подписать</param>
		/// <param name="signerCert">Сертифиакт которым будет производится подпись</param>
		/// <returns>Подпист в виде массива байтов</returns>
		Byte[] Sign(Byte[] docByteArray);

		/// <summary>
		/// Метод проверки подписи масива данных
		/// </summary>
		/// <param name="docByteArray">Байтовый массив данных, которые подписаны</param>
		/// <param name="signature">Байтовый массив подписи, которые надо проверить</param>
		/// <returns>Возвращает true если подипсь верна</returns>
		Boolean Verify(Byte[] docByteArray, Byte[] signature);

		/// <summary>
		/// Метод подписи файла
		/// </summary>
		/// <param name="pathSignedFile">Путь к файлу, который требуется подписать</param>
		/// <param name="signerCert">Сертифиакт которым будет производится подпись</param>
		/// <returns>Путь до файла подписи</returns>
		String Sign(String pathSignedFile);

		/// <summary>
		/// Метод проверки подписи файла
		/// </summary>
		/// <param name="pathSignedFile">Путь к файлу, который подписан</param>
		/// <param name="pathsignature">Путь до файла подписи, которую надо проверить</param>
		/// <returns>Возвращает true если подипсь верна</returns>
		Boolean Verify(String pathSignedFile, String pathsignature);

		/// <summary>
		/// Получение массива байтов из файла
		/// </summary>
		/// <param name="fullFilePath">Путь до файла</param>
		/// <returns>Возвращает массив байтов</returns>
		Byte[] GetByteArrayFromFile(String fullFilePath);

		/// <summary>
		/// Создание файла подписи
		/// </summary>
		/// <param name="bytes">Массив байтов подписи</param>
		/// <param name="fullOutFilePath">Путь для записи файла</param>
		/// <returns>Если все записалось то true</returns>
		void WriteByteArray(String fullOutFilePath, Byte[] bytes);

        /// <summary>
        /// Списо серийных номеров сертификатов и намименования сертификата
        /// </summary>
        Dictionary<string, string> CertificateCollection { get; }

	    void ViewCertificateInformation(IntPtr parrent);
	}
}
