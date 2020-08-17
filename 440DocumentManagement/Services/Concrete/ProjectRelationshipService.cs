using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ProjectRelationship;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Services.Concrete
{
    public class ProjectRelationshipService : IProjectRelationshipService
    {
        public string CreateProjectRelationship(DatabaseHelper dbHelper, ProjectRelationship obj)
        {
            try
            {
                obj.project_relationship_id = string.IsNullOrEmpty(obj.project_relationship_id) ? Guid.NewGuid().ToString() : obj.project_relationship_id;
                obj.project_id = string.IsNullOrEmpty(obj.project_id) ? Guid.NewGuid().ToString() : obj.project_id;
                obj.company_id = string.IsNullOrEmpty(obj.company_id) ? Guid.NewGuid().ToString() : obj.company_id;
                obj.project_relationship_status = string.IsNullOrEmpty(obj.project_relationship_status) ? "active" : obj.project_relationship_status;
                var created_user_id = Guid.NewGuid().ToString();
                using (var cmd = dbHelper.SpawnCommand())
                {
                    string query = string.Format(@"INSERT INTO public.project_relationships (project_id, project_relation_id, company_id, contact_id, project_relationship_type_id, create_datetime, edit_datetime, create_user_id, edit_user_id, project_relationship_display_name, project_relationship_status) VALUES(@project_id, @project_relation_id, @company_id, @contact_id, @project_relationship_type_id, @create_datetime, @edit_datetime, @create_user_id, @edit_user_id, @project_relationship_display_name, @project_relationship_status);");
                    cmd.CommandText = query;

                    cmd.Parameters.AddWithValue("@project_id", obj.project_id);
                    cmd.Parameters.AddWithValue("@project_relation_id", obj.project_relationship_id);
                    cmd.Parameters.AddWithValue("@company_id", (object)obj.company_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@contact_id", (object)obj.contact_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@project_relationship_type_id", (object)obj.project_relationship_type_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_datetime", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@create_user_id", created_user_id);
                    cmd.Parameters.AddWithValue("@edit_user_id", created_user_id);
                    cmd.Parameters.AddWithValue("@project_relationship_display_name", (object)obj.project_relationship_display_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@project_relationship_status", obj.project_relationship_status);
                    cmd.ExecuteNonQuery();
                    return obj.project_relationship_id;
                }
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        public List<Dictionary<string, object>> FindProjectRelationships(DatabaseHelper dbHelper, ProjectRelationshipCreteria search)
        {
            try
            {
                string query = @"SELECT 
                                 projects.project_name, 
                                 customer_contacts.contact_display_name,
                                 customer_companies.company_name,
                                 project_relationships.project_relationship_type_id,
                                 project_relationships.project_relation_id,
                                 project_relationships.project_id,
                                 project_relationships.company_id,
                                 project_relationships.contact_id,
                                 project_relationships.project_relationship_status,
                                 project_relationships.project_relationship_display_name
                                 FROM project_relationships
                                 LEFT JOIN projects ON project_relationships.project_id = projects.project_id
                                 LEFT JOIN customer_companies ON project_relationships.company_id = customer_companies.company_id
                                 LEFT JOIN customer_contacts ON project_relationships.contact_id = customer_contacts.contact_id";
                string where = "# ";
                using (var cmd = dbHelper.SpawnCommand())
                {
                    if (!string.IsNullOrEmpty(search.company_id))
                    {
                        where += " project_relationships.company_id = @company_id * ";
                        cmd.Parameters.AddWithValue("@company_id", search.company_id);
                    }
                    if (!string.IsNullOrEmpty(search.project_id))
                    {

                        where += " project_relationships.project_id = @project_id * ";
                        cmd.Parameters.AddWithValue("@project_id", search.project_id);
                    }
                    if (!string.IsNullOrEmpty(search.contact_id))
                    {
                        where += "project_relationships.contact_id = @contact_id * ";
                        cmd.Parameters.AddWithValue("@contact_id", search.contact_id);
                    }
                    where = where.Remove(where.Length - 2);
                    where = where.Replace("# ", " WHERE ").Replace("* ", " OR ");

                    cmd.CommandText = query + where;

                    var result = new List<Dictionary<string, object>>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "company_id", ApiExtension.GetString(reader["company_id"]) },
                                { "company_display_name", ApiExtension.GetString(reader["company_name"]) },
                                { "contact_id", ApiExtension.GetString(reader["contact_id"]) },
                                { "contact_display_name", ApiExtension.GetString(reader["contact_display_name"]) },
                                { "project_id", ApiExtension.GetString(reader["project_id"]) },
                                { "project_display_name", ApiExtension.GetString(reader["project_name"]) },
                                { "project_relationship_display_name", ApiExtension.GetString(reader["project_relationship_display_name"]) },
                                { "project_relationship_id", ApiExtension.GetString(reader["project_relation_id"]) },
                                { "project_relationship_status", ApiExtension.GetString(reader["project_relationship_status"]) },
                                { "project_relationship_type_id", ApiExtension.GetString(reader["project_relationship_type_id"]) },
                            });
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        public List<Dictionary<string, object>> GetProjectRelationship(DatabaseHelper dbHelper, string project_relationship_id)
        {
            try
            {
                string query = string.Format(@"SELECT 
                                 projects.project_name, 
                                 customer_contacts.contact_display_name,
                                 customer_companies.company_name,
                                 project_relationships.project_relationship_type_id,
                                 project_relationships.project_relation_id,
                                 project_relationships.project_id,
                                 project_relationships.company_id,
                                 project_relationships.contact_id,
                                 project_relationships.project_relationship_status,
                                 project_relationships.project_relationship_display_name
                                 FROM project_relationships
                                 LEFT JOIN projects ON project_relationships.project_id = projects.project_id
                                 LEFT JOIN customer_companies ON project_relationships.company_id = customer_companies.company_id
                                 LEFT JOIN customer_contacts ON project_relationships.contact_id = customer_contacts.contact_id WHERE project_relationships.project_relation_id='{0}'", project_relationship_id);
                using (var cmd = dbHelper.SpawnCommand())
                {
                    cmd.CommandText = query;
                    var result = new List<Dictionary<string, object>>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                { "company_id", ApiExtension.GetString(reader["company_id"]) },
                                { "company_display_name", ApiExtension.GetString(reader["company_name"]) },
                                { "contact_id", ApiExtension.GetString(reader["contact_id"]) },
                                { "contact_display_name", ApiExtension.GetString(reader["contact_display_name"]) },
                                { "project_id", ApiExtension.GetString(reader["project_id"]) },
                                { "project_display_name", ApiExtension.GetString(reader["project_name"]) },
                                { "project_relationship_display_name", ApiExtension.GetString(reader["project_relationship_display_name"]) },
                                { "project_relationship_id", ApiExtension.GetString(reader["project_relation_id"]) },
                                { "project_relationship_status", ApiExtension.GetString(reader["project_relationship_status"]) },
                                { "project_relationship_type_id", ApiExtension.GetString(reader["project_relationship_type_id"]) },
                            });
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        public int UpdateDataView(DatabaseHelper dbHelper, ProjectRelationship obj)
        {
            try
            {
                obj.project_relationship_status = string.IsNullOrEmpty(obj.project_relationship_status) ? "active" : obj.project_relationship_status;
                var created_user_id = Guid.NewGuid().ToString();
                using (var cmd = dbHelper.SpawnCommand())
                {
                    string command = @"UPDATE public.project_relationships SET edit_datetime=@edit_datetime, edit_user_id=@edit_user_id , project_relationship_status=@project_relationship_status ";
                    cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@edit_user_id", created_user_id);
                    cmd.Parameters.AddWithValue("@project_relationship_status", obj.project_relationship_status);

                    if (!string.IsNullOrEmpty(obj.project_id))
                    {
                        command += " ,project_id = @project_id";
                        cmd.Parameters.AddWithValue("@project_id", obj.project_id);
                    }
                    if (!string.IsNullOrEmpty(obj.company_id))
                    {
                        command += " ,company_id = @company_id";
                        cmd.Parameters.AddWithValue("@company_id", (object)obj.company_id ?? DBNull.Value);
                    }

                    if (!string.IsNullOrEmpty(obj.contact_id))
                    {
                        command += " ,contact_id = @contact_id";
                        cmd.Parameters.AddWithValue("@contact_id", (object)obj.contact_id ?? DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(obj.project_relationship_type_id))
                    {
                        command += " ,project_relationship_type_id = @project_relationship_type_id";
                        cmd.Parameters.AddWithValue("@project_relationship_type_id", (object)obj.project_relationship_type_id ?? DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(obj.project_relationship_display_name))
                    {
                        command += " ,project_relationship_display_name = @project_relationship_display_name";
                        cmd.Parameters.AddWithValue("@project_relationship_display_name", (object)obj.project_relationship_display_name ?? DBNull.Value);
                    }
                    command += " WHERE project_relation_id=@project_relation_id;";
                    cmd.Parameters.AddWithValue("@project_relation_id", obj.project_relationship_id);
                    cmd.CommandText = command;
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }
    }
}
