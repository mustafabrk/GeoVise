namespace WebApi.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
    }


    public class newCls
    {
        public newCls()
        {
            TodoList = new List<Todo>();
        }
        public List<Todo> TodoList { get; set; }
    }
}
