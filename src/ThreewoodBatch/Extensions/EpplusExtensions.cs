using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThreewoodBatch.Models;

namespace ThreewoodBatch.Extensions
{
    public static class EPPLusExtensions
    {
        public static IEnumerable<T> ConvertSheetToObjects<T>(this ExcelWorksheet worksheet) where T : new()
        {

            Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(Column);

            var columns = typeof(T).GetProperties()
                                   .Where(x => x.CustomAttributes.Any(columnOnly))
                                   .Select(p => new {
                                       Property = p,
                                       //Column = p.GetCustomAttributes<Column>().First().ColumnIndex //safe because if where above
                                       Column = p.GetCustomAttributes<Column>().First().ColumnName
                                   })
                                   .ToList();


            var rows = worksheet.Cells
                                .Select(cell => cell.Start.Row)
                                .Distinct()
                                .OrderBy(x => x);


            //Create the collection container
            var collection = rows.Skip(1)
                                 .Select(row =>
                                 {
                                     var tnew = new T();
                                     columns.ForEach(col =>
                                     {
                                         //This is the real wrinkle to using reflection - Excel stores all numbers as double including int
                                         //var val = worksheet.Cells[row, col.Column];
                                         var val = worksheet.Cells[string.Format("{0}{1}", col.Column, row.ToString())];
                                         //If it is numeric it is a double since that is how excel stores all numbers
                                         if (val.Value == null)
                                         {
                                             col.Property.SetValue(tnew, null);
                                             return;
                                         }
                                         // Int32
                                         if (col.Property.PropertyType == typeof(Int32))
                                         {
                                             col.Property.SetValue(tnew, val.GetValue<int>());
                                             return;
                                         }
                                         //double
                                         if (col.Property.PropertyType == typeof(double))
                                         {
                                             col.Property.SetValue(tnew, val.GetValue<double>());
                                             return;
                                         }
                                         //DateTime
                                         if (col.Property.PropertyType == typeof(DateTime))
                                         {
                                             col.Property.SetValue(tnew, val.GetValue<DateTime>());
                                             return;
                                         }
                                         //string
                                         col.Property.SetValue(tnew, val.GetValue<string>());
                                     });

                                     return tnew;
                                 });

            return collection;
        }
    }
}
