using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using LinqDemo.Context;
using LinqDemo.Initializer;
using LinqDemo.Models;

namespace LinqDemo {
  class Program {
    private static void Main(string[] args) {
      Console.WriteLine("Init");
      var k = "";
      do {
        Console.WriteLine("Seeding? (Yes | No)");
        k = Console.ReadKey().KeyChar.ToString().ToLowerInvariant();
        Console.WriteLine("");
      } while (!(new[] { "y", "n" }).Contains(k));
      if (k == "y") {
        Database.SetInitializer(new MyInitializer());
      }
      Console.WriteLine(("Done"));
      using (var ctx = new MyContext()) {
        Console.WriteLine("Context Open");
        var p = ctx.Projects.Count();
        var o = ctx.Elements.Count();
        Console.WriteLine("{0} Projekte und {1} Opus", p, o);
        Console.WriteLine("Closing Context");
      }
      Console.WriteLine("Closed");
      // Aktionen
      do {
        Console.WriteLine("Aktion? (eXit, Copy, Move)");
        k = Console.ReadKey().KeyChar.ToString().ToLowerInvariant();
        Console.WriteLine("");
      } while (!(new[] { "x", "c", "b" }).Contains(k));
      Element startElement;
      Element targetElement;
      switch (k) {
        case "x":
          Console.WriteLine("Exiting");
          break;
        case "c":
          Console.WriteLine("Copying Opus");
          using (var ctx = new MyContext()) {
            startElement = ctx.Elements.OfType<Opus>().SingleOrDefault(o => o.Name == "Test Manual");
          }
          CopyElementTree(startElement);
          break;
        case "e":
          // put boilerplate copy to the end
          Console.WriteLine("Copying Boilerplate Chapter");
          using (var ctx = new MyContext()) {
            startElement = ctx.Elements.OfType<Section>().Single(o => o.IsBoilerplate);
            targetElement = ctx.Elements.OfType<Opus>().Single(o => o.Name == "Test Manual");
          }
          CopyElementTree(startElement, targetElement);
          break;
        case "b":
          // predefine the order
          Console.WriteLine("Copying Boilerplate Chapter");
          Action<int> write = s => Console.WriteLine(" > {0}", s);
          int[] idsInOrder;
          using (var ctx = new MyContext()) {
            startElement = ctx.Elements.OfType<Section>().Single(o => o.IsBoilerplate);
            targetElement = ctx.Elements.OfType<Opus>().Single(o => o.Name == "Test Manual");
            // we define the new order by arranging old and new Ids into an array, in test we have the new "in between"
            idsInOrder = new[] { targetElement.Children.First().Id, startElement.Id, targetElement.Children.Last().Id };
            Console.WriteLine("New Order: ");
            idsInOrder.ToList().ForEach(write);
          }
          CopyElementTree(startElement, targetElement);

          // after copy is done, we need to set the Order according to idsInOrder
          // however, any element might be a reference, so we resolve these first
          using (var ctx = new MyContext()) {
            //var normalizedIdsInOrder = ctx.Elements.OfType<Section>().Where(e => idsInOrder.Any(i => e.Predecessor == null ? i == e.Id : i == e.Predecessor.Id)).Select(e => e.Id);
            var normalizedIdsInOrder = (from i in idsInOrder 
                                        let section = ctx.Elements.OfType<Section>().Single(e => e.Id == i)
                                        select section.Parent.Id == targetElement.Id ? section : ctx.Elements.OfType<Section>().Single(s => s.Predecessor.Id == i)).ToList();
            Console.WriteLine("Normalized Order: ");
            Action<Element> write2 = s => Console.WriteLine(" > {0}", s.Id);
            normalizedIdsInOrder.ToList().ForEach(write2);
            Console.WriteLine("Now ordering");
            var newOrder = 0;
            foreach (var i in normalizedIdsInOrder) {
              i.OrderNr = newOrder++;
            }
            Console.WriteLine("Saving");
            ctx.SaveChanges();
          }
          break;
      }


      Console.WriteLine("End. Press Key to Exit");
      Console.ReadLine();
    }


    private static void CopyElementTree(Element startElement, Element targetElement = null) {
      Console.WriteLine("Create new Opus by copying one");
      try {
        using (var ctx = new MyContext()) {
          // get copies to have it in new context
          var element = ctx.Elements.Find(startElement.Id);
          var target = targetElement != null ? ctx.Elements.Find(targetElement.Id) : null;
          Section section;
          // only if success
          if (element == null) return;
          var notTracked = ctx.Elements.AsNoTracking();
          var clone = ctx.Elements.AsNoTracking().Single(o => o.Id == element.Id);
          // re-apply all dependend objects
          var opus = clone as Opus;
          if (opus != null) {
            opus.Project = ctx.Projects.Find(((Opus)element).Project.Id);
          }
          Func<IEnumerable<Element>, List<Element>> cloneChildren = null;
          // recursively loop through 
          cloneChildren = sourceChildren => {
            var sourceChildIds = sourceChildren.Select(c => c.Id);
            var clonedChildren = notTracked.Where(e => sourceChildIds.Any(c => c == e.Id)).ToList();
            foreach (var clonedChild in clonedChildren) {
              section = clonedChild as Section;
              if (section != null) {
                // remove the boilerplate sign for the target
                section.IsBoilerplate = false;
              }
              if (clonedChild.HasChildren()) {
                clonedChild.Children = cloneChildren(clonedChild.Children);
              }
            }
            return clonedChildren;
          };
          // deep copy of all children
          clone.Children = cloneChildren(element.Children);
          // store a reference to the predecessor
          ((Snippet)clone).Predecessor = element as Snippet;
          // register predecessor and copy at root level
          if (opus != null) {
            ctx.Elements.Add(opus);
            opus.PreviousVersion = (Opus)element;
          } else {
            // if not at root we need a target
            if (target != null) {
              if (target.Children == null) {
                target.Children = new List<Element>();
              }
              // remove the boilerplate sign for the target
              section = clone as Section;
              if (section != null) {
                section.IsBoilerplate = false;
              }
              // add to tracker and create graph
              ctx.Elements.Add(clone);
              target.Children.Add(clone);
            }
          }
          ctx.SaveChanges();
        }
      } catch (DbEntityValidationException ex) {
        var regClr = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var validationErrors in ex.EntityValidationErrors) {
          foreach (var validationError in validationErrors.ValidationErrors) {
            string message = string.Format("{0}:{1}",
                validationErrors.Entry.Entity.ToString(),
                validationError.ErrorMessage);
            Console.WriteLine(message);
          }
        } Console.ForegroundColor = regClr;
      }
    }

    private static void CopySection() {

    }

  }
}
