using System;
using System.Data.Entity;
using System.Linq;
using Texxtoor.Editor.Context;

namespace Texxtoor.Editor {
  internal class Program {
    private static void Main(string[] args) {

      Database.SetInitializer(new EditorDbDataInitializer());

      Console.WriteLine("Creating Demo Context ...");
      var ctx = new EditorContext();
      Console.WriteLine("Creating Database ...");

      try {
        var test = ctx.Documents.ToList();
        test.ForEach(o => Console.WriteLine(o.Name));
      }      
      catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        if (ctx.GetValidationErrors().Any()) {
          Console.WriteLine(ctx.GetValidationErrors().First().ValidationErrors.First().ErrorMessage);
          Console.WriteLine(ctx.GetValidationErrors().First().ValidationErrors.First().PropertyName);
          Console.WriteLine(ctx.GetValidationErrors().First().Entry.GetType().Name);
        }
        Console.ForegroundColor = ConsoleColor.White;
      }
      Console.WriteLine("Done...hit any key");
      Console.ReadLine();
    }
  }
}
