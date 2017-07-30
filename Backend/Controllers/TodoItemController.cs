using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Backend.DataObjects;
using Backend.Extensions;
using Backend.Models;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    [Authorize]
	public class TodoItemController : TableController<TodoItem>
	{
		public string UserId
		{
			get
			{
				var principal = this.User as ClaimsPrincipal;
				return principal.FindFirst(ClaimTypes.NameIdentifier).Value;
			}
		}

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			MobileServiceContext context = new MobileServiceContext();
			//DomainManager = new EntityDomainManager<TodoItem>(context, Request);
			DomainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);

		}

		// GET tables/TodoItem
		public IQueryable<TodoItem> GetAllTodoItems() => Query().PerUserFilter(UserId);

		// GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<TodoItem> GetTodoItem(string id) => new SingleResult<TodoItem>(Lookup(id).Queryable.PerUserFilter(UserId));

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            ValidateOwner(id);
            return UpdateAsync(id, patch);
        }

		// POST tables/TodoItem
		public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
		{
			item.UserId = UserId;

			TodoItem current = await InsertAsync(item);
			return CreatedAtRoute("Tables", new { id = current.Id }, current);
		}

		// DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id) {
            ValidateOwner(id);
            return DeleteAsync(id);
        } 

		private void ValidateOwner(string id)
		{
			var result = Lookup(id).Queryable.PerUserFilter(UserId).FirstOrDefault<TodoItem>();
			if (result == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}
		}
	}
}