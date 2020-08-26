using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ProjectEvalCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
		[OpenApiTag("Project Eval Criteria Management")]
    public class ProjectEvalCriteriaManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public ProjectEvalCriteriaManagementController()
        {
            _dbHelper = new DatabaseHelper();
        }
        [HttpPost]
        [Route("CreateProjectEvalCriteria")]
        public IActionResult CreateProjectEvalCriteria(ProjectEvalCriteria request)
        {
            try
            {
                if (request != null)
                {
                    DateTime _date = DateTime.UtcNow;
                    string created_user_id = Guid.NewGuid().ToString();
                    //verify required fields
                    var missingParameter = request.CheckRequiredParameters(new string[] { "action_attribute", "action_value", "condition_source", "condition_source_operator", "condition_source_value", "customer_id" });

                    if (missingParameter != null)
                    {
                        return BadRequest(new
                        {
                            status = missingParameter + " is required"
                        });
                    }
                    request.eval_criteria_id = string.IsNullOrEmpty(request.eval_criteria_id) ? Guid.NewGuid().ToString() : request.eval_criteria_id;
                    //check if eval_criteria_id already created or not
                    if (_IsExists(request.eval_criteria_id))
                    {
                        return BadRequest(new
                        {
                            status = $"A project eval criteria is already exist for this eval_criteria_id = {request.eval_criteria_id}"
                        });
                    }

                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        string query = @"INSERT INTO public.project_eval_criteria(action_attribute, action_value, condition_source, condition_source_operator, condition_source_value, customer_id, customer_office_id, eval_criteria_id, create_datetime, create_userid, edit_datetime, edit_userid) VALUES (@action_attribute, @action_value, @condition_source, @condition_source_operator, @condition_source_value, @customer_id, @customer_office_id, @eval_criteria_id, @create_datetime, @create_userid, @edit_datetime, @edit_userid); ";
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@action_attribute", request.action_attribute);
                        cmd.Parameters.AddWithValue("@action_value", request.action_value);
                        cmd.Parameters.AddWithValue("@condition_source", request.condition_source);
                        cmd.Parameters.AddWithValue("@condition_source_operator", request.condition_source_operator);
                        cmd.Parameters.AddWithValue("@condition_source_value", request.condition_source_value);
                        cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
                        cmd.Parameters.AddWithValue("@customer_office_id", request.customer_office_id ?? string.Empty);
                        cmd.Parameters.AddWithValue("@eval_criteria_id", request.eval_criteria_id);
                        cmd.Parameters.AddWithValue("@create_datetime", _date);
                        cmd.Parameters.AddWithValue("@create_userid", created_user_id);
                        cmd.Parameters.AddWithValue("@edit_datetime", _date);
                        cmd.Parameters.AddWithValue("@edit_userid", created_user_id);
                        cmd.ExecuteNonQuery();
                    }
                    return Ok(new
                    {
                        status = "Success",
                        request.eval_criteria_id
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
        [Route("UpdateProjectEvalCriteria")]
        public IActionResult UpdateProjectEvalCriteria(ProjectEvalCriteria request)
        {
            try
            {
                DateTime _date = DateTime.UtcNow;
                //verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[] { "eval_criteria_id" });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = missingParameter + " is required"
                    });
                }
                string created_user_id = Guid.NewGuid().ToString();
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    string command = @"UPDATE public.project_eval_criteria SET edit_datetime=@edit_datetime ";
                    cmd.Parameters.AddWithValue("@edit_datetime", _date);

                    if (!string.IsNullOrEmpty(request.action_attribute))
                    {
                        command += " ,action_attribute = @action_attribute";
                        cmd.Parameters.AddWithValue("@action_attribute", request.action_attribute);
                    }
                    if (!string.IsNullOrEmpty(request.action_value))
                    {
                        command += " ,action_value = @action_value";
                        cmd.Parameters.AddWithValue("@action_value", request.action_value);
                    }
                    if (!string.IsNullOrEmpty(request.condition_source))
                    {
                        command += " ,condition_source = @condition_source";
                        cmd.Parameters.AddWithValue("@condition_source", request.condition_source);
                    }
                    if (!string.IsNullOrEmpty(request.condition_source_operator))
                    {
                        command += " ,condition_source_operator = @condition_source_operator";
                        cmd.Parameters.AddWithValue("@condition_source_operator", request.condition_source_operator);
                    }
                    if (!string.IsNullOrEmpty(request.condition_source_value))
                    {
                        command += " ,condition_source_value = @condition_source_value";
                        cmd.Parameters.AddWithValue("@condition_source_value", request.condition_source_value);
                    }
                    if (!string.IsNullOrEmpty(request.customer_id))
                    {
                        command += " ,customer_id = @customer_id";
                        cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
                    }
                    if (!string.IsNullOrEmpty(request.customer_office_id))
                    {
                        command += " ,customer_office_id = @customer_office_id";
                        cmd.Parameters.AddWithValue("@customer_office_id", request.customer_office_id);
                    }
                    if (!string.IsNullOrEmpty(created_user_id))
                    {
                        command += " ,edit_userid = @edit_userid";
                        cmd.Parameters.AddWithValue("@edit_userid", created_user_id);
                    }
                    command += " WHERE eval_criteria_id= @eval_criteria_id;";
                    cmd.Parameters.AddWithValue("@eval_criteria_id", request.eval_criteria_id);
                    cmd.CommandText = command;
                    int affctedrowcount = cmd.ExecuteNonQuery();
                    if (affctedrowcount == 0)
                    {
                        return BadRequest(new
                        {
                            status = "No matching record found for eval_criteria_id =" + request.eval_criteria_id
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = "Successfully updated record."
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequest(error: ex.Message);
            }
            finally
            {
                _dbHelper.CloseConnection();
            }
        }
        [HttpGet]
        [Route("FindProjectEvalCriteria")]
        public IActionResult FindProjectEvalCriteria(string customer_id, string detail_level)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {

                if (string.IsNullOrEmpty(customer_id))
                {
                    return BadRequest(new
                    {
                        status = "customer_id is required."
                    });
                }
                string query = "SELECT action_attribute, action_value, condition_source, condition_source_operator, condition_source_value, customer_id, customer_office_id, eval_criteria_id, create_datetime, create_userid, edit_datetime, edit_userid FROM public.project_eval_criteria WHERE project_eval_criteria.customer_id = @customer_id";

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@customer_id", customer_id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "action_attribute",Convert.ToString(reader["action_attribute"]) },
                                { "action_value", Convert.ToString(reader["action_value"]) },
                                { "condition_source", Convert.ToString(reader["condition_source"]) },
                                { "condition_source_operator", Convert.ToString(reader["condition_source_operator"]) },
                                { "condition_source_value", Convert.ToString(reader["condition_source_value"]) },
                                { "customer_id", Convert.ToString(reader["customer_id"]) },
                                { "customer_office_id", Convert.ToString(reader["customer_office_id"]) },
                                { "eval_criteria_id", Convert.ToString(reader["eval_criteria_id"]) },
                                { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                { "edit_userid", Convert.ToString(reader["edit_userid"]) }
                            });
                        }
                    }
                }

                detail_level = string.IsNullOrEmpty(detail_level) ? detail_level : detail_level.ToLower();
                switch (detail_level)
                {
                    case "basic":
                        foreach (var item in result)
                        {
                            item.Remove("create_userid");
                            item.Remove("edit_userid");
                        };
                        break;
                    default:
                        break;
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
        [Route("GetProjectEvalCriteria")]
        public IActionResult GetProjectEvalCriteria(string eval_criteria_id)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                if (!string.IsNullOrEmpty(eval_criteria_id))
                {
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        cmd.CommandText = @"SELECT action_attribute, action_value, condition_source, condition_source_operator, condition_source_value, customer_id, customer_office_id, eval_criteria_id, create_datetime, create_userid, edit_datetime, edit_userid FROM public.project_eval_criteria WHERE project_eval_criteria.eval_criteria_id = @eval_criteria_id";
                        cmd.Parameters.AddWithValue("@eval_criteria_id", eval_criteria_id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new Dictionary<string, object>
                                {
                                    { "action_attribute",Convert.ToString(reader["action_attribute"]) },
                                    { "action_value", Convert.ToString(reader["action_value"]) },
                                    { "condition_source", Convert.ToString(reader["condition_source"]) },
                                    { "condition_source_operator", Convert.ToString(reader["condition_source_operator"]) },
                                    { "condition_source_value", Convert.ToString(reader["condition_source_value"]) },
                                    { "customer_id", Convert.ToString(reader["customer_id"]) },
                                    { "customer_office_id", Convert.ToString(reader["customer_office_id"]) },
                                    { "eval_criteria_id", Convert.ToString(reader["eval_criteria_id"]) },
                                    { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                    { "edit_datetime", Convert.ToString(reader["edit_datetime"]) }
                                });
                            }
                            return Ok(result);
                        }
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        status = "eval_criteria_id can't be null"
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
        private bool _IsExists(string eval_criteria_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT EXISTS (SELECT true FROM project_eval_criteria WHERE eval_criteria_id = '{eval_criteria_id }')";
                return (bool)cmd.ExecuteScalar();
            }
        }
    }
}
