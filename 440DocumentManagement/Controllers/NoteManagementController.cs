using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [OpenApiTag("Note Management")]
    public class NoteManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;
        public NoteManagementController()
        {
            _dbHelper = new DatabaseHelper();
        }

        [HttpPost]
        [Route("CreateNote")]
        public IActionResult CreateNote(NoteFilter request)
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

                DateTime _date = DateTime.UtcNow;
                request.note_id = Guid.NewGuid().ToString();
                ///string created_user_id = Guid.NewGuid().ToString();
                //verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[]
                {
                    "note_company_id", "note_desc", "note_parent_id", "note_subject"
                });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }
                request.note_display_name = string.IsNullOrEmpty(request.note_display_name)
                    ? $"Note-{request.created_user_id}-{request.note_subject}" : request.note_display_name;

                request.note_id = string.IsNullOrEmpty(request.note_id) ? Guid.NewGuid().ToString() : request.note_id;
                request.note_status = string.IsNullOrEmpty(request.note_status) ? "active" : request.note_status;

                request.note_timeline_displayname = string.IsNullOrEmpty(request.note_timeline_displayname)
                    ? $"Note-{request.note_parent_type}-{request.created_user_id}-{request.note_subject}"
                    : request.note_timeline_displayname;

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    string query = "INSERT INTO notes(note_id, note_type, create_datetime, create_user_id, edit_datetime, edit_user_id, note_company_id, note_subject, note_desc, note_status, note_parent_id, note_vote_count, note_relevance_number, note_priority, note_parent_type, note_displayname, note_timeline_displayname) VALUES (@note_id, @note_type, @create_datetime, @created_user_id, @edit_datetime, @edit_user_id, @company_id, @subject, @description, @status, @parent_id, @vote_count, @relevance_number, @priority, @parent_type, @display_name, @timeline_displayname);";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@note_id", request.note_id);
                    cmd.Parameters.AddWithValue("@note_type", (object)request.note_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_datetime", _date);
                    cmd.Parameters.AddWithValue("@created_user_id", request.created_user_id);
                    cmd.Parameters.AddWithValue("@edit_datetime", _date);
                    cmd.Parameters.AddWithValue("@edit_user_id", request.created_user_id);
                    cmd.Parameters.AddWithValue("@company_id", request.note_company_id);
                    cmd.Parameters.AddWithValue("@subject", request.note_subject);
                    cmd.Parameters.AddWithValue("@description", request.note_desc);
                    cmd.Parameters.AddWithValue("@status", request.note_status);
                    cmd.Parameters.AddWithValue("@parent_id", request.note_parent_id);
                    cmd.Parameters.AddWithValue("@vote_count", request.note_vote_count);
                    cmd.Parameters.AddWithValue("@relevance_number", request.note_relevance_number);
                    cmd.Parameters.AddWithValue("@priority", (object)request.note_priority ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@parent_type", (object)request.note_parent_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@display_name", request.note_display_name);
                    cmd.Parameters.AddWithValue("@timeline_displayname", request.note_timeline_displayname);

                    int affectedRowCount = cmd.ExecuteNonQuery();
                    return Ok(new
                    {
                        status = "Note has been updated.",
                        request.note_id
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
        [Route("FindNotes")]
        public IActionResult FindNotes(NoteFilterSearch request)
        {
            try
            {
                var parentId = "";

                if (request.annotation_id != null)
                {
                    parentId = request.annotation_id;
                }

                if (request.company_id != null)
                {
                    parentId = request.company_id;
                }

                if (request.doc_id != null)
                {
                    parentId = request.doc_id;
                }

                if (request.event_id != null)
                {
                    parentId = request.event_id;
                }
                if (request.folder_id != null)
                {
                    parentId = request.folder_id;
                }
                if (request.markup_id != null)
                {
                    parentId = request.markup_id;
                }
                if (request.note_id != null)
                {
                    parentId = request.note_id;
                }
                if (request.note_type != null)
                {
                    parentId = request.note_type;
                }
                if (request.office_id != null)
                {
                    parentId = request.office_id;
                }
                if (request.project_id != null)
                {
                    parentId = request.project_id;
                }
                if (request.user_id != null)
                {
                    parentId = request.user_id;
                }

                var result = __getChildNotes(parentId, request.return_child_notes);

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

        //------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("UpdateNote")]
        public IActionResult UpdateNote(NoteFilter request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        status = "Request is null"
                    });
                }

                DateTime dt = DateTime.Now;

                // Verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[] { "note_id" });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    string command = "UPDATE notes SET edit_datetime = @edit_datetime ";
                    cmd.Parameters.AddWithValue("@edit_datetime", dt);
                    if (!string.IsNullOrEmpty(request.note_type))
                    {
                        command += " ,note_type =@note_type ";
                        cmd.Parameters.AddWithValue("@note_type", request.note_type);
                    }
                    if (!string.IsNullOrEmpty(request.note_company_id))
                    {
                        command += " ,note_company_id= @company_id";
                        cmd.Parameters.AddWithValue("@company_id", request.note_company_id);
                    }
                    if (!string.IsNullOrEmpty(request.note_subject))
                    {
                        command += " ,note_subject= @subject";
                        cmd.Parameters.AddWithValue("@subject", request.note_subject);
                    }
                    if (!string.IsNullOrEmpty(request.note_desc))
                    {
                        command += " ,note_desc= @note_desc";
                        cmd.Parameters.AddWithValue("@note_desc", request.note_desc);
                    }
                    if (!string.IsNullOrEmpty(request.note_status))
                    {
                        command += " ,note_status= @note_status";
                        cmd.Parameters.AddWithValue("@note_status", request.note_status);
                    }
                    if (!string.IsNullOrEmpty(request.note_parent_id))
                    {
                        command += " ,note_parent_id= @note_parent_id";
                        cmd.Parameters.AddWithValue("@note_parent_id", request.note_parent_id);
                    }
                    if (request.note_vote_count.HasValue)
                    {
                        command += " ,note_vote_count= @note_vote_count";
                        cmd.Parameters.AddWithValue("@note_vote_count", request.note_vote_count);
                    }
                    if (request.note_relevance_number.HasValue)
                    {
                        command += " ,note_relevance_number= @note_relevance_number";
                        cmd.Parameters.AddWithValue("@note_relevance_number", request.note_relevance_number);
                    }

                    if (!string.IsNullOrEmpty(request.note_priority))
                    {
                        command += " ,note_priority= @note_priority";
                        cmd.Parameters.AddWithValue("@note_priority", request.note_priority);
                    }
                    if (!string.IsNullOrEmpty(request.note_parent_type))
                    {
                        command += " ,note_parent_type= @note_parent_type";
                        cmd.Parameters.AddWithValue("@note_parent_type", request.note_parent_type);
                    }
                    if (!string.IsNullOrEmpty(request.note_display_name))
                    {
                        command += " ,note_display_name= @note_display_name";
                        cmd.Parameters.AddWithValue("@note_display_name", request.note_display_name);
                    }
                    if (!string.IsNullOrEmpty(request.note_timeline_displayname))
                    {
                        command += " ,note_timeline_displayname= @note_timeline_displayname";
                        cmd.Parameters.AddWithValue("@note_timeline_displayname", request.note_timeline_displayname);
                    }
                    command += " WHERE note_id=@note_id";
                    cmd.Parameters.AddWithValue("@note_id", request.note_id);
                    cmd.CommandText = command;
                    int affectedRowCount = cmd.ExecuteNonQuery();
                    return Ok(new
                    {
                        status = $"{affectedRowCount} notes are updated."
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
        [Route("RemoveNotes")]
        public IActionResult Delete(string note_id)
        {
            try
            {

                if (string.IsNullOrEmpty(note_id))
                    return BadRequest(new { status = "NoteId is required" });

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = $"DELETE from Notes where note_id =@note_id";
                    cmd.Parameters.AddWithValue("@note_id", note_id);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return Ok(new
                        {
                            status = "Notes has been removed"
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            status = "Notes doesn't exist or note_id is incorrect"
                        });
                    }
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

        private List<Dictionary<string, object>> __getChildNotes(string noteId, bool returnChildNotes)
        {
            var children = new List<Dictionary<string, object>> { };

            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT notes.create_datetime, notes.edit_datetime, notes.note_company_id, notes.note_desc, notes.note_displayname,notes.note_id, notes.note_parent_id, notes.note_parent_type,  notes.note_priority, notes.note_relevance_number, notes.note_status, notes.note_subject, notes.note_timeline_displayname, notes.note_type, notes.note_vote_count, "
                    + "users.user_firstname, users.user_lastname, customers.customer_name "
                    + "FROM notes "
                    + "LEFT JOIN users ON notes.create_user_id=users.user_id "
                    + "LEFT JOIN customers ON notes.note_company_id=customers.customer_id "
                    + $"WHERE note_parent_id='{noteId}'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var firstName = Convert.ToString(reader["user_firstname"]);
                        var lastName = Convert.ToString(reader["user_lastname"]);
                        var customerName = Convert.ToString(reader["customer_name"]);

                        children.Add(new Dictionary<string, object>
                        {
                            { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                            { "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
                            { "note_company_id", Convert.ToString(reader["note_company_id"]) },
                            { "note_desc", Convert.ToString(reader["note_desc"]) },
                            { "note_display_name", Convert.ToString(reader["note_displayname"]) },
                            { "note_id", Convert.ToString(reader["note_id"]) },
                            { "note_parent_id", Convert.ToString(reader["note_parent_id"]) },
                            { "note_parent_type", Convert.ToString(reader["note_parent_type"]) },
                            { "note_priority", Convert.ToString(reader["note_priority"]) },
                            { "note_relevance_number", Convert.ToString(reader["note_relevance_number"]) },
                            { "note_status", Convert.ToString(reader["note_status"]) },
                            { "note_subject", Convert.ToString(reader["note_subject"]) },
                            { "note_timeline_displayname", Convert.ToString(reader["note_timeline_displayname"]) },
                            { "note_type", Convert.ToString(reader["note_type"]) },
                            { "note_vote_count", Convert.ToString(reader["note_vote_count"]) },
                            { "author_user_displayname", $"{lastName}, {firstName}" },
                            { "author_company_displayname", customerName },
                        });
                    }
                }
            }

            if (returnChildNotes)
            {
                foreach (var child in children)
                {
                    child["children"] = __getChildNotes((string)child["note_id"], returnChildNotes);
                }
            }

            return children;
        }
    }
}
