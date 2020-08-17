using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace _440DocumentManagement.Controllers
{
  [Produces("application/json")]
  [Route("api")]
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
          return BadRequest(new
          {
            status = "Please pass only one argument."
          });
        }
        string query = "SELECT notes.create_datetime, notes.edit_datetime, notes.note_company_id, notes.note_desc, notes.note_displayname,notes.note_id, notes.note_parent_id, notes.note_parent_type,  notes.note_priority, notes.note_relevance_number, notes.note_status, notes.note_subject, notes.note_timeline_displayname, notes.note_type, notes.note_vote_count FROM notes ";
        string where = "";
        using (var cmd = _dbHelper.SpawnCommand())
        {
          if (request.annotation_id != null)
          {
            where = " where notes.note_parent_id = @annotation_id";
            cmd.Parameters.AddWithValue("@annotation_id", request.annotation_id);
          }

          if (request.company_id != null)
          {
            where = " where notes.note_parent_id = @company_id";
            cmd.Parameters.AddWithValue("@company_id", request.company_id);
          }

          if (request.doc_id != null)
          {
            where = " where notes.note_parent_id = @doc_id";
            cmd.Parameters.AddWithValue("@doc_id", request.doc_id);
          }

          if (request.event_id != null)
          {
            where = " where notes.note_parent_id = @event_id";
            cmd.Parameters.AddWithValue("@event_id", request.event_id);
          }
          if (request.folder_id != null)
          {
            where = " where notes.note_parent_id = @folder_id";
            cmd.Parameters.AddWithValue("@folder_id", request.folder_id);
          }
          if (request.markup_id != null)
          {
            where = " where notes.note_parent_id = @markup_id";
            cmd.Parameters.AddWithValue("@markup_id", request.markup_id);
          }
          if (request.note_id != null)
          {
            where = " where notes.note_parent_id = @note_id";
            cmd.Parameters.AddWithValue("@note_id", request.note_id);
          }
          if (request.note_type != null)
          {
            where = " where notes.note_parent_id = @note_type";
            cmd.Parameters.AddWithValue("@note_type", request.note_type);
          }
          if (request.office_id != null)
          {
            where = " where notes.note_parent_id = @office_id";
            cmd.Parameters.AddWithValue("@office_id", request.office_id);
          }
          if (request.project_id != null)
          {
            where = " where notes.note_parent_id = @project_id";
            cmd.Parameters.AddWithValue("@project_id", request.project_id);
          }
          if (request.user_id != null)
          {
            where = " where notes.note_parent_id = @user_id";
            cmd.Parameters.AddWithValue("@user_id", request.user_id);
          }

          cmd.CommandText = query + where;
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              result.Add(new Dictionary<string, object>
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
                                { "note_vote_count", Convert.ToString(reader["note_vote_count"]) }
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

    //------------------------------------------------------------------------------------------------
    [HttpGet]
    [Route("FindCompanyNotes/{companyId}")]
    public IActionResult FindCompanyNotes(string companyId)
    {
      List<CustomModel> result = new List<CustomModel>();
      try
      {
        if (string.IsNullOrEmpty(companyId))
        {
          return BadRequest(new
          {
            status = "Please pass companyId."
          });
        }

        string query = "SELECT t1.create_datetime,t1.create_user_id,t1.note_parent_type,t1.create_datetime,t1.note_company_id,t1.edit_datetime, t1.note_company_id, t1.note_desc,t1.note_displayname,t1.note_id, t1.note_parent_id,t1.note_parent_type,  t1.note_priority, t1.note_relevance_number,t1.note_status, t1.note_subject, t1.note_timeline_displayname, t1.note_type, t1.note_type,t1.note_vote_count,t2.user_firstname,t2.user_lastname,t2.user_role FROM notes as t1 inner join users as t2 on t2.user_id = t1.create_user_id ";
        string where = "";
        using (var cmd = _dbHelper.SpawnCommand())
        {
          if (!string.IsNullOrEmpty(companyId))
          {
            where = " where t1.note_company_id = @company_id";
            cmd.Parameters.AddWithValue("@company_id", companyId);
          }

          cmd.CommandText = query + where;
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              result.Add(new CustomModel
              {
                Id = Convert.ToString(reader["note_id"]),
                Name = Convert.ToString(reader["note_displayname"]),
                Description = Convert.ToString(reader["note_desc"]),
                ParentId = Convert.ToString(reader["note_parent_id"]) == Convert.ToString(reader["note_company_id"]) ? null : Convert.ToString(reader["note_parent_id"]),
                Subject = Convert.ToString(reader["note_subject"]),
                UserId = Convert.ToString(reader["create_user_id"]),
                NoteType = Convert.ToString(reader["note_type"]),
                CompanyId = Convert.ToString(reader["note_company_id"]),
                NoteParentType = Convert.ToString(reader["note_parent_type"]),
                CreatedDate = Convert.ToString(reader["create_datetime"]),
                FirstName = Convert.ToString(reader["user_firstname"]),
                LastName = Convert.ToString(reader["user_lastname"]),
                UserRole = Convert.ToString(reader["user_role"]),

              });
            }
          }
        }

        var nodes = new List<CustomModel>();

        foreach (var item in result.Where(x => x.ParentId == null).ToList())
        {
          item.Children.AddRange(GetChildrens(result, item.Id));
          nodes.Add(item);
        }

        return Ok(nodes);
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

    private List<CustomModel> GetChildrens(List<CustomModel> customModels, string parentId)
    {
      var nodes = new List<CustomModel>();

      foreach (var item in customModels.Where(x => x.ParentId == parentId).ToList())
      {
        item.Children.AddRange(GetChildrens(customModels, item.Id));
        nodes.Add(item);
      }

      return nodes;
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
  }
}
