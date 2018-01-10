using F_Result.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;

namespace F_Result.Methods
{
    public static class UsrWksMethods
    {

        // Метод определения идентификатора связанного сотрудника 
        // для текущего пользователя системы в роли "Руководитель проекта"
        public static List<int> GetWorkerId(FRModel db)
        {
            // Проверяем принадлежность текущего пользователя к роли "Руководитель проекта"
            bool isPrgManager = System.Web.HttpContext.Current.User.IsInRole("ProjectManager");
            List<int> WorkerIdsList = new List<int>() { -1};
            if (isPrgManager)
            {
                //Получаем идентификатор текущего пользователя
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    WorkerIdsList  = db.UsrWksRelations.Where(x => x.UserId == user).Select(x=>x.WorkerId).ToList();
            }
            return WorkerIdsList;
        }
    }
}