﻿using FluentValidation;
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
        .Must(IsNumberOfFour)
        .WithMessage("Please enter a valid Year.");

        RuleFor(sf => sf.Month_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidMonth)
        .WithMessage("Please enter a valid Month.");

        RuleFor(sf => sf.Location_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidLocation).WithMessage("Please enter a valid Location.");

        RuleFor(sf => sf.Amount_v)
        .Cascade(CascadeMode.Continue)
        .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");
    }

    public bool IsAValidMonth(string month)
    {
        
         var regex = new Regex("(^0?[1-9]$)|(^1[0-2]$)");
         return regex.IsMatch(month);

    }
    public bool IsAValidLocation(string location)
    {
        String[] s = { "IND", "DEN", "PIT", "FTW" };
        for (int i = 0; i < s.Length; i++)
        {
            if (location == s[i])
                return true;
        }
        return false;
    }

    public bool IsAValidAmount(string amount)
    {
        var regex = new Regex("^-?\\d*(\\.\\d+)?$");

        return regex.IsMatch(amount);
    }

    public bool IsNumberOfFour(string year)
    {
       var regex = new Regex("^[0-9]+$");

        return regex.IsMatch(year) && year.Length.Equals(4);
    }
}
