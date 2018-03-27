using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreewoodBatch.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Our.Umbraco.Vorto.Models;
using Newtonsoft.Json;

namespace ThreewoodBatch.Helper
{
    public class PhonebookHelper
    {
        const string EMPLOYEEID_PROP_TYPE_ALIAS = "employeeID";
        const string DEPARTMENT_PROP_TYPE_ALIAS = "department";
        const string CONTACT_NAME_PROP_TYPE_ALIAS = "contactName";
        const string POSITION_NAME_PROP_TYPE_ALIAS = "position";
        const string WORK_LOCATION_PROP_TYPE_ALIAS = "workLocation";
        const string FLOOR_PROP_TYPE_ALIAS = "floor";
        const string TRADE_PROP_TYPE_ALIAS = "trade";
        const string TELEPHONE_PROP_TYPE_ALIAS = "telephoneExt";
        const string FAX_PROP_TYPE_ALIAS = "fax";
        const string EMAIL_PROP_TYPE_ALIAS = "email";

        const string VORTO_DATA_TYPE_NAME_ALIAS = "Vorto Textstring";

        private ApplicationContext _applicationContext;
        private IPublishedContent _homePage;
        private Guid _vortoTextstringGuid;

        public PhonebookHelper(ApplicationContext applicationContext,
                               IPublishedContent homePage)
        {
            _applicationContext = applicationContext;
            _homePage = homePage;
            var dts = _applicationContext.Services.DataTypeService;
            _vortoTextstringGuid = dts.GetAllDataTypeDefinitions().Where(x => x.Name == VORTO_DATA_TYPE_NAME_ALIAS).FirstOrDefault().Key;
        }
     
        public Response ImportPhonebook(string file, string departmentNameAlias, string phonebookNameAlias)
        {
            Response result = new Response();
            result.message = "Failure unknow reason, please check with system administrator";
            
            Phonebook model = new Phonebook();
            ExcelPackage excel = null;

            if (!ExcelHelper.ReadExcelFile(file, ref excel))
            {
                throw new PhonebookValidationException("File format is not supported", System.Net.HttpStatusCode.InternalServerError);
            }

            if (!ExcelHelper.ParseExcelFile(excel, ref model))
            {
                throw new PhonebookValidationException("File format is invalid", System.Net.HttpStatusCode.InternalServerError);
            }

            if (IsDuplicate(model))
            {
                throw new PhonebookValidationException("Duplicated Data found", System.Net.HttpStatusCode.InternalServerError);
            }

            result = WriteData2Backoffice(model, departmentNameAlias, phonebookNameAlias);                

            return result;
        }
        
