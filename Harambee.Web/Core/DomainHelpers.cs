using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harambee.Web.Core
{
	public class DomainHelpers : IDomainHelper
	{
		public User SetUserPasswordData(ref User user, string tempPin)
		{
			string[] encryptPassword = GenerateKeySalt(tempPin.ToString());

			user.Password = encryptPassword[0];
			user.Salt = encryptPassword[1];
			user.IsTempPin = true;
			user.IsLockedOut = false;
			user.FailedLoginAttempts = 0;

			return user;
		}

		public APICorrespondenceSMSViewModel SetSmsMessage(string tenantMessage, string tempPin, string mobileNumber)
		{
			var smsMessage = new APICorrespondenceSMSViewModel()
			{
				MobileMessage = string.Format(tenantMessage, tempPin),
				MobileNumber = mobileNumber,
				Source = "Harambee Mobi",
				Reference = string.Format("{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)
			};

			return smsMessage;
		}

		public APICorrespondenceEmailViewModel SetEmailMessage(string userName, string userLastName, string userIDNumber, string mobileNumber, string tempPin, string emailAddress)
		{
			var email = new APICorrespondenceEmailViewModel()
			{
				Source = "Harambee Mobi",
				BodyIsHtml = true,
				EmailBody = string.Format("Good Day, please assist {0} {1} ({2}) in resetting his/her harambee mobi password <br/> Please phone the user on : {3} <br/> Please supply the user with the following temporary password : {4}", userName, userLastName, userIDNumber, mobileNumber, tempPin),
				EmailTo = emailAddress,
				EmailSubject = "Mobi Password Reset Assistance",
				Reference = string.Format("{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)
			};

			return email;
		}

		public APICorrespondenceEmailViewModel SetNotValidatedEmailMessage(string mobileNumber, string emailAddress)
		{
			var email = new APICorrespondenceEmailViewModel()
			{
				Source = "Harambee Mobi",
				BodyIsHtml = false,
				EmailBody = string.Format("Good Day, a candidate tried to access the mobi portal various times without success, please give the person a call on {0}", mobileNumber),
				EmailTo = emailAddress,
				EmailSubject = "Mobi Forgot Pin : Validation",
				Reference = string.Format("{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)
			};

			return email;
		}



		#region Password Reset Code
		public static string[] GenerateKeySalt(string plainTxtPass)
		{
			string[] s = new string[2];
			string salt = GenerateRandomSalt();
			string passAndSalt = plainTxtPass + salt;
			string hash = GetHashKey(passAndSalt);

			s[0] = hash;
			s[1] = salt;

			return s;
		}
		#region Generate random salt

		public static string GenerateRandomSalt()
		{
			var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
			var buff = new byte[5];
			rng.GetBytes(buff);
			return Convert.ToBase64String(buff);

			//using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
			//{
			//	byte[] saltInBytes = new byte[5];
			//	crypto.GetBytes(saltInBytes);
			//	return Convert.ToBase64String(saltInBytes);
			//}
		}

		#endregion

		#region Get the hash key for a given string
		public static string GetHashKey(string input)
		{
			byte[] tmpSource;
			byte[] tmpHash;
			//Convert input to a Byte Array (needed for hash function)
			tmpSource = Encoding.ASCII.GetBytes(input);
			//Compute hash based on source data.
			var hashAlgorithm = System.Security.Cryptography.MD5.Create();
			tmpHash = hashAlgorithm.ComputeHash(tmpSource);
			//tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
			return Convert.ToBase64String(tmpHash);

			//byte[] tmpSource;
			//byte[] tmpHash;
			////Convert input to a Byte Array (needed for hash function)
			//tmpSource = Encoding.ASCII.GetBytes(input);
			////Compute hash based on source data.
			//tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
			//return Convert.ToBase64String(tmpHash);
		}
		#endregion

		public static string[] EncryptAnswer(string Password, string Action, string AnswerSalt)
		{
			string[] answerdetails = new string[3];

			string salt = "";
			if (Action == "create")
			{
				salt = GenerateRandomSalt();
			}
			else if (Action == "view")
			{
				salt = AnswerSalt;
			}

			string passAndSalt = Password + salt;
			string hash = GetHashKey(passAndSalt);
			answerdetails[0] = Password;
			answerdetails[1] = hash;
			answerdetails[2] = salt;

			return answerdetails;
		}

		internal static bool ValidatePassword(string SuppliedPassword, string ActualPassword, string Salt)
		{
			string hashKey = SuppliedPassword + Salt;
			string hash = GetHashKey(hashKey);

			if (hash == ActualPassword)
			{
				return true;
			}

			return false;

		}

		#endregion
	}
}
