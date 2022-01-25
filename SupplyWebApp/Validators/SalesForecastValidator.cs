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



    public class SalesForecastValidator :  AbstractValidator<SalesForecast>
    {
       public SalesForecastValidator()
       {
          RuleFor(sf => sf.Year.ToString().Length)
          .Cascade(CascadeMode.Continue)
          .Equal(4).WithMessage("Please enter  a valid year");

          RuleFor(sf => sf.Month)
          .Cascade(CascadeMode.Continue)
          .Must(BeAValidMonth)
          .WithMessage("please enter a valid month");

          RuleFor(sf => sf.Location)
          .Cascade(CascadeMode.Continue)
          .Must(BeAValidLocation).WithMessage("please enter a valid location");
       }

       public bool BeAValidMonth(int month) 
       {
          for (int i = 1; i <= 12; i++)
          {
              if(month == i)
                return true;
           
          }
          return false;
       }
       public bool BeAValidLocation(string location)
       {
           String[] s = {"IND","DEN","PIT","FTW"};
           for (int i = 0; i < s.Length; i++)
           {
               if(location == s[i])
                return true;
           }
           return false;
       }
    }
