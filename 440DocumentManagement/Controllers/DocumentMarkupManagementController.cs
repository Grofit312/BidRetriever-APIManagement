using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DocMarkupAnnotations;
using _440DocumentManagement.Models.DocumentMarkup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
		[OpenApiTag("Document Markup Management")]
    public class DocumentMarkupManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public DocumentMarkupManagementController()
        {
            _dbHelper = new DatabaseHelper();
        }

        [HttpPost]
        [Route("CreateDocumentMarkup")]
        public IActionResult CreateDocumentMarkup(DocumentMarkup request, bool IsSlipsheet = false)
        {
            try
            {
                string userId = Guid.NewGuid().ToString();
                //string datetime = Convert.ToString(new DateTimeOffset(DateTime.Now));
                DateTime dt = DateTime.Now;
                DateTimeOffset datetime = new DateTimeOffset(
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, dt.Second,
                    new TimeSpan(+5, 30, 0));
                request.markup_id = Guid.NewGuid().ToString();

                // Verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[]
                {
                    "author_userid", "doc_id", "file_id"
                });
                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    // Create new document_markups
                    string query = string.Empty;
                    if (IsSlipsheet)//Create document markups slipsheet
                    {
                        query = string.Format(@"INSERT INTO public.document_markups (markup_id, author_userid, author_display_name, create_datetime, edit_datetime, create_userid, author_companyname, doc_id, edit_userid, markup_name, markup_description, status, doc_file_id, parent_markup_id) VALUES(@markup_id, @author_userid, @author_displayname, @create_datetime, @edit_datetime, @create_userid,@author_companyname, @doc_id, @edit_userid, @markup_name, @markup_description, @status, @file_id, @parent_markup_id);");
                        cmd.Parameters.AddWithValue("@parent_markup_id", (object)request.parent_markup_id ?? DBNull.Value);
                    }
                    else//Create Normal document markups
                    {
                        query = string.Format(@"INSERT INTO public.document_markups (markup_id, author_userid, author_display_name, create_datetime, edit_datetime, create_userid, author_companyname, doc_id, edit_userid, markup_name, markup_description, status, doc_file_id) VALUES(@markup_id, @author_userid, @author_displayname, @create_datetime, @edit_datetime, @create_userid,@author_companyname, @doc_id, @edit_userid, @markup_name, @markup_description, @status, @file_id);");
                    }
                    cmd.Parameters.AddWithValue("@markup_id", request.markup_id);
                    cmd.Parameters.AddWithValue("@author_userid", request.author_userid);
                    cmd.Parameters.AddWithValue("@author_displayname", (object)request.author_displayname ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_datetime", request.create_datetime ?? datetime);
                    cmd.Parameters.AddWithValue("@edit_datetime", request.edit_datetime ?? datetime);
                    cmd.Parameters.AddWithValue("@create_userid", request.create_userid ?? userId);
                    cmd.Parameters.AddWithValue("@author_companyname", (object)request.author_companyname ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@doc_id", request.doc_id ?? Guid.NewGuid().ToString());
                    cmd.Parameters.AddWithValue("@edit_userid", request.edit_userid ?? userId);
                    cmd.Parameters.AddWithValue("@markup_name", (object)request.markup_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@markup_description", (object)request.markup_description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", (object)request.status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@file_id", request.file_id);

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    #region code
                    //cmd.CommandText = @"INSERT INTO public.document_markups (markup_id, author_userid, author_display_name, create_datetime, edit_datetime, create_userid, author_companyname, doc_id, edit_userid, markup_name, markup_description, status) VALUES('@markup_id', '@author_userid', '@author_display_name', '@create_datetime', '@edit_datetime', '@create_userid', '@author_companyname', '@doc_id', '@edit_userid', '@markup_name', '@markup_description', '@status'); ";

                    //cmd.Parameters.AddWithValue("markup_id", request.markup_id);
                    //cmd.Parameters.AddWithValue("author_userid", request.author_userid);
                    //cmd.Parameters.AddWithValue("author_display_name", request.author_displayname);
                    //cmd.Parameters.Add(new NpgsqlParameter("@create_datetime", NpgsqlDbType.Char)).Value = request.create_datetime ?? datetime;
                    //cmd.Parameters.Add(new NpgsqlParameter("@edit_datetime", NpgsqlDbType.Char)).Value = request.edit_datetime ?? datetime;
                    //cmd.Parameters.AddWithValue("create_datetime", request.create_datetime ?? datetime);
                    //cmd.Parameters.AddWithValue("edit_datetime", request.edit_datetime ?? datetime);
                    //cmd.Parameters.AddWithValue("@create_datetime", request.create_datetime ?? datetime);
                    //cmd.Parameters["create_datetime"].NpgsqlDbType = NpgsqlDbType.TimestampTz;
                    //cmd.Parameters.AddWithValue("@edit_datetime", request.edit_datetime ?? datetime);
                    //cmd.Parameters["edit_datetime"].NpgsqlDbType = NpgsqlDbType.TimestampTz;
                    //cmd.Parameters.Add(new NpgsqlParameter("create_datetime", NpgsqlDbType.Text)).Value = request.create_datetime ?? "";
                    //cmd.Parameters.Add(new NpgsqlParameter("edit_datetime", NpgsqlDbType.Text)).Value = request.edit_datetime ?? "";

                    //cmd.Parameters.AddWithValue("create_datetime", request.create_datetime ?? datetime);
                    //cmd.Parameters.AddWithValue("edit_datetime", request.edit_datetime ?? datetime);
                    //cmd.Parameters.AddWithValue("create_userid", request.create_userid ?? userId);
                    //cmd.Parameters.AddWithValue("author_companyname", request.author_companyname);
                    //cmd.Parameters.AddWithValue("doc_id", request.doc_id ?? Guid.NewGuid().ToString());
                    //cmd.Parameters.AddWithValue("edit_userid", request.edit_userid ?? userId);
                    //cmd.Parameters.AddWithValue("markup_name", request.markup_name);
                    //cmd.Parameters.AddWithValue("markup_description", request.markup_description);
                    //cmd.Parameters.AddWithValue("status", request.status);
                    //cmd.ExecuteNonQuery();
                    #endregion
                    return Ok(new
                    {
                        request.markup_id,
                        status = "Completed."
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
        [Route("DeleteDocumentMarkup")]
        public IActionResult DeleteDocumentMarkup(DocumentMarkupDeleteRequest request)
        {
            try
            {
                // Remove all annotations belonged to that markup_id
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = "DELETE FROM document_markup_annotations WHERE markup_id=@markup_id";
                    cmd.Parameters.AddWithValue("@markup_id", request.markup_id);
                    cmd.ExecuteNonQuery();
                }

                // Remove document markup
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = "DELETE FROM document_markups WHERE markup_id=@markup_id";
                    cmd.Parameters.AddWithValue("@markup_id", request.markup_id);
                    cmd.ExecuteNonQuery();
                }

                return Ok(new
                {
                    markup_id = request.markup_id,
                    status = "Completed"
                });
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
        [Route("UpdateDocumentMarkup")]
        public IActionResult UpdateDocumentMarkup(DocumentMarkup request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        status = "Request can't be null."
                    });
                }

                // Verify required fields
                if (request.markup_id == null)
                {
                    return BadRequest(new
                    {
                        status = "markup_id is missing."
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    string command = "UPDATE document_markups SET edit_datetime = @edit_datetime";
                    cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);

                    if (!string.IsNullOrEmpty(request.author_userid))
                    {
                        command += " ,author_userid=COALESCE(@author_userid, author_userid) ";
                        cmd.Parameters.AddWithValue("@author_userid", request.author_userid);
                    }
                    if (!string.IsNullOrEmpty(request.author_displayname))
                    {
                        command += " ,author_display_name=COALESCE(@author_display_name, author_display_name) ";
                        cmd.Parameters.AddWithValue("@author_display_name", request.author_displayname);
                    }
                    if (!string.IsNullOrEmpty(request.create_userid))
                    {
                        command += " ,create_userid=COALESCE(@create_userid, create_userid) ";
                        cmd.Parameters.AddWithValue("@create_userid", request.create_userid);
                    }
                    if (!string.IsNullOrEmpty(request.author_userid))
                    {
                        command += " ,author_userid=COALESCE(@author_userid, author_userid) ";
                        cmd.Parameters.AddWithValue("@author_userid", request.author_userid);
                    }
                    if (!string.IsNullOrEmpty(request.author_userid))
                    {
                        command += " ,author_userid=COALESCE(@author_userid, author_userid) ";
                        cmd.Parameters.AddWithValue("@author_userid", request.author_userid);
                    }
                    if (!string.IsNullOrEmpty(request.author_companyname))
                    {
                        command += " ,author_companyname=COALESCE(@author_companyname, author_companyname) ";
                        cmd.Parameters.AddWithValue("@author_companyname", request.author_companyname);
                    }
                    if (!string.IsNullOrEmpty(request.doc_id))
                    {
                        command += " ,doc_id=COALESCE(@doc_id, doc_id) ";
                        cmd.Parameters.AddWithValue("@doc_id", request.doc_id);
                    }
                    if (!string.IsNullOrEmpty(request.edit_userid))
                    {
                        command += " ,edit_userid=COALESCE(@edit_userid, edit_userid) ";
                        cmd.Parameters.AddWithValue("@edit_userid", request.edit_userid);
                    }
                    if (!string.IsNullOrEmpty(request.markup_name))
                    {
                        command += " ,markup_name=COALESCE(@markup_name, markup_name) ";
                        cmd.Parameters.AddWithValue("@markup_name", request.markup_name);
                    }
                    if (!string.IsNullOrEmpty(request.markup_description))
                    {
                        command += " ,markup_description=COALESCE(@markup_description, markup_description) ";
                        cmd.Parameters.AddWithValue("@markup_description", request.markup_description);
                    }
                    if (!string.IsNullOrEmpty(request.status))
                    {
                        command += " ,status=COALESCE(@status, status) ";
                        cmd.Parameters.AddWithValue("@status", request.status);
                    }
                    if (!string.IsNullOrEmpty(request.parent_markup_id))
                    {
                        command += " ,parent_markup_id=COALESCE(@parent_markup_id, parent_markup_id) ";
                        cmd.Parameters.AddWithValue("@parent_markup_id", request.parent_markup_id);
                    }
                    if (!string.IsNullOrEmpty(request.file_id))
                    {
                        command += " ,doc_file_id=COALESCE(@doc_file_id, doc_file_id) ";
                        cmd.Parameters.AddWithValue("@doc_file_id", request.file_id);
                    }

                    command += " WHERE markup_id=@markup_id";
                    cmd.Parameters.AddWithValue("@markup_id", request.markup_id);

                    cmd.CommandText = command;

                    int affectedRowCount = cmd.ExecuteNonQuery();
                    return Ok(new
                    {
                        status = $"{affectedRowCount} markups are updated."
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
        [Route("GetDocumentMarkup")]
        public IActionResult GetDocumentMarkupById(string markup_id)
        {
            try
            {
                if (string.IsNullOrEmpty(markup_id))
                {
                    return BadRequest(new
                    {
                        status = "markup_id can't be null."
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = string.Format("SELECT * FROM public.document_markups where markup_id = @markup_id");
                    cmd.Parameters.AddWithValue("@markup_id", markup_id);

                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "author_companyname", Convert.ToString(reader["author_companyname"]) },
                                { "author_display_name", Convert.ToString(reader["author_display_name"]) },
                                { "author_userid", Convert.ToString(reader["author_userid"]) },
                                { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "doc_id", Convert.ToString(reader["doc_id"]) },
                                { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                { "edit_userid", Convert.ToString(reader["edit_userid"]) },
                                { "markup_description", Convert.ToString(reader["markup_description"]) },
                                { "markup_id", Convert.ToString(reader["markup_id"]) },
                                { "markup_name", Convert.ToString(reader["markup_description"]) },
                                { "markeup_status", Convert.ToString(reader["markup_description"]) },
                                { "parent_markup_id", Convert.ToString(reader["parent_markup_id"]) },
                                { "file_id", Convert.ToString(reader["doc_file_id"]) }
                            });
                        }
                        return Ok(new
                        {
                            status = "Success",
                            statuscode = StatusCodes.Status200OK,
                            data = result
                        });
                    }
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
        [Route("FindDocumentMarkups")]
        public IActionResult FindDocumentMarkups(FindDocumentCriteria criteria)
        {
            try
            {
                string query = "SELECT * FROM public.document_markups";
                string where = string.Empty;
                if (criteria != null)
                {
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        //Search By doc_id
                        if (!string.IsNullOrEmpty(criteria.doc_id))
                        {
                            where = string.Format(" WHERE doc_id =@doc_id ");

                            cmd.Parameters.AddWithValue("@doc_id", criteria.doc_id);
                        }
                        //Search By user_id
                        if (!string.IsNullOrEmpty(criteria.user_id))
                        {
                            if (string.IsNullOrEmpty(where))
                            {
                                where = string.Format(" WHERE create_userid =@create_userid ");
                            }
                            else
                            {
                                where += string.Format(" AND create_userid =@create_userid ");
                            }
                            cmd.Parameters.AddWithValue("@create_userid", criteria.user_id);
                        }
                        query += where;

                        cmd.CommandText = query;

                        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new Dictionary<string, object>
                                {
                                    { "author_companyname", Convert.ToString(reader["author_companyname"]) },
                                    { "author_display_name", Convert.ToString(reader["author_display_name"]) },
                                    { "author_userid", Convert.ToString(reader["author_userid"]) },
                                    { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                    { "create_userid", Convert.ToString(reader["create_userid"]) },
                                    { "doc_id", Convert.ToString(reader["doc_id"]) },
                                    { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                                    { "edit_userid", Convert.ToString(reader["edit_userid"]) },
                                    { "markup_description", Convert.ToString(reader["markup_description"]) },
                                    { "markup_id", Convert.ToString(reader["markup_id"]) },
                                    { "markup_name", Convert.ToString(reader["markup_name"]) },
                                    { "markup_status", Convert.ToString(reader["status"]) },
                                    { "parent_markup_id", Convert.ToString(reader["parent_markup_id"]) }
                                });
                            }
                        }
                        return Ok(result);
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        status = "Please provide criteria."
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

        private bool _IsExists(string parent_id, string tablename)
        {
            if (!string.IsNullOrEmpty(parent_id) && !string.IsNullOrEmpty(tablename))
            {
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = $"SELECT EXISTS (SELECT true FROM public.{tablename} WHERE parent_markup_id='{parent_id }')";
                    return (bool)cmd.ExecuteScalar();
                }
            }
            else
            {
                return false;
            }
        }
    }
}