using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class PermissionManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;
        public PermissionManagementController()
        {
            _dbHelper = new DatabaseHelper();
        }

        [HttpPost]
        [Route("CreatePermission")]
        public IActionResult CreatePermission(Permission request)
        {
            try
            {
                if (request != null)
                {
                    DateTime _date = DateTime.UtcNow;
                    request.permission_id = string.IsNullOrEmpty(request.permission_id) ? Guid.NewGuid().ToString() : request.permission_id;
                    string created_user_id = Guid.NewGuid().ToString();
                    //verify required fields
                    var missingParameter = request.CheckRequiredParameters(new string[] { "object_displayname", "object_id", "object_type", "permission_level", "permission_status" });

                    if (missingParameter != null)
                    {
                        return BadRequest(new
                        {
                            status = missingParameter + " is required"
                        });
                    }
                    if (string.IsNullOrEmpty(request.company_id) && string.IsNullOrEmpty(request.user_id))
                    {
                        return BadRequest(new
                        {
                            status = "Please provide either company_id or user_id."
                        });
                    }

                    //check if permission already created or not
                    if (_IsExists(request.user_id, request.object_id))
                    {
                        return BadRequest(new
                        {
                            statuscode = StatusCodes.Status403Forbidden,
                            message = $"A permission is already exist for this user_id = {request.user_id} and object_id={request.object_id}"
                        });
                    }

                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        string query = "INSERT INTO public.permissions(company_id, object_id, object_type, permission_id, permission_level, permission_status, user_id, create_datetime, create_userid, edit_datetime, edit_userid, object_displayname, user_displayname) VALUES(@company_id, @object_id, @object_type, @permission_id, @permission_level, @permission_status, @user_id, @create_datetime, @create_userid, @edit_datetime, @edit_userid, @object_displayname, @user_displayname); ";
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@company_id", (object)request.company_id ?? "");
                        cmd.Parameters.AddWithValue("@object_id", request.object_id);
                        cmd.Parameters.AddWithValue("@create_datetime", _date);
                        cmd.Parameters.AddWithValue("@create_userid", created_user_id);
                        cmd.Parameters.AddWithValue("@edit_datetime", _date);
                        cmd.Parameters.AddWithValue("@edit_userid", created_user_id);
                        cmd.Parameters.AddWithValue("@object_type", request.object_type);
                        cmd.Parameters.AddWithValue("@permission_id", request.permission_id);
                        cmd.Parameters.AddWithValue("@permission_level", request.permission_level);
                        cmd.Parameters.AddWithValue("@permission_status", request.permission_status);
                        cmd.Parameters.AddWithValue("@user_id", (object)request.user_id ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@object_displayname", request.object_displayname);
                        cmd.Parameters.AddWithValue("@user_displayname", (object)request.user_displayname ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    return Ok(new
                    {
                        status = "Success",
                        request.permission_id
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
        [Route("UpdatePermission")]
        public IActionResult UpdatePermission(PermissionsFilter request)
        {
            try
            {
                if (request != null)
                {
                    DateTime _date = DateTime.UtcNow;

                    //verify required fields
                    var missingParameter = request.CheckRequiredParameters(new string[] { "permission_id" });

                    if (missingParameter != null)
                    {
                        return BadRequest(new
                        {
                            status = missingParameter + " is required"
                        });
                    }

                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        string command = @"UPDATE public.permissions SET edit_datetime=@edit_datetime ";
                        cmd.Parameters.AddWithValue("@edit_datetime", _date);
                        if (!string.IsNullOrEmpty(request.object_displayname))
                        {
                            command += " ,object_displayname = @object_displayname";
                            cmd.Parameters.AddWithValue("@object_displayname", request.object_displayname);
                        }
                        if (!string.IsNullOrEmpty(request.permission_level))
                        {
                            command += " ,permission_level = @permission_level";
                            cmd.Parameters.AddWithValue("@permission_level", request.permission_level);
                        }
                        if (!string.IsNullOrEmpty(request.permission_status))
                        {
                            command += " ,permission_status = @permission_status";
                            cmd.Parameters.AddWithValue("@permission_status", request.permission_status);
                        }
                        if (!string.IsNullOrEmpty(request.user_displayname))
                        {
                            command += " ,user_displayname = @user_displayname";
                            cmd.Parameters.AddWithValue("@user_displayname", request.user_displayname);
                        }
                        command += " WHERE permission_id= @permission_id ";
                        cmd.Parameters.AddWithValue("@permission_id", request.permission_id);
                        cmd.CommandText = command;
                        int affctedrowcount = cmd.ExecuteNonQuery();
                        if (affctedrowcount == 0)
                        {
                            return BadRequest(new
                            {
                                status = "No matching record found for permission_id =" + request.permission_id
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
                else
                {
                    return BadRequest(new
                    {
                        status = "Request Can't contains null"
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

        [HttpGet]
        [Route("FindPermissions")]
        public IActionResult FindPermissions(PermissionFilterSearch request)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {

                int i = 0;
                PropertyInfo[] properties = request.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(request) != null)
                    {
                        i++;
                    }
                }

                if (i > 1)
                {
                    return BadRequest(new { statuscode = StatusCodes.Status400BadRequest, message = "Please pass only one argument!" });
                }
                string query = "SELECT company_id, object_id, object_type, permission_id, permission_level, permission_status, user_id, create_datetime, create_userid, edit_datetime, edit_userid, object_displayname, user_displayname, company_displayname FROM public.permissions ";
                string where = string.Empty;

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    if (request.object_id != null)
                    {
                        where = " where object_id = @object_id";
                        cmd.Parameters.AddWithValue("@object_id", request.object_id);
                    }

                    if (request.company_id != null)
                    {
                        where = " where company_id = @company_id";
                        cmd.Parameters.AddWithValue("@company_id", request.company_id);
                    }

                    if (request.object_type != null)
                    {
                        where = " where object_type = @object_type";
                        cmd.Parameters.AddWithValue("@object_type", request.object_type);
                    }

                    if (request.user_id != null)
                    {
                        where = " where user_id = @user_id";
                        cmd.Parameters.AddWithValue("@user_id", request.user_id);
                    }

                    cmd.CommandText = query + where;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "create_datetime",Convert.ToString(reader["create_datetime"]) },
                                { "company_displayname", Convert.ToString(reader["company_displayname"]) },
                                { "company_id", Convert.ToString(reader["company_id"]) },
                                { "object_displayname", Convert.ToString(reader["object_displayname"]) },
                                { "object_id", Convert.ToString(reader["object_id"]) },
                                { "object_type", Convert.ToString(reader["object_type"]) },
                                { "permission_id", Convert.ToString(reader["permission_id"]) },
                                { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                { "permission_level", Convert.ToString(reader["permission_level"]) },
                                { "permission_status", Convert.ToString(reader["permission_status"]) },
                                { "user_displayname", Convert.ToString(reader["user_displayname"]) },
                                { "user_id", Convert.ToString(reader["user_id"]) },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "edit_userid", Convert.ToString(reader["edit_userid"]) }
                            });
                        }
                    }
                }

                request.detail_level = string.IsNullOrEmpty(request.detail_level) ? request.detail_level : request.detail_level.ToLower();
                if (request.detail_level != null && request.detail_level != "admin")
                {
                    foreach (var item in result)
                    {
                        item.Remove("create_userid");
                        item.Remove("edit_userid");
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
        private bool _IsExists(string user_id, string object_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT EXISTS (SELECT true FROM public.permissions WHERE user_id='{user_id }' and object_id='{object_id}')";
                return (bool)cmd.ExecuteScalar();
            }
        }
    }
}
