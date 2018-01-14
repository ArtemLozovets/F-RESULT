﻿using F_Result.Models;
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

            // Флаг для отмены фильтрации по руководителю проекта в запросе.
            // Если текущий пользователь не принадлежит к роли "Руководитель проекта" - 
            // возвращаем один элемент списка со значением "-1". 
            List<int> WorkerIdsList = new List<int>() { -1 };
            if (isPrgManager)
            {
                //Получаем идентификатор текущего пользователя
                var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                //Получаем список связанных сотрудников
                WorkerIdsList = db.UsrWksRelations.Where(x => x.UserId == user).Select(x => x.WorkerId).ToList();
            }
            return WorkerIdsList;
        }


        // Метод определения наличия прав доступа к данной сущности у текущего пользователя
        // на основании сопоставления пользователей системы сотрудникам
        public static bool isAllowed(FRModel db, int PrjId)
        {
            // Проверяем принадлежность текущего пользователя к роли "Руководитель проекта"
            bool isPrgManager = System.Web.HttpContext.Current.User.IsInRole("ProjectManager");
            if (!isPrgManager)
            {
                return true; //Разрешаем доступ к сущности
            }
            else
            {
                //Получаем идентификатор текущего пользователя
                var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                //Получаем список связанных сотрудников
               // WorkerIdsList = db.UsrWksRelations.Where(x => x.UserId == user).Select(x => x.WorkerId).ToList();
                var wks

                return false;
            }

        }

    }
}