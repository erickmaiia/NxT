﻿namespace SalesWebMVC.Models
{
    public class Department(int id, string name)
    {
        public int Id { get; } = id;
        public string Name { get; set; } = name;
    }
}
