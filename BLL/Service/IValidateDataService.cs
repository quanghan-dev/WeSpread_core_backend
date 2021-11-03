using BLL.Dto;
using System;

namespace BLL.Service
{
    public interface IValidateDataService
    {
        bool IsValidPhone(string phone);

        bool IsValidName(string name, string obj);

        bool IsValidAddress(string address);

        bool IsValidEmail(string email);

        bool IsValidForetime(DateTime? date);

        bool Is15DaysBefore(DateTime startDate, DateTime endDate);

        bool IsValidDonationTarget(double target);

        bool IsValidDonateAmount(long amount);

        bool IsValidDateInProjectPeriod(Period projectPeriod, DateTime startDate, DateTime endDate);

        bool Is7DaysBeforeStartDate(DateTime date, DateTime startDate);
    }
}
