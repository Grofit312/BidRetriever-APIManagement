using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DocMarkupAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
		[OpenApiTag("Document Annotation Management")]
    public class DocumentAnnotationManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public DocumentAnnotationManagementController()
        {
            _dbHelper = new DatabaseHelper();
        }

        [HttpPost]
        [Route("CreateDocumentMarkupAnnotation")]
        public IActionResult CreateDocumentAnnotation(DocumentAnnotation request)
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

                string userId = Guid.NewGuid().ToString();
                DateTime dt = DateTime.Now;
                DateTimeOffset datetime = new DateTimeOffset(
                    dt.Year, dt.Month, dt.Day,
                    dt.Hour, dt.Minute, dt.Second,
                    new TimeSpan(+5, 30, 0));
                request.annotation_id = request.annotation_id ?? Guid.NewGuid().ToString();
                request.parent_annotation_id = (request.parent_annotation_id == null
                    || string.IsNullOrEmpty(request.parent_annotation_id))
                    ? Guid.NewGuid().ToString() : request.parent_annotation_id;

                //verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[]
                {
                    "annotation_type", "annotation_current_data", "annotation_status"
                });
                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                //Check if exists to avoid duplicate entry
                if (_IsExists(request.parent_annotation_id, "document_markup_annotations"))
                {
                    return BadRequest(new
                    {
                        status = "document_markup_annotation for this parent_annotation_id is already existed."
                    });
                }
                else
                {
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        // Create new document_markup_annotations
                        string query = string.Format(@"INSERT INTO public.document_markup_annotations (annotation_id, annotation_type, create_datetime, create_userid, edit_datetime, edit_userid, markup_id, annotation_current_data, annotation_status) VALUES(@annotation_id, @annotation_type, @create_datetime, @create_userid, @edit_datetime, @edit_userid, @markup_id, @annotation_current_data, @annotation_status)");
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@annotation_id", request.annotation_id);
                        cmd.Parameters.AddWithValue("@annotation_type", request.annotation_type);
                        cmd.Parameters.AddWithValue("@create_datetime", request.create_datetime ?? datetime);
                        cmd.Parameters.AddWithValue("@create_userid", request.create_userid ?? userId);
                        cmd.Parameters.AddWithValue("@edit_datetime", request.edit_datetime ?? datetime);
                        cmd.Parameters.AddWithValue("@edit_userid", request.edit_userid ?? userId);
                        cmd.Parameters.AddWithValue("@markup_id", (object)request.markup_id ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@annotation_current_data", request.annotation_current_data);
                        cmd.Parameters.AddWithValue("@annotation_status", request.annotation_status);

                        cmd.ExecuteNonQuery();

                        // Log
                        __WriteApplicationLog(new DocMarkupAnnotationTransaction
                        {
                            annotation_id = request.annotation_id,
                            create_datetime = request.create_datetime ?? dt,
                            create_userid = request.create_userid ?? userId,
                            annotation_transaction_data = request.annotation_current_data,
                            annotation_transaction_status = request.annotation_status,
                            annotation_transaction_type = "create_annotation"
                        }); ;

                        return Ok(new
                        {
                            request.annotation_id,
                            status = "Completed."
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

        [HttpPost]
        [Route("UpdateDocumentMarkupAnnotation")]
        public IActionResult UpdateDocumentMarkupAnnotation(DocumentAnnotation request)
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

                //verify required fields
                if (request.annotation_id == null)
                {
                    return BadRequest(new
                    {
                        status = "annotation_id is missing."
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    string command = "UPDATE document_markup_annotations SET edit_datetime = @edit_datetime";
                    cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);

                    if (!string.IsNullOrEmpty(request.annotation_type))
                    {
                        command += " ,annotation_type=COALESCE(@annotation_type, annotation_type) ";
                        cmd.Parameters.AddWithValue("@annotation_type", request.annotation_type);
                    }
                    if (!string.IsNullOrEmpty(request.create_userid))
                    {
                        command += " ,create_userid=COALESCE(@create_userid, create_userid) ";
                        cmd.Parameters.AddWithValue("@create_userid", request.create_userid);
                    }
                    if (!string.IsNullOrEmpty(request.edit_userid))
                    {
                        command += " ,edit_userid=COALESCE(@edit_userid, edit_userid) ";
                        cmd.Parameters.AddWithValue("@edit_userid", request.edit_userid);
                    }
                    if (!string.IsNullOrEmpty(request.markup_id))
                    {
                        command += " ,markup_id=COALESCE(@markup_id, markup_id) ";
                        cmd.Parameters.AddWithValue("@markup_id", request.markup_id);
                    }
                    if (!string.IsNullOrEmpty(request.annotation_current_data))
                    {
                        command += " ,annotation_current_data=COALESCE(@annotation_current_data, annotation_current_data) ";
                        cmd.Parameters.AddWithValue("@annotation_current_data", request.annotation_current_data);
                    }
                    if (!string.IsNullOrEmpty(request.annotation_status))
                    {
                        command += " ,annotation_status=COALESCE(@annotation_status, annotation_status) ";
                        cmd.Parameters.AddWithValue("@annotation_status", request.annotation_status);
                    }
                    command += " WHERE annotation_id=@annotation_id";
                    cmd.Parameters.AddWithValue("@annotation_id", request.annotation_id);
                    cmd.CommandText = command;
                    int affectedRowCount = cmd.ExecuteNonQuery();
                    if (affectedRowCount == 0)
                    {
                        return BadRequest(new
                        {
                            status = "No matching record found",
                            request.annotation_id
                        });
                    }
                    else
                    {
                        // Log
                        __WriteApplicationLog(new DocMarkupAnnotationTransaction
                        {
                            annotation_id = request.annotation_id,
                            create_datetime = DateTime.UtcNow,
                            create_userid = request.create_userid ?? null,
                            annotation_transaction_data = request.annotation_current_data,
                            annotation_transaction_status = request.annotation_status,
                            annotation_transaction_type = "edit_annotation"
                        });

                        return Ok(new
                        {
                            status = "Completed.",
                            request.annotation_type,
                            request.annotation_current_data,
                            request.annotation_status
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
        [Route("GetDocumentMarkupAnnotation")]
        public IActionResult GetDocumentMarkupAnnotationById(string annotation_id)
        {
            try
            {
                if (string.IsNullOrEmpty(annotation_id))
                {
                    return BadRequest(new
                    {
                        status = "annotation_id can't be null."
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = "SELECT * FROM public.document_markup_annotations where annotation_id = @annotation_id";
                    cmd.Parameters.AddWithValue("@annotation_id", annotation_id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "annotation_id", Convert.ToString(reader["annotation_id"]) },
                                { "annotation_type", Convert.ToString(reader["annotation_type"]) },
                                { "create_datetime", reader["create_datetime"] },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "edit_datetime", reader["edit_datetime"] },
                                { "edit_userid", Convert.ToString(reader["edit_userid"]) },
                                { "parent_annotation_id", Convert.ToString(reader["parent_annotation_id"]) },
                                { "markup_id", Convert.ToString(reader["markup_id"]) },
                                { "annotation_current_data", Convert.ToString(reader["annotation_current_data"]) },
                                { "annotation_status", Convert.ToString(reader["annotation_status"]) },
                            });
                        }
                        return Ok(result);
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
        [Route("FindDocumentMarkupAnnotation")]
        public IActionResult FindDocumentMarkupAnnotation(string markup_id)
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
                    cmd.CommandText = "SELECT * FROM public.document_markup_annotations where markup_id = @markup_id";
                    cmd.Parameters.AddWithValue("@markup_id", markup_id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "annotation_id", Convert.ToString(reader["annotation_id"]) },
                                { "annotation_type", Convert.ToString(reader["annotation_type"]) },
                                { "create_datetime", reader["create_datetime"] },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "edit_datetime", reader["edit_datetime"] },
                                { "edit_userid", Convert.ToString(reader["edit_userid"]) },
                                { "parent_annotation_id", Convert.ToString(reader["parent_annotation_id"]) },
                                { "markup_id", Convert.ToString(reader["markup_id"]) },
                                { "annotation_current_data", Convert.ToString(reader["annotation_current_data"]) },
                                { "annotation_status", Convert.ToString(reader["annotation_status"]) },
                            });
                        }
                        return Ok(result);
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
        [Route("GetDocumentMarkupAnnotationTransaction")]
        public IActionResult GetDocumentMarkupAnnotationTransaction(string annotation_transaction_id)
        {
            try
            {
                if (string.IsNullOrEmpty(annotation_transaction_id))
                {
                    return BadRequest(new
                    {
                        status = "annotation_transaction_id can't be null."
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = "SELECT document_markup_annotation_transactions.annotation_id,document_markup_annotation_transactions.annotation_transaction_data, document_markup_annotation_transactions.annotation_transaction_id, document_markup_annotation_transactions.annotation_transaction_status,	document_markup_annotation_transactions.annotation_transaction_type, document_markup_annotations.annotation_type,	document_markup_annotation_transactions.create_datetime,document_markup_annotation_transactions.create_userid,document_markups.markup_id FROM	document_markups LEFT JOIN document_markup_annotations ON document_markups.markup_id = document_markup_annotations.markup_id LEFT JOIN	document_markup_annotation_transactions	ON document_markup_annotations.annotation_id = document_markup_annotation_transactions.annotation_id WHERE document_markup_annotation_transactions.annotation_transaction_id = @annotation_transaction_id";
                    cmd.Parameters.AddWithValue("@annotation_transaction_id", annotation_transaction_id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "annotation_id", Convert.ToString(reader["annotation_id"]) },
                                { "annotation_transaction_data", Convert.ToString(reader["annotation_transaction_data"]) },
                                { "annotation_transaction_id", Convert.ToString(reader["annotation_transaction_id"]) },
                                { "annotation_transaction_status", Convert.ToString(reader["annotation_transaction_status"]) },
                                { "annotation_transaction_type", Convert.ToString(reader["annotation_transaction_type"]) },
                                { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                { "create_userid", Convert.ToString(reader["create_userid"]) },
                                { "markup_id", Convert.ToString(reader["markup_id"]) },
                            });
                        }
                        return Ok(result);
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
        [Route("FindDocumentMarkupAnnotationTransactions")]
        public IActionResult FindDocumentMarkupAnnotationTransactions(string markup_id)
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
                if (!string.IsNullOrEmpty(markup_id))
                {
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        cmd.CommandText = "SELECT document_markups.markup_id, document_markup_annotation_transactions.annotation_id, document_markup_annotations.annotation_type, document_markup_annotation_transactions.create_userid, document_markup_annotation_transactions.create_datetime, document_markup_annotation_transactions.annotation_transaction_id, document_markup_annotation_transactions.annotation_transaction_type, document_markup_annotation_transactions.annotation_transaction_data, document_markup_annotation_transactions.annotation_transaction_status FROM document_markups LEFT JOIN document_markup_annotations ON document_markups.markup_id = document_markup_annotations.markup_id LEFT JOIN document_markup_annotation_transactions ON document_markup_annotations.annotation_id = document_markup_annotation_transactions.annotation_id WHERE document_markups.markup_id = @markup_id";
                        cmd.Parameters.AddWithValue("@markup_id", markup_id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                            while (reader.Read())
                            {
                                result.Add(new Dictionary<string, object>
                                {
                                    { "annotation_id", Convert.ToString(reader["annotation_id"]) },
                                    { "annotation_transaction_data", Convert.ToString(reader["annotation_transaction_data"]) },
                                    { "annotation_transaction_id", Convert.ToString(reader["annotation_transaction_id"]) },
                                    { "annotation_transaction_status", Convert.ToString(reader["annotation_transaction_status"]) },
                                    { "annotation_transaction_type", Convert.ToString(reader["annotation_transaction_type"]) },
                                    { "create_datetime", Convert.ToString(reader["create_datetime"]) },
                                    { "create_userid", Convert.ToString(reader["create_userid"]) },
                                    { "markup_id", Convert.ToString(reader["markup_id"]) },
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
                        status = "markup_id can't be null."
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
                    cmd.CommandText = $"SELECT EXISTS (SELECT true FROM public.{tablename} WHERE parent_annotation_id ='{parent_id }')";
                    return (bool)cmd.ExecuteScalar();
                }
            }
            else
            {
                return false;
            }
        }

        private void __WriteApplicationLog(DocMarkupAnnotationTransaction log)
        {
            try
            {
                log.annotation_transaction_id = (log.annotation_transaction_id == null || string.IsNullOrEmpty(log.annotation_transaction_id)) ? Guid.NewGuid().ToString() : log.annotation_transaction_id;

                // write log record
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    DateTime dateTime = DateTime.UtcNow;
                    string command = string.Format(@"INSERT INTO public.document_markup_annotation_transactions (annotation_transaction_id, annotation_id, create_datetime, create_userid, annotation_transaction_type, annotation_transaction_data, annotation_transaction_status) VALUES(@annotation_transaction_id, @annotation_id, @create_datetime, @create_userid, @annotation_transaction_type, @annotation_transaction_data, @annotation_transaction_status);");
                    cmd.CommandText = command;
                    cmd.Parameters.AddWithValue("@annotation_transaction_id", log.annotation_transaction_id);
                    cmd.Parameters.AddWithValue("@annotation_id", log.annotation_id);
                    cmd.Parameters.AddWithValue("@create_datetime", log.create_datetime);
                    cmd.Parameters.AddWithValue("@create_userid", log.create_userid);
                    cmd.Parameters.AddWithValue("@annotation_transaction_type", log.annotation_transaction_type);
                    cmd.Parameters.AddWithValue("@annotation_transaction_data", log.annotation_transaction_data);
                    cmd.Parameters.AddWithValue("@annotation_transaction_status", log.annotation_transaction_status);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
