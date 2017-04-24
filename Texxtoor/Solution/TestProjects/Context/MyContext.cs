using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using LinqDemo.Models;

namespace LinqDemo.Context {
  public class MyContext : DbContext {

    public DbSet<Project>  Projects { get; set; }

    //public DbSet<Opus> Opera { get; set; }

    public DbSet<Element> Elements { get; set; }


  }
}
