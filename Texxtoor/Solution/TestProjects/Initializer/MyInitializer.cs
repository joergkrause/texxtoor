using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using LinqDemo.Context;
using LinqDemo.Models;

namespace LinqDemo.Initializer {
  public class MyInitializer : DropCreateDatabaseAlways<MyContext> {

    private const string Lorem = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";

    protected override void Seed(MyContext context) {
      base.Seed(context);
      Console.WriteLine("Seeding Database");
      // Project
      var p = new Project {
        Name = "Test",
        Short = "Test for Moving / Copying",
        Active = true
      };
      Console.WriteLine("Add Project");
      context.Projects.Add(p);
      Console.WriteLine("Saving");
      context.SaveChanges();
      Console.WriteLine("Saved");

      Console.WriteLine("Create Opus");
      // Opus with boilerplates
      var ob = new Opus {
        Name = "Boilerplates",
        Active = true,
        LocaleId = "de",
        Variation = VariationType.HeadRevision,
        Version = 1,
        Project = p
      };
      context.Elements.Add(ob);
      var s = CreateChapter(ob, "Boilerplate", true);
      Console.WriteLine("Chapter " + s.Name + " created");

      // Opus with regular content
      var oc = new Opus {
        Name = "Test Manual",
        Active = true,
        LocaleId = "de",
        Variation = VariationType.HeadRevision,
        Version = 1,
        Project = p
      };
      context.Elements.Add(oc);
      var r = CreateChapter(oc, "Regular Text");
      Console.WriteLine("Chapter " + r.Name + " created");
      r = CreateChapter(oc, "Second Text");
      Console.WriteLine("Chapter " + r.Name + " created");

      Console.WriteLine("Saving");
      context.SaveChanges();
      Console.WriteLine("Saved");
      // Content, two chapters with 2 text snippets each

      Console.WriteLine("Exiting Seed...");
    }

    private Section CreateChapter(Opus parent, string name, bool isBoilerplate = false) {
      var s = new Section {
        Content = Encoding.UTF8.GetBytes("Kapitel " + name),
        Name = "Kapitel " + name,
        Parent = parent,
        IsBoilerplate = isBoilerplate,
        OrderNr = parent.Children.Any() ? parent.Children.Max(c => c.OrderNr) + 1 : 0,
        LocaleId = parent.LocaleId,
        Children = new List<Element>(new[] {
          new TextSnippet {
            Content = Encoding.UTF8.GetBytes(GetLorem()),
            Name = "Text 1 " + name,
            OrderNr = 1,
            Children = new List<Element>(new [] { new SidebarSnippet {
              Name = "Inner Sidebar",
              Content = Encoding.UTF8.GetBytes(GetLorem())              
            }
            })
          },
          new TextSnippet {
            Content = Encoding.UTF8.GetBytes(GetLorem()),
            Name = "Text 2 " + name,
            OrderNr = 2
          },
          new TextSnippet {
            Content = Encoding.UTF8.GetBytes(GetLorem()),
            Name = "Text 3 " + name,
            OrderNr = 3
          }
        })
      };
      parent.Children.Add(s);
      return s;
    }

    private string GetLorem() {
      var start = new Random().Next(Lorem.Length / 2);
      var end = Math.Min(new Random().Next(Lorem.Length / 2), Lorem.Length);
      return Lorem.Substring(start, end);
    }

  }
}
