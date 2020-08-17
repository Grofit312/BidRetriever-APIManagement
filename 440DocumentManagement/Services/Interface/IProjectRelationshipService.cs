using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ProjectRelationship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Services.Interface
{
    public interface IProjectRelationshipService
    {
        string CreateProjectRelationship(DatabaseHelper dbHelper, ProjectRelationship obj);
        List<Dictionary<string, object>> FindProjectRelationships(DatabaseHelper dbHelper, ProjectRelationshipCreteria search);
        List<Dictionary<string, object>> GetProjectRelationship(DatabaseHelper dbHelper, string project_relationship_id);
        int UpdateDataView(DatabaseHelper dbHelper, ProjectRelationship request);
    }
}