        private Response WriteData2Backoffice(Phonebook model,
                                              string departmentNameAlias,
                                              string phonebookNameAlias,  
                                              string departmentDocTypeAlias = "department",
                                              string phonebookDocTypeAlias = "phonebook",
                                              string contactItemDocTypeAlias = "contactItem")
        {
            Response result = new Response();

            IPublishedContent phonebook = _homePage.Descendants()                                                  
                                                   .Where(x => x.DocumentTypeAlias == phonebookDocTypeAlias)
                                                   .Where(x => x.Name == phonebookNameAlias)
                                                   .Where(x => x.Parent.Name == departmentNameAlias)
                                                   .FirstOrDefault();

            if (phonebook != null)
            {
                var cs = _applicationContext.Services.ContentService;
                
                foreach(ContactDto contact in model.Contacts.Where(x => x.Action == "Add"))
                {
                    if(AddRecord(phonebook, contact, contactItemDocTypeAlias))
                    {
                        result.SuccessAdded.Add(contact.Id);
                        LogHelper.Info<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Added successfull", contact.Id, contact.EmpolyeeId));
                    }
                    else
                    {
                        result.FailAdded.Add(contact.Id);
                        LogHelper.Error<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Added Failure", contact.Id, contact.EmpolyeeId), null);
                    }
                }

                foreach(ContactDto contact in model.Contacts.Where(x => x.Action == "Update"))
                {
                    if (UpdateRecord(phonebook, contact, contactItemDocTypeAlias))
                    {
                        result.SuccessUpdated.Add(contact.Id);
                        LogHelper.Info<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Edit successfull", contact.Id, contact.EmpolyeeId));
                    }
                    else
                    {
                        result.FailUpdated.Add(contact.Id);
                        LogHelper.Error<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Edit Failure", contact.Id, contact.EmpolyeeId), null);
                    }
                    
                }

                foreach(ContactDto contact in model.Contacts.Where(x => x.Action == "Delete"))
                {
                    if(DeleteRecord(phonebook, contact, contactItemDocTypeAlias))
                    {
                        result.SuccessDeleted.Add(contact.Id);
                        LogHelper.Info<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Delete successfull", contact.Id, contact.EmpolyeeId));
                    }
                    else
                    {
                        result.FailDeleted.Add(contact.Id);
                        LogHelper.Error<PhonebookHelper>(string.Format("contact ID({0}) EmpID({1}) Delete Failure", contact.Id, contact.EmpolyeeId), null);
                    }
                }                
            }
            else
            {
                string msg = string.Format("IPublishContent phonebook is null, departmentNameAlias({0}), phonebookNameAlias({1})", departmentNameAlias, phonebookNameAlias);
                LogHelper.Error<PhonebookHelper>(msg, null);
                throw new PhonebookValidationException(msg, System.Net.HttpStatusCode.InternalServerError);
            }

            return result;
        }
        
