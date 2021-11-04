using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Web.Http.Controllers;
//using System.Web.Http.Controllers;

namespace test2.ActionFilters
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(PermissionItem item, PermissionAction action)
        : base(typeof(AuthorizeActionFilter))
        {
            Arguments = new object[] { item, action };
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly PermissionItem _item;
        private readonly PermissionAction _action;
        public AuthorizeActionFilter(PermissionItem item, PermissionAction action)
        {
            _item = item;
            _action = action;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool isAuthorized = true;

            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public enum PermissionItem
    {
        User,
        Product,
        Contact,
        Review,
        Client
    }

    public enum PermissionAction
    {
        Read,
        Create,
    }
}
