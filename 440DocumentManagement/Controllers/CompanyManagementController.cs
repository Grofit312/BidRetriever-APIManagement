using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.CompanyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
  [Produces("application/json")]
  [Route("api")]
  public class CompanyManagementController : Controller
  {
    private readonly DatabaseHelper _dbHelper;
    public CompanyManagementController()
    {
      _dbHelper = new DatabaseHelper();
    }

    [HttpPost]
    [Route("CreateCompany")]
    public IActionResult CreateCompany(Company request)
    {
      try
      {
        if (request != null)
        {
          DateTime _date = DateTime.UtcNow;
          request.company_id = (request.company_id == null || string.IsNullOrEmpty(request.company_id)) ? Guid.NewGuid().ToString() : request.company_id;
          request.customer_id = (request.customer_id == null || string.IsNullOrEmpty(request.customer_id)) ? Guid.NewGuid().ToString() : request.customer_id;
          request.company_status = (request.company_status == null || string.IsNullOrEmpty(request.company_status)) ? "active" : request.company_status;

          request.company_crm_id = (request.company_crm_id == null || string.IsNullOrEmpty(request.company_crm_id)) ? Guid.NewGuid().ToString() : request.company_crm_id;
          //request.company_admin_user_id = (request.company_admin_user_id == null || string.IsNullOrEmpty(request.company_admin_user_id)) ? Guid.NewGuid().ToString() : request.company_admin_user_id;
          request.company_duns_number = (request.company_duns_number == null || string.IsNullOrEmpty(request.company_duns_number)) ? "active" : request.company_duns_number;

          string created_user_id = Guid.NewGuid().ToString();
          //verify required fields
          var missingParameter = request.CheckRequiredParameters(new string[] { "company_name", "company_domain" });

          if (missingParameter != null)
          {
            return BadRequest(new
            {
              status = missingParameter + " is required"
            });
          }
          //check if company_id and company_domain exists
          if (_IsExists(request.company_id, request.company_domain))
          {
            return BadRequest(new
            {
              status = $"Company already exist with a company_id or company_domain."
            });
          }

          using (var cmd = _dbHelper.SpawnCommand())
          {
            string query = @"INSERT INTO public.customer_companies (company_address1, company_address2, company_admin_user_id, company_city, company_country, company_crm_id, company_domain, company_duns_number, company_email, company_id, company_name, company_phone, company_photo_id, company_service_area, company_state, company_timezone, company_type, company_website, company_zip, create_datetime, create_user_id, edit_datetime, edit_user_id, record_source, status, customer_id, company_logo_id, company_revenue, company_employee_number)	VALUES (@company_address1,  @company_address2, @company_admin_user_id, @company_city, @company_country, @company_crm_id, @company_domain, @company_duns_number, @company_email, @company_id, @company_name, @company_phone, @company_photo_id, @company_service_area, @company_state, @company_timezone, @company_type, @company_website, @company_zip, @create_datetime, @create_user_id, @edit_datetime, @edit_user_id, @record_source, @status, @customer_id, @company_logo_id, @company_revenue, @company_employee_number);";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@company_address1", (object)request.company_address1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_address2", (object)request.company_address2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_admin_user_id", (object)request.company_admin_user_id ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("@company_billing_id", request.company_billing_id);
            cmd.Parameters.AddWithValue("@company_city", (object)request.company_city ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_country", (object)request.company_country ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_crm_id", request.company_crm_id);
            cmd.Parameters.AddWithValue("@company_domain", (object)request.company_domain ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_duns_number", request.company_duns_number);
            cmd.Parameters.AddWithValue("@company_email", (object)request.company_email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_id", request.company_id);
            //cmd.Parameters.AddWithValue("@company_logo_id", request.company_logo_id);
            cmd.Parameters.AddWithValue("@company_name", request.company_name);
            cmd.Parameters.AddWithValue("@company_phone", (object)request.company_phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_photo_id", (object)request.company_photo_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_service_area", (object)request.company_service_area ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_state", (object)request.company_state ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@company_subscription_level", request.company_subscription_level);
            //cmd.Parameters.AddWithValue("@company_subscription_level_id", request.company_subscription_level_id);
            cmd.Parameters.AddWithValue("@company_timezone", (object)request.company_timezone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_type", (object)request.company_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_website", (object)request.company_website ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_zip", (object)request.company_zip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@create_datetime", _date);
            cmd.Parameters.AddWithValue("@create_user_id", created_user_id);
            cmd.Parameters.AddWithValue("@edit_datetime", _date);
            cmd.Parameters.AddWithValue("@edit_user_id", created_user_id);
            //cmd.Parameters.AddWithValue("@full_address", request.company_full_address);
            cmd.Parameters.AddWithValue("@record_source", (object)request.company_record_source ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", request.company_status);

            cmd.Parameters.AddWithValue("@customer_id", (object)request.customer_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_logo_id", (object)request.company_logo_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_revenue", (object)request.company_revenue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company_employee_number", (object)request.company_employee_number ?? DBNull.Value);
            cmd.ExecuteNonQuery();
          }
          if (!string.IsNullOrEmpty(request.user_id))
          {
            using (var cmd = _dbHelper.SpawnCommand())
            {
              cmd.CommandText = "INSERT INTO public.user_companies VALUES (@create_datetime, @customer_company_id, @user_id,  @user_company_id);";
              cmd.Parameters.AddWithValue("@create_datetime", _date);
              cmd.Parameters.AddWithValue("@user_id", request.user_id);
              cmd.Parameters.AddWithValue("@customer_company_id", request.company_id);
              cmd.Parameters.AddWithValue("@user_company_id", request.company_id);
              cmd.ExecuteNonQuery();
            }
          }
          if (!string.IsNullOrEmpty(request.customer_office_id))
          {
            using (var cmd = _dbHelper.SpawnCommand())
            {
              cmd.CommandText = "INSERT INTO public.customer_office_companies VALUES (@create_datetime,  @customer_office_id, @customer_company_id, @office_company_id);";
              cmd.Parameters.AddWithValue("@create_datetime", _date);
              cmd.Parameters.AddWithValue("@customer_office_id", request.customer_office_id);
              cmd.Parameters.AddWithValue("@customer_company_id", request.company_id);
              cmd.Parameters.AddWithValue("@office_company_id", request.company_id);
              cmd.ExecuteNonQuery();
            }
          }
          return Ok(new
          {
            status = "Success",
            request.company_id
          });
        }
        else
        {
          return BadRequest(new
          {
            status = "Request can't contains null"
          });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          status = ex.Message
        });
      }
      finally
      {
        _dbHelper.CloseConnection();
      }
    }

    [HttpPost]
    [Route("UpdateCompany")]
    public IActionResult UpdateCompany(Company request)
    {
      try
      {
        //verify required fields

        var missingParameter = request.CheckRequiredParameters(new string[] { "company_id" });
        //var missingParameter = request.CheckRequiredParameters(new string[] { "company_name", "company_id" });

        if (missingParameter != null)
        {
          return BadRequest(new
          {
            status = missingParameter + " is required"
          });
        }
        DateTime _date = DateTime.UtcNow;
        request.company_status = string.IsNullOrEmpty(request.company_status) ? "active" : request.company_status;
        request.customer_id = string.IsNullOrEmpty(request.customer_id) ? Guid.NewGuid().ToString() : request.customer_id;

        using (var cmd = _dbHelper.SpawnCommand())
        {
          string command = "UPDATE public.customer_companies SET edit_datetime=@edit_datetime ";
          cmd.Parameters.AddWithValue("@edit_datetime", _date);

          //cmd.CommandText = @"UPDATE public.customer_companies SET company_address1=@company_address1, company_address2=@company_address2, company_admin_user_id=@company_admin_user_id, company_billing_id=@company_billing_id, company_city=@company_city,company_country=@company_country, company_crm_id=@company_crm_id, company_domain=@company_domain, company_duns_number=@company_duns_number, company_email=@company_email, company_logo_id=@company_logo_id, company_name=@company_name, company_phone=@company_phone, company_photo_id=@company_photo_id, company_service_area=@company_service_area, company_state=@company_state, company_subscription_level=@company_subscription_level, company_subscription_level_id=@company_subscription_level_id, company_timezone=@company_timezone, company_type=@company_type, company_website=@company_website, company_zip=@company_zip, create_user_id=@create_user_id, edit_datetime=@edit_datetime, edit_user_id=@edit_user_id, full_address=@full_address, record_source=@record_source, status=@status,  customer_id=@customer_id, company_logo_id=@company_logo_id, company_revenue=@company_revenue, company_employee_number=@company_employee_number WHERE company_id=@company_id; ";

          if (!string.IsNullOrEmpty(request.company_address1))
          {
            command += " ,company_address1 = @company_address1";
            cmd.Parameters.AddWithValue("@company_address1", (object)request.company_address1 ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_address2))
          {
            command += " ,company_address2 = @company_address2";
            cmd.Parameters.AddWithValue("@company_address2", (object)request.company_address2 ?? DBNull.Value);
          }

          if (!string.IsNullOrEmpty(request.company_admin_user_id))
          {
            command += " ,company_admin_user_id = @company_admin_user_id";
            cmd.Parameters.AddWithValue("@company_admin_user_id", (object)request.company_admin_user_id ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_city))
          {
            command += " ,company_city = @company_city";
            cmd.Parameters.AddWithValue("@company_city", (object)request.company_city ?? DBNull.Value);
          }

          if (!string.IsNullOrEmpty(request.company_country))
          {
            command += " ,company_country = @company_country";
            cmd.Parameters.AddWithValue("@company_country", (object)request.company_country ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_crm_id))
          {
            command += " ,company_crm_id = @company_crm_id";
            cmd.Parameters.AddWithValue("@company_crm_id", request.company_crm_id);
          }

          //------
          if (!string.IsNullOrEmpty(request.company_domain))
          {
            command += " ,company_domain = @company_domain";
            cmd.Parameters.AddWithValue("@company_domain", (object)request.company_domain ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_duns_number))
          {
            command += " ,company_duns_number = @company_duns_number";
            cmd.Parameters.AddWithValue("@company_duns_number", request.company_duns_number);
          }
          //company_email is not required filed 
          command += " ,company_email = @company_email";
          cmd.Parameters.AddWithValue("@company_email", (object)request.company_email ?? DBNull.Value);

          if (!string.IsNullOrEmpty(request.company_name))
          {
            command += " ,company_name = @company_name";
            cmd.Parameters.AddWithValue("@company_name", request.company_name);
          }

          if (!string.IsNullOrEmpty(request.company_phone))
          {
            command += " ,company_phone = @company_phone";
            cmd.Parameters.AddWithValue("@company_phone", (object)request.company_phone ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_photo_id))
          {
            command += " ,company_photo_id = @company_photo_id";
            cmd.Parameters.AddWithValue("@company_photo_id", (object)request.company_photo_id ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_service_area))
          {
            command += " ,company_service_area = @company_service_area";
            cmd.Parameters.AddWithValue("@company_service_area", (object)request.company_service_area ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_state))
          {
            command += " ,company_state = @company_state";
            cmd.Parameters.AddWithValue("@company_state", (object)request.company_state ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_timezone))
          {
            command += " ,company_timezone = @company_timezone";
            cmd.Parameters.AddWithValue("@company_timezone", (object)request.company_timezone ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_type))
          {
            command += " ,company_type = @company_type";
            cmd.Parameters.AddWithValue("@company_type", (object)request.company_type ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_website))
          {
            command += " ,company_website = @company_website";
            cmd.Parameters.AddWithValue("@company_website", (object)request.company_website ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_zip))
          {
            command += " ,company_zip = @company_zip";
            cmd.Parameters.AddWithValue("@company_zip", (object)request.company_zip ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_record_source))
          {
            command += " ,record_source = @record_source";
            cmd.Parameters.AddWithValue("@record_source", (object)request.company_record_source ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.customer_id))
          {
            command += " ,customer_id = @customer_id";
            cmd.Parameters.AddWithValue("@customer_id", (object)request.customer_id ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_logo_id))
          {
            command += " ,company_logo_id = @company_logo_id";
            cmd.Parameters.AddWithValue("@company_logo_id", (object)request.company_logo_id ?? DBNull.Value);
          }
          if (!string.IsNullOrEmpty(request.company_revenue))
          {
            command += " ,company_revenue = @company_revenue";
            cmd.Parameters.AddWithValue("@company_revenue", (object)request.company_revenue ?? DBNull.Value);
          }
          if (request.company_employee_number.HasValue)
          {
            command += " ,company_employee_number = @company_employee_number";
            cmd.Parameters.AddWithValue("@company_employee_number", (object)request.company_employee_number ?? DBNull.Value);
          }
          command += " ,status = @status";
          command += " WHERE company_id = @company_id";
          cmd.Parameters.AddWithValue("@status", request.company_status);
          cmd.Parameters.AddWithValue("@company_id", request.company_id);
          cmd.CommandText = command;
          var affectedRows = cmd.ExecuteNonQuery();

          if (affectedRows == 0)
          {
            return BadRequest(new
            {
              status = "Failed to find company to update"
            });
          }

          return Ok(new
          {
            status = "updated"
          });
        }
      }
      catch (Exception exception)
      {
        return BadRequest(new
        {
          status = exception.Message
        });
      }
      finally
      {
        _dbHelper.CloseConnection();
      }
    }

    [HttpGet]
    [Route("FindCompanies")]
    public IActionResult FindCompanies(FindCompaniesCreteria request)
    {
      List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
      try
      {
        string query = "SELECT customer_companies.*, users.user_displayname FROM customer_companies LEFT JOIN users ON customer_companies.company_admin_user_id = users.user_id";
        string where = string.Empty;
        using (var cmd = _dbHelper.SpawnCommand())
        {
          if (!string.IsNullOrEmpty(request.company_name))
          {
            where += " where customer_companies.company_name=@company_name";
            cmd.Parameters.AddWithValue("@company_name", request.company_name);
          }
          if (!string.IsNullOrEmpty(request.company_domain))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_domain=@company_domain";
            }
            else
            {
              where += " and customer_companies.company_domain=@company_domain";
            }
            cmd.Parameters.AddWithValue("@company_domain", request.company_domain);
          }
          if (!string.IsNullOrEmpty(request.company_type))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_type=@company_type";
            }
            else
            {
              where += " and customer_companies.company_type=@company_type";
            }
            cmd.Parameters.AddWithValue("@company_type", request.company_type);
          }
          if (!string.IsNullOrEmpty(request.company_service_area))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_service_area=@company_service_area";
            }
            else
            {
              where += " and customer_companies.company_service_area=@company_service_area";
            }
            cmd.Parameters.AddWithValue("@company_service_area", request.company_service_area);
          }
          if (!string.IsNullOrEmpty(request.company_record_source))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.record_source=@record_source";
            }
            else
            {
              where += " and customer_companies.record_source=@record_source";
            }
            cmd.Parameters.AddWithValue("@record_source", request.company_record_source);
          }
          if (!string.IsNullOrEmpty(request.company_state))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_state=@company_state";
            }
            else
            {
              where += " and customer_companies.company_state=@company_state";
            }
            cmd.Parameters.AddWithValue("@company_state", request.company_state);
          }
          if (!string.IsNullOrEmpty(request.company_zip))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_zip=@company_zip";
            }
            else
            {
              where += " and customer_companies.company_zip=@company_zip";
            }
            cmd.Parameters.AddWithValue("@company_zip", request.company_zip);
          }
          if (!string.IsNullOrEmpty(request.company_status))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.company_status=@company_status";
            }
            else
            {
              where += " and customer_companies.company_status=@company_status";
            }
            cmd.Parameters.AddWithValue("@company_status", request.company_status);
          }

          if (!string.IsNullOrEmpty(request.customer_id))
          {
            if (string.IsNullOrEmpty(where))
            {
              where += " where customer_companies.customer_id =@customer_id ";
            }
            else
            {
              where += " and customer_companies.customer_id =@customer_id ";
            }
            cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
          }

          cmd.CommandText = query + where + " Order By company_name";
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              result.Add(new Dictionary<string, object>
                                                        {
                                                                { "company_email", Convert.ToString(reader["company_email"]) },
                                                                { "company_id", Convert.ToString(reader["company_id"]) },
                                                                { "company_admin_displayname", Convert.ToString(reader["user_displayname"]) },
                                                                { "company_admin_user_id", Convert.ToString(reader["company_admin_user_id"]) },
                                                                { "company_domain", Convert.ToString(reader["company_domain"]) },
                                                                { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                                                { "company_address1", Convert.ToString(reader["company_address1"]) },
                                                                { "company_address2", Convert.ToString(reader["company_address2"]) },
                                                                { "company_country", Convert.ToString(reader["company_country"]) },
                                                                { "company_duns_number", Convert.ToString(reader["company_duns_number"]) },
                                                                { "company_service_area", Convert.ToString(reader["company_service_area"]) },
                                                                { "company_photo_id", Convert.ToString(reader["company_photo_id"]) },
                                                                { "company_record_source", Convert.ToString(reader["record_source"]) },
                                                                { "company_timezone", Convert.ToString(reader["company_timezone"]) },
                                                                { "company_website", Convert.ToString(reader["company_website"]) },
                                                                { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                                                { "company_status", Convert.ToString(reader["status"]) },
                                                                { "company_crm_id", Convert.ToString(reader["company_crm_id"]) },
                                                                { "create_user_id", Convert.ToString(reader["create_user_id"]) },
                                                                { "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
                                                                { "company_type", Convert.ToString(reader["company_type"]) },
                                                                { "company_city", Convert.ToString(reader["company_city"]) },
                                                                { "company_name", Convert.ToString(reader["company_name"]) },
                                                                { "company_phone", Convert.ToString(reader["company_phone"]) },
                                                                { "company_state", Convert.ToString(reader["company_state"]) },
                                                                { "company_zip", Convert.ToString(reader["company_zip"]) },
                                                                { "status", Convert.ToString(reader["status"]) },

                                                                { "customer_id", Convert.ToString(reader["customer_id"]) },
                                                                { "company_employee_number", Convert.ToString(reader["company_employee_number"]) },
                                                                { "company_revenue", Convert.ToString(reader["company_revenue"]) },
                                                                { "company_logo_id", Convert.ToString(reader["company_logo_id"]) },

                                                        });
            }
          }
        }

        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          status = ex.Message
        });
      }
      finally
      {
        _dbHelper.CloseConnection();
      }
    }

    [HttpGet]
    [Route("GetCompany")]
    public IActionResult GetCompany(string company_id)
    {
      List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
      try
      {
        string query = $"SELECT * FROM customer_companies WHERE company_id= '{ company_id }' ";
        string where = string.Empty;
        using (var cmd = _dbHelper.SpawnCommand())
        {
          cmd.CommandText = query + where;
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              result.Add(new Dictionary<string, object>
                                                        {
                                                                { "company_email", Convert.ToString(reader["company_email"]) },
                                                                { "company_id", Convert.ToString(reader["company_id"]) },
                                                                { "company_domain", Convert.ToString(reader["company_domain"]) },
                                                                { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                                                { "company_address1", Convert.ToString(reader["company_address1"]) },
                                                                { "company_address2", Convert.ToString(reader["company_address2"]) },
                                                                { "company_country", Convert.ToString(reader["company_country"]) },
                                                                { "company_duns_number", Convert.ToString(reader["company_duns_number"]) },
                                                                { "company_service_area", Convert.ToString(reader["company_service_area"]) },
                                                                { "company_photo_id", Convert.ToString(reader["company_photo_id"]) },
                                                                { "company_record_source", Convert.ToString(reader["record_source"]) },
                                                                { "company_timezone", Convert.ToString(reader["company_timezone"]) },
                                                                { "company_website", Convert.ToString(reader["company_website"]) },
                                                                { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                                                { "company_status", Convert.ToString(reader["status"]) },
                                                                { "company_crm_id", Convert.ToString(reader["company_crm_id"]) },
                                                                { "create_user_id", Convert.ToString(reader["create_user_id"]) },
                                                                { "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
                                                                { "company_type", Convert.ToString(reader["company_type"]) },
                                                                { "company_city", Convert.ToString(reader["company_city"]) },
                                                                { "company_name", Convert.ToString(reader["company_name"]) },
                                                                { "company_phone", Convert.ToString(reader["company_phone"]) },
                                                                { "company_state", Convert.ToString(reader["company_state"]) },
                                                                { "company_zip", Convert.ToString(reader["company_zip"]) },
                                                                { "status", Convert.ToString(reader["status"]) },

                                                                { "customer_id", Convert.ToString(reader["customer_id"]) },
                                                                { "company_employee_number", Convert.ToString(reader["company_employee_number"]) },
                                                                { "company_revenue", Convert.ToString(reader["company_revenue"]) },
                                                                { "company_logo_id", Convert.ToString(reader["company_logo_id"]) },
                                                        });
            }
          }
        }
        if (result.Count == 0)
        {
          return BadRequest("No Record found for company_id=" + company_id);
        }
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          status = ex.Message
        });
      }
      finally
      {
        _dbHelper.CloseConnection();
      }
    }

    private bool _IsExists(string company_id, string company_domain)
    {
      using (var cmd = _dbHelper.SpawnCommand())
      {
        cmd.CommandText = $"SELECT EXISTS (SELECT true FROM public.customer_companies WHERE company_id=@company_id or company_domain= @company_domain)";
        cmd.Parameters.AddWithValue("@company_id", company_id);
        cmd.Parameters.AddWithValue("@company_domain", (object)company_domain ?? DBNull.Value);
        return (bool)cmd.ExecuteScalar();
      }
    }

    }    
  }