        private bool AddRecord(IPublishedContent parent,
                               ContactDto model,
                               string contactItemDocTypeAlias)
        {
            bool result = false;

            try
            {
                IPublishedContent contactContent = parent.Children.Where(x => x.Name == model.Id)
                                                  .Where(x => x.GetPropertyValue<string>(EMPLOYEEID_PROP_TYPE_ALIAS) == model.EmpolyeeId)
                                                  .FirstOrDefault();

                if (contactContent == null)
                {
                    var cs = _applicationContext.Services.ContentService;

                    var newContent = cs.CreateContent(name: model.Id, parentId: parent.Id, contentTypeAlias: contactItemDocTypeAlias, userId: 0);
                    newContent.SetValue(EMPLOYEEID_PROP_TYPE_ALIAS, model.EmpolyeeId);
                    newContent.SetValue(DEPARTMENT_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Department_EN, model.Department_CHT)));
                    newContent.SetValue(CONTACT_NAME_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Name_EN, model.Name_CHT)));
                    newContent.SetValue(POSITION_NAME_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Position_EN, model.Position_CHT)));
                    newContent.SetValue(WORK_LOCATION_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.WorkLocation_EN, model.WorkLocation_CHT)));
                    newContent.SetValue(FLOOR_PROP_TYPE_ALIAS, model.Floor);
                    newContent.SetValue(TRADE_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Trade_EN, model.Trade_CHT)));
                    newContent.SetValue(TELEPHONE_PROP_TYPE_ALIAS, model.Telephone);
                    newContent.SetValue(FAX_PROP_TYPE_ALIAS, model.Fax);
                    newContent.SetValue(EMAIL_PROP_TYPE_ALIAS, model.Email);
                    cs.SaveAndPublishWithStatus(newContent);
                    result = true;
                }
                else
                {
                    LogHelper.Warn<PhonebookHelper>(string.Format("contact id({0}) existed", model.Id));
                    result = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<PhonebookHelper>(string.Format("contact id({0}) exception message : {1}", model.Id, ex.Message), ex);
                result = false;
            }

            return result;
        }

        private bool UpdateRecord(IPublishedContent parent,
                                  ContactDto model,
                                  string contactItemDocTypeAlias)
        {
            bool result = false;


            try
            {
                IPublishedContent contactContent = parent.Children.Where(x => x.Name == model.Id)
                                                                  .Where(x => x.GetPropertyValue<string>(EMPLOYEEID_PROP_TYPE_ALIAS) == model.EmpolyeeId)
                                                                  .FirstOrDefault();

                if (contactContent != null)
                {

                    var cs = _applicationContext.Services.ContentService;
                    var contact = cs.GetById(Convert.ToInt32(contactContent.Id));

                    contact.SetValue(DEPARTMENT_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Department_EN, model.Department_CHT)));
                    contact.SetValue(CONTACT_NAME_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Name_EN, model.Name_CHT)));
                    contact.SetValue(POSITION_NAME_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Position_EN, model.Position_CHT)));
                    contact.SetValue(WORK_LOCATION_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.WorkLocation_EN, model.WorkLocation_CHT)));
                    contact.SetValue(FLOOR_PROP_TYPE_ALIAS, model.Floor);
                    contact.SetValue(TRADE_PROP_TYPE_ALIAS, JsonConvert.SerializeObject(GetVortoProperty(model.Trade_EN, model.Trade_CHT)));
                    contact.SetValue(TELEPHONE_PROP_TYPE_ALIAS, model.Telephone);
                    contact.SetValue(FAX_PROP_TYPE_ALIAS, model.Fax);
                    contact.SetValue(EMAIL_PROP_TYPE_ALIAS, model.Email);
                    cs.SaveAndPublishWithStatus(contact);

                    result = true;
                }                
            }
            catch (Exception ex)
            {
                LogHelper.Error<PhonebookHelper>(string.Format("contact id({0}) exception message : {1}", model.Id, ex.Message), ex);
                result = false;
            }

            return result;
        }

        private bool DeleteRecord(IPublishedContent parent,
                                  ContactDto model,
                                  string contactItemDocTypeAlias)
        {
            bool result = false;

            try
            {
                IPublishedContent contactContent = parent.Children.Where(x => x.Name == model.Id)
                                                                  .Where(x => x.GetPropertyValue<string>(EMPLOYEEID_PROP_TYPE_ALIAS) == model.EmpolyeeId)
                                                                  .FirstOrDefault();

                if (contactContent != null)
                {
                    var cs = _applicationContext.Services.ContentService;
                    var contact = cs.GetById(Convert.ToInt32(contactContent.Id));
                    cs.MoveToRecycleBin(contact);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<PhonebookHelper>(string.Format("contact id({0}) exception message : {1}", model.Id, ex.Message), ex);
                result = false;
            }

            return result;
        }

        private bool IsDuplicate(Phonebook model)
        {
            bool result = true;
            try
            {
                // Check duplicate of ID
                var duplicatedIDs = model.Contacts.GroupBy(x => x.Id)
                                          .Where(g => g.Count() > 1)
                                          .Where(g => g.Key != null)
                                          .ToDictionary(x => x.Key, y => y.Count());

                if (duplicatedIDs.Count > 0)
                {
                    LogHelper.Error<PhonebookHelper>(string.Format("Duplicated IDs = {0}", CommonHelper.SerializeObject<List<string>>(duplicatedIDs.Keys.ToList())), null);
                    return true;
                }                

                // Check duplicate of EmployeeID
                var duplicatedEmployeeIDs = model.Contacts.GroupBy(x => x.EmpolyeeId)
                                                          .Where(g => g.Count() > 1)
                                                          .Where(g => g.Key != null)
                                                          .ToDictionary(x => x.Key, y => y.Count());
                if (duplicatedEmployeeIDs.Count > 0)
                {
                    LogHelper.Error<PhonebookHelper>(string.Format("Duplicated Employee IDs = {0}", CommonHelper.SerializeObject<List<string>>(duplicatedEmployeeIDs.Keys.ToList())), null);
                    return true;
                }

                result = false;

            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
            }

            return result;
        }

        private VortoValue GetVortoProperty(string englishValue, string chineseValue)
        {
            VortoValue property = new VortoValue();

            property.DtdGuid = _vortoTextstringGuid;
            property.Values = new Dictionary<string, object>();
            property.Values.Add("en-US", (object)englishValue);
            property.Values.Add("zh-HK", (object)chineseValue);

            return property;
        }
    }
}
