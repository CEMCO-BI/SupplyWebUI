using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SupplyWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SupplyWebApp.Helpers.Enums;
using Microsoft.Extensions.Configuration;
using ExcelDataReader;
using SupplyWebApp.Models;
using System.Text.RegularExpressions;




public class SalesForecastValidator : AbstractValidator<SalesForecastValidateObj>
{
    public SalesForecastValidator()
    {
        RuleFor(sf => sf.Year_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsValidYear)
        .WithMessage("Year cannot be empty. Please enter year in YYYY format.");

        RuleFor(sf => sf.Month_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidMonth)
        .WithMessage("Month cannot be empty. Please enter month in MM format in the range 1-12.");

        RuleFor(sf => sf.Location_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidLocation).WithMessage("Location cannot be empty. Please Enter valid Location");

        RuleFor(sf => sf.Amount_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidAmount).WithMessage("Amount cannot be empty.Please enter a numeric value for Amount.");
    }

    public bool IsAValidMonth(string month)
    {
        try {
            var regex = new Regex("(^0?[1-9]$)|(^1[0-2]$)");
            return regex.IsMatch(month);
        } catch (Exception ex) {
            return false;
                }

    }
    public bool IsAValidLocation(string location)
    {
        try {
            String[] s = { "IND", "DEN", "PIT", "FTW" };
            for (int i = 0; i < s.Length; i++)
            {
                if (location == s[i])
                    return true;
            }
            return false;
        }
        catch (Exception ex) {
            return false;
        }
        
    }

    public bool IsAValidAmount(string amount)
    {
        try {
            bool result;
            if(amount == "")
            {
                result = false;
            }
            else
            {
                var regex = new Regex("^-?\\d*(\\.\\d+)?$");
                result = regex.IsMatch(amount);
            }
            return result;
        }
        catch (Exception ex) {

            return false;
        }

    }

    public bool IsValidYear(string year)
    {
        try {
            var regex = new Regex("^[0-9]+$");
            return regex.IsMatch(year) && year.Length.Equals(4);

        }
        catch (Exception ex) {
            return false;
        }
    }
}
