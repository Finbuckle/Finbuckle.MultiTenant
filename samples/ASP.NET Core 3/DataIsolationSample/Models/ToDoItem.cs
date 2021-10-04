// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace DataIsolationSample.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }
        
        public string Title { get; set; }

        public bool Completed { get; set; }
    }
}