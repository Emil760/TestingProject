﻿using LegacyApp.Enums;

namespace LegacyApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public ClientStatus ClientStatus { get; set; }
    }
}