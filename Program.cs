using System;

namespace Queryable
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var queryProvider = new QueryProvider();
            var businessObjects = new Query<BusinessObject>(queryProvider);

            var result = businessObjects.Where(e => e.One == "Hello world").ToList();

            Console.ReadKey();
        }
    }

    public class BusinessObject
    {
        public string One { get; set; }

        public int Two { get; set; }
    }
}