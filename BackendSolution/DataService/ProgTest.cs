using Microsoft.EntityFrameworkCore;
using DataService.Entities;
using DataService.Data;

var db = new CITContext();
Console.WriteLine(db.Titles.Find("tt0052520").Name);
