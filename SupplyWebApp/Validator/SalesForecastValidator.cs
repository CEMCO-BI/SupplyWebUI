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




public class SalesForecastValidator : AbstractValidator<SalesForecast>
{
    public SalesForecastValidator()
    {
        RuleFor(sf => sf.Year.ToString().Length)
        .Cascade(CascadeMode.Continue)
        .Equal(4).WithMessage("Please enter a valid Year.");

        RuleFor(sf => sf.Month)
        .Cascade(CascadeMode.Continue)
        .Must(isAValidMonth)
        .WithMessage("Please enter a valid Month.");

        RuleFor(sf => sf.Location)
        .Cascade(CascadeMode.Continue)
        .Must(isAValidLocation).WithMessage("Please enter a valid Location.");

        RuleFor(sf => sf.Amount.ToString())
        .Cascade(CascadeMode.Continue)
        .Must(isAValidAmount).WithMessage("Please enter a numeric value for Amount.");
    }

    public bool isAValidMonth(int month)
    {
        for (int i = 1; i <= 12; i++)
        {
            if (month == i)
                return true;

        }
        return false;
    }
    public bool isAValidLocation(string location)
    {
        String[] s = { "IND", "DEN", "PIT", "FTW" };
        for (int i = 0; i < s.Length; i++)
        {
            if (location == s[i])
                return true;
        }
        return false;
    }

    public bool isAValidAmount(string amount)
    {
        var regex = new Regex("[+-]?([0-9]*[.])?[0-9]+");

        return regex.IsMatch(amount);
    }
}
