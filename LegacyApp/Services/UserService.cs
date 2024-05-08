using System;
using LegacyApp.Constants;
using LegacyApp.Models;
using LegacyApp.Repositories;
using LegacyApp.Services;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstname, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsAddUserValid(firstname, surname, email, dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            if (client is null)
            {
                return false;
            }

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firstname,
                Surname = surname
            };

            if (client.Name == ClientType.VeryImportantClient)
            {
                // Skip credit chek
                user.HasCreditLimit = false;
            }
            else if (client.Name == ClientType.ImportantClient)
            {
                // Do credit check and double credit limit
                user.HasCreditLimit = true;
                var creditLimit = CalculateCreditLimitForImportantClient(user);
                if (IsCreditLimitValid(creditLimit))
                {
                    user.CreditLimit = creditLimit;
                }

                return false;
            }
            else
            {
                // Do credit check
                user.HasCreditLimit = true;
                var creditLimit = CalculateCreditLimitForCommonClient(user);
                if (IsCreditLimitValid(creditLimit))
                {
                    user.CreditLimit = creditLimit;
                }

                return false;
            }
            
            UserDataAccess.AddUser(user);

            return true;
        }

        private int CalculateCreditLimitForCommonClient(User user)
        {
            using (var userCreditService = new UserCreditServiceClient())
            {
                var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                return creditLimit;
            }
        }

        private int CalculateCreditLimitForImportantClient(User user)
        {
            using (var userCreditService = new UserCreditServiceClient())
            {
                var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
                creditLimit *= 2;
                return creditLimit;
            }
        }

        private bool IsAddUserValid(string firstname, string surname, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(surname))
            {
                return false;
            }

            if (email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            if (!IsAgeValid(dateOfBirth))
            {
                return false;
            }

            return true;
        }

        private bool IsAgeValid(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;

            if (now.Month < dateOfBirth.Month
                || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            if (age < Limits.AgeLimit)
            {
                return false;
            }

            return true;
        }

        private bool IsCreditLimitValid(int creditLimit)
        {
            if (creditLimit < Limits.CreditLimit)
            {
                return false;
            }

            return true;
        }
    }
}