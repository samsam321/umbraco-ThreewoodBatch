using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThreewoodBatch.Extensions;
using ThreewoodBatch.Models;
using Umbraco.Core.Logging;

namespace ThreewoodBatch.Helper
{
    static public class ExcelHelper
    {
        const int RESOURCES_WORKSHEET = 1;

        static public bool ReadExcelFile(string file, ref ExcelPackage excel)
        {
            bool result = false;

            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                {                   
                    excel = new ExcelPackage(fileStream);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
            }
            return result;
        }

        static public bool ParseExcelFile(ExcelPackage excel, ref Phonebook model)
        {
            bool result = false;

            try
            {
                var workSheet = excel.Workbook.Worksheets[RESOURCES_WORKSHEET];
                IEnumerable<ContactDto> newCollection = workSheet.ConvertSheetToObjects<ContactDto>();
                model.Contacts = newCollection.Where(x => IsIdNull(x.Id) == false)
                                              .Where(x => IsEmployeeIdNull(x.EmpolyeeId) == false)
                                              .Where(x => IsNullModel(x) == false)
                                              .ToList();
                result = true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
            }

            return result;
        }

        static private bool IsIdNull(string id)
        {
            bool result = true;

            if (id != null)
            {
                result = false;
            }

            return result;
        }

        static private bool IsEmployeeIdNull(string id)
        {
            bool result = true;

            if (id != null)
            {
                result = false;
            }

            return result;
        }

        static private bool IsNullModel(ContactDto model)
        {
            bool result = true;

            foreach (PropertyInfo prop in model.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(string))
                {
                    if (prop.GetValue(model, null).ToString() != null)
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
