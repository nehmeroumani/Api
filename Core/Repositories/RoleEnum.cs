using System.Collections.Generic;

namespace Core.Repositories
{
    public enum RoleEnum
    {
        NotSet = -1,
        Admin = 10,
        Analyst = 20,
        Annotator = 30
    }

    public class Role
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public static List<Role> List() => new List<Role>()
        {
            new Role(){Id = (int)RoleEnum.Admin,Title = RoleEnum.Admin.ToString()},
            new Role(){Id = (int)RoleEnum.Analyst,Title = RoleEnum.Analyst.ToString()},
            new Role(){Id = (int)RoleEnum.Annotator,Title = RoleEnum.Annotator.ToString()}
        };

    }
}