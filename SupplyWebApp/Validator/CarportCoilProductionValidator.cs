﻿using FluentValidation;
using SupplyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SupplyWebApp.Validator
{
    public class CarportCoilProductionValidator: AbstractValidator<CarportCoilProductionValidateObj>
    {
        public CarportCoilProductionValidator()
        {
            RuleFor(sf => sf.Year_v)
            .Cascade(CascadeMode.Continue)
            .Must(IsValidYear)
            .WithMessage("Year cannot be empty. Please enter a valid Year.");

            RuleFor(sf => sf.Month_v)
            .Cascade(CascadeMode.Continue)
            .Must(IsAValidMonth)
            .WithMessage("Month cannot be empty. Please enter a valid Month.");

            //RuleFor(sf => sf.ClassCode_v)
            //.Cascade(CascadeMode.Continue)
            //.Must(IsAValidLocation).WithMessage("Class Code cannot be empty. Please enter a valid Class Code.");

            RuleFor(sf => sf.Amount_v)
            .Cascade(CascadeMode.Continue)
            .Must(IsAValidAmount).WithMessage("Amount cannot be empty. Please enter a numeric value for Amount.");
        }

        public bool IsAValidMonth(string month)
        {
            try
            {
                var regex = new Regex("(^0?[1-9]$)|(^1[0-2]$)");
                return month == "" || regex.IsMatch(month);
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        //public bool IsAValidLocation(string location)
        //{
        //    try
        //    {
        //        String[] s = { "IND", "DEN", "PIT", "FTW" };
        //        for (int i = 0; i < s.Length; i++)
        //        {
        //            if (location == s[i] || location == "")
        //                return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //}

        public bool IsAValidAmount(string amount)
        {
            try
            {
                var regex = new Regex("^-?\\d*(\\.\\d+)?$");
                return regex.IsMatch(amount) || amount == "";
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        public bool IsValidYear(string year)
        {
            try
            {
                var regex = new Regex("^[0-9]+$");
                return regex.IsMatch(year) && year.Length.Equals(4) || year == "";

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
