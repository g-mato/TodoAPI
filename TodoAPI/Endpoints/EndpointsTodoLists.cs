using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoAPI.Contexts;
using TodoAPI.Models;

namespace TodoAPI.Endpoints
{
    public static class EndpointsTodoLists
    {
        /// <summary>
        /// Adds endpoints for working with todo lists.
        /// </summary>
        /// <param name="app">WebApplication where endpoints are added</param>
        public static void MapTodoLists(this WebApplication app)
        {
            var todoItems = app.MapGroup("/todolists");
            todoItems.MapGet("/", GetAllTodoList);
            todoItems.MapGet("/{id}", GetTodoList);
            todoItems.MapPost("/", CreateTodoList).RequireAuthorization();
            todoItems.MapPost("/{id}/adduser", AddUserToList).RequireAuthorization();
        }
        /// <summary>
        /// Adds user to todo list. List must exist and the user who calls the function must already be a member of that list.
        /// Does nothing if specified user is a member of the list already.
        /// </summary>
        /// <param name="id">Id of the list to where new member should be added</param>
        /// <param name="todoUserDTO">DTO with user name</param>
        /// <param name="db">Database context</param>
        /// <param name="claimsPrincipal">Information about current user</param>
        /// <returns></returns>
        static async Task<IResult> AddUserToList(int id, TodoUserDTO todoUserDTO, ApplicationDbContext db, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity == null)
            {
                return TypedResults.Unauthorized();
            }
            var todoList = await db.TodoLists.Include(list => list.TodoUsers).FirstOrDefaultAsync(list => list.Id == id);
            if (todoList == null)
            {
                return TypedResults.NotFound("list not found");
            }
            TodoUser user = await db.Users.Where(user => user.UserName == claimsPrincipal.Identity.Name).FirstAsync();
            if (!todoList.TodoUsers.Contains(user))
            {
                return TypedResults.Forbid();
            }
            TodoUser? userToAdd = await db.Users.Where(user => user.UserName == todoUserDTO.name).FirstOrDefaultAsync();
            if (userToAdd == null)
            {
                return TypedResults.NotFound("user not found");
            }
            if (todoList.TodoUsers.Contains(userToAdd))
            {
                return TypedResults.Ok();
            }
            todoList.TodoUsers.Add(userToAdd);
            await db.SaveChangesAsync();
            return TypedResults.Ok();
        }
        /// <summary>
        /// Creates new todo list and assigns its creator as a member for that list.
        /// </summary>
        /// <param name="todoListDTO">DTO with required Title field</param>
        /// <param name="db">Database context</param>
        /// <param name="claimsPrincipal">Information about current user</param>
        /// <returns></returns>
        static async Task<IResult> CreateTodoList(TodoListDTO todoListDTO, ApplicationDbContext db, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity == null)
            {
                return TypedResults.Unauthorized();
            }
            var todoList = new TodoList()
            {
                Title = todoListDTO.Title
            };
            todoList.TodoUsers.Add(await db.Users.Where(user => user.UserName == claimsPrincipal.Identity.Name).FirstAsync());
            db.TodoLists.Add(todoList);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/todolists/{todoList.Id}", new TodoListDTO(todoList));
        }
        /// <summary>
        /// Retrieves single list specified by id. List includes all its todo items.
        /// </summary>
        /// <param name="id">Id of the list</param>
        /// <param name="db">Database context</param>
        /// <returns>Single list of items.</returns>
        static async Task<IResult> GetTodoList(int id, ApplicationDbContext db)
        {
            var todoList = await db.TodoLists.Include(list => list.TodoItems).Include(list => list.TodoUsers).FirstOrDefaultAsync(list => list.Id == id);
            if (todoList == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(new TodoListDTO(todoList));
        }

        /// <summary>
        /// Retrieves all lists from database. Each item contains id and Title, but lists of users and todo items are omitted.
        /// </summary>
        /// <param name="db">Database context</param>
        /// <returns>All lists in the database.</returns>
        static async Task<IResult> GetAllTodoList(ApplicationDbContext db)
        {
            return TypedResults.Ok(await db.TodoLists.Select(list => new TodoListDTO(list, false)).ToArrayAsync());
        }
    }
}
