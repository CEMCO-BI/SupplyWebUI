using Microsoft.AspNetCore.Http;
using SupplyWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SupplyWebApp.Helpers.Enums;

namespace SupplyWebApp.Services
{
    public sealed class ImportService
    {
        private static IDictionary<FileNames, Type> _fileImporters = new Dictionary<FileNames, Type>();
        private DataContext _dataContext;

        public ImportService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public static void RegisterImporter(FileNames file, Type reportImporter)
        {
            _fileImporters.Add(file, reportImporter);
        }

        public void Import(FileNames fileName, IFormFile file)
        {
            CreateImporter(fileName).Import(file);
        }

        private Importer CreateImporter(FileNames fileName)
        {
            Importer service = (Importer)Activator.CreateInstance(_fileImporters[fileName]);
            service.DataContext = _dataContext;
            return service;
        }

    }
}
