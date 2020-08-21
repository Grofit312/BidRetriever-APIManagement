using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.ContactManagement
{ 
 public class Contact : BaseModel
{
  public string company_id { get; set; }
  public string company_name { get; set; }
  public string company_office_id { get; set; }
  public string company_office_name { get; set; }
  public string contact_address1 { get; set; }
  public string contact_address2 { get; set; }
  public string contact_city { get; set; }
  public string contact_company_name { get; set; }
  public string contact_country { get; set; }
  public string contact_zip { get; set; }
  public string contact_crm_id { get; set; }
  public string contact_email { get; set; }
  public string contact_firstname { get; set; }
  public string contact_id { get; set; }
  public string contact_display_name { get; set; }
  public string contact_lastname { get; set; }
  public string contact_mobile_phone { get; set; }
  public string contact_password { get; set; }
  public string contact_phone { get; set; }
  public string contact_photo_id { get; set; }
  public string contact_record_source { get; set; }
  public string contact_role { get; set; }
  public string contact_state { get; set; }
  public string contact_status { get; set; }
  public string contact_title { get; set; }
  public string contact_username { get; set; }
  public string contact_verification_datetime { get; set; }
  public string contact_verification_level { get; set; }
  public string customer_id { get; set; }
  public string customer_office_id { get; set; }
  public string user_id { get; set; }
}
}
