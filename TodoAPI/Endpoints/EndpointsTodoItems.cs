using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoAPI.Contexts;
using TodoAPI.Models;

namespace TodoAPI.Endpoints
{
    public static class EndpointsTodoItems
    {
        /// <summary>
        /// Adds endpoints for working with todo items.
        /// </summary>
        /// <param name="app">WebApplication where endpoints are added</param>
        public static void MapTodoItems(this WebApplication app)
        {
            var todoItems = app.MapGroup("/todoitems");
            todoItems.MapGet("/{id}", GetTodoItem);
            todoItems.MapPost("/{listId}", CreateTodoItem).RequireAuthorization();
            todoItems.MapPut("/{id}", UpdateTodoItem).RequireAuthorization();
        }
        /// <summary>
        /// Updates existing todo item. Includes the check if user who calls the function is the member of the list that has the item, i.e. user has right to update.
        /// Can be used to update item flag <see cref="TodoItemFlag"/>
        /// </summary>
        /// <param name="id">Id of the item to be updated</param>
        /// <param name="todoItemDTO">DTO with new information</param>
        /// <param name="db">Database context</param>
        /// <param name="claimsPrincipal">Information about current user</param>
        /// <returns>No content on success, error otherwise</returns>
        static async Task<IResult> UpdateTodoItem(int id, TodoItemDTO todoItemDTO, ApplicationDbContext db, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity == null)
            {
                return TypedResults.Unauthorized();
            }
            var todoItem = await db.TodoItems.FirstOrDefaultAsync(item => item.Id == id);
            if (todoItem == null)
            {
                return TypedResults.NotFound();
            }
            var todoList = await db.TodoLists.Include(list => list.TodoUsers).FirstAsync(list => list.TodoItems.Contains(todoItem));
            TodoUser user = await db.Users.Where(user => user.UserName == claimsPrincipal.Identity.Name).FirstAsync();
            if (!todoList.TodoUsers.Contains(user))
            {
                return TypedResults.Forbid();
            }
            todoItem.Title = todoItemDTO.Title;
            todoItem.Text = todoItemDTO.Text;
            todoItem.Deadline = todoItemDTO.Deadline;
            todoItem.Flag = todoItemDTO.Flag;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        /// <summary>
        /// Creates new todo item and puts it into the list specified by list id. Includes the check if user who calls the function is the member of the list.
        /// </summary>
        /// <param name="listId">Id of the list to where new item is added</param>
        /// <param name="todoItemDTO">DTO with the information for the new item</param>
        /// <param name="db">Database context</param>
        /// <param name="claimsPrincipal">Information about current user</param>
        /// <returns>Created todo item if successful</returns>
        static async Task<IResult> CreateTodoItem(int listId, TodoItemDTO todoItemDTO, ApplicationDbContext db, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity == null)
            {
                return TypedResults.Unauthorized();
            }
            var todoList = await db.TodoLists.Include(list => list.TodoItems).Include(list => list.TodoUsers).FirstOrDefaultAsync(list => list.Id == listId);
            if (todoList == null)
            {
                return TypedResults.NotFound("list not found");
            }
            TodoUser user = await db.Users.Where(user => user.UserName == claimsPrincipal.Identity.Name).FirstAsync();
            if (!todoList.TodoUsers.Contains(user))
            {
                return TypedResults.Forbid();
            }
            var todoItem = new TodoItem()
            {
                Title = todoItemDTO.Title,
                Text = todoItemDTO.Text,
                Flag = TodoItemFlag.Active,
                Deadline = todoItemDTO.Deadline,
                CreatedBy = user
            };
            todoList.TodoItems.Add(todoItem);
            db.TodoItems.Add(todoItem);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
        }
        /// <summary>
        /// Retrieves single todo item from database.
        /// </summary>
        /// <param name="id">Id of the item</param>
        /// <param name="db">Databse context</param>
        /// <returns>Single todo item if found</returns>
        static async Task<IResult> GetTodoItem(int id, ApplicationDbContext db)
        {
            var todoItem = await db.TodoItems.Include(item => item.CreatedBy).FirstOrDefaultAsync(item => item.Id == id);
            if (todoItem == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(new TodoItemDTO(todoItem));
        }
    }
}
