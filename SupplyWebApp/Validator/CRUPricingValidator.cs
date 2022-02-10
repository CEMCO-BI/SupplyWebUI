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


    public class CRUPricingValidator : AbstractValidator<CRUPricingValidateObj>
    {
        public CRUPricingValidator() {

        RuleFor(cru => cru.date_v)
       .Cascade(CascadeMode.Continue)
       .Must(IsValidYear)
       .WithMessage("Please enter a valid Year.");

        RuleFor(cru => cru.date_v)
       .Cascade(CascadeMode.Continue)
       .Must(IsAValidMonth)
       .WithMessage("Please enter a valid Month.");

        RuleFor(cru => cru.Week1_v)
       .Cascade(CascadeMode.Continue)
       .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");

        RuleFor(cru => cru.Week2_v)
      .Cascade(CascadeMode.Continue)
      .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");

        RuleFor(cru => cru.Week3_v)
      .Cascade(CascadeMode.Continue)
      .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");

        RuleFor(cru => cru.Week4_v)
      .Cascade(CascadeMode.Continue)
      .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");

        RuleFor(cru => cru.Week5_v)
      .Cascade(CascadeMode.Continue)
      .Must(IsAValidAmount).WithMessage("Please enter a numeric value for Amount.");


    }

    public bool IsValidYear(string date)
    {
        try
        {
            var year = Convert.ToDateTime(date).Year.ToString();
            var regex = new Regex("^[0-9]+$");
            return regex.IsMatch(year) && year.Length.Equals(4);
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool IsAValidMonth(string date)
    {
        try {
            var month = Convert.ToDateTime(date).Month.ToString();
            var regex = new Regex("(^0?[1-9]$)|(^1[0-2]$)");
            return regex.IsMatch(month);
        } catch(Exception ex) {
            return false;
        }

    }

    public bool IsAValidAmount(string amount)
    {
        try {
            var regex = new Regex("^-?\\d*(\\.\\d+)?$");
            return regex.IsMatch(amount);
        }
        catch (Exception ex) {
            return false;
        }
    }
}

