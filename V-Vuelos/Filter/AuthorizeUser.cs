using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using V_Vuelos.Models;

namespace V_Vuelos.Filter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeUser : AuthorizeAttribute
    {
        private Usuario _User;
        private readonly V_vuelos_ProyectoEntities db = new V_vuelos_ProyectoEntities();
        private int idOperation; //recibe 

        public AuthorizeUser(int idOperation = 0)
        {
            this.idOperation = idOperation;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                _User = (Usuario)HttpContext.Current.Session["User"];
                var lstPermisos = from p in db.Permisos
                                  where p.Rol == _User.Rol
                                  && p.Premiso == idOperation
                                  select p;
                if (lstPermisos.ToList().Count == 0)
                {
                    var ToOperation = db.Permisos.Find(idOperation);
                    int? idModule = ToOperation.Premiso;
                    
                    

                    filterContext.Result = new RedirectResult("/");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                filterContext.Result = new RedirectResult("/");
            }
        }
        
    }
}