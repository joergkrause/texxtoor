using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.EPub.Model;
using Texxtoor.DataModels.Model.Cms.Localization;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.JobPortal;
using Texxtoor.DataModels.Models.Marketing;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Functions;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.DataModels.Context {
  public class PortalContext : IdentityDbContext<User, Role, int, TexxtoorLogin, TexxtoorUserRole, TexxtoorUserClaim>, IUnitOfWork, IDisposable {

    public PortalContext() {
      ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;
    }

    /// <summary>
    /// Assure that the modified at field is always written if an object has been touched.
    /// </summary>
    bool DetectAndAdjustChanges() {
      var now = DateTime.Now;
      ChangeTracker.DetectChanges();
      var entries = (from e in ChangeTracker.Entries()
                     where e.Entity != null && e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted
                     select e).ToList();
      foreach (var entry in entries) {
        var timestampedEntity = entry.Entity as EntityBase;
        if (timestampedEntity == null) continue;
        timestampedEntity.ModifiedAt = now;
        if (entry.State == EntityState.Added) {
          timestampedEntity.CreatedAt = now;
        } else {
          Entry(timestampedEntity).Property(p => p.CreatedAt).IsModified = false;
        }
      }
      return entries.Any();
    }

    public PortalContext(string cnt)
      : base(cnt) {
    }

    # region CMS

    public DbSet<CmsPage> Pages { get; set; }
    public DbSet<CmsMedia> Media { get; set; }
    public DbSet<CmsMenu> Menus { get; set; }
    public DbSet<CmsMenuItem> MenuItems { get; set; }

    // Localization Table
    public DbSet<ResourceBase> Localization { get; set; }

    # endregion

    # region Job Portal

    public DbSet<JobAdvertisment> JobAdvertisments { get; set; }

    public DbSet<JobContact> JobContacts { get; set; }

    public DbSet<JobCategory> JobCategories { get; set; }

    public DbSet<JobCV> JobCVs { get; set; }

    # endregion

    # region Common

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<TemplateGroup> TemplateGroups { get; set; }

    public DbSet<Template> Templates { get; set; }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<Country> Countries { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Language> Languages { get; set; }

    public DbSet<AddressBook> AddressBook { get; set; }

    public DbSet<Application> Applications { get; set; }

    public DbSet<ContactFormRequest> ContactFormRequests { get; set; }

    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageText> MessageTexts { get; set; }
    public DbSet<ContributorMatrix> ContributorMatrix { get; set; }
    public DbSet<ConsumerMatrix> ConsumerMatrix { get; set; }
    public DbSet<LanguageMatrix> LanguageMatrix { get; set; }

    public DbSet<WizardWorkflow> WizardWorkflows { get; set; }

    # endregion

    # region Reader Portal

    //public DbSet<DcmesElement> DcmesElement { get; set; }

    public DbSet<EpubBook> Books { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Work> Works { get; set; }

    public DbSet<Published> Published { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<FrozenFragment> FrozenFragments { get; set; }

    public DbSet<WorkingFragment> WorkingFragments { get; set; }

    //public DbSet<ContentCollection> Collections { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<UserFile> UserFiles { get; set; }

    public DbSet<UserActivity> UserActivities { get; set; }

    public DbSet<Catalog> Catalog { get; set; }

    # endregion

    # region Reader Social Portal

    public DbSet<ReaderGroup> ReaderGroups { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Bookmark> Bookmarks { get; set; }

    public DbSet<Theme> Themes { get; set; }

    public DbSet<QuestionsAnswers> QuestionsAnswers { get; set; }

    public DbSet<Forum> Forums { get; set; }

    public DbSet<WorkChat> WorkChats { get; set; }

    # endregion

    # region Reader Shop Portal

    public DbSet<OrderMedia> OrderMedias { get; set; }

    public DbSet<OrderProduct> OrderProducts { get; set; }

    public DbSet<OrderShippingAddress> OrderShippingAddresses { get; set; }

    public DbSet<OrderInvoiceAddress> OrderInvoiceAddresses { get; set; }

    # endregion

    #region Author Protal

    public DbSet<Element> Elements { get; set; }

    # region Repositories

    public DbSet<TermSet> TermSets { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<CiteTerm> CiteTerms { get; set; }
    public DbSet<DefinitionTerm> DefinitionTerms { get; set; }
    public DbSet<AbbreviationTerm> AbbreviationTerms { get; set; }
    public DbSet<IdiomTerm> IdiomTerms { get; set; }
    public DbSet<VariableTerm> VariableTerms { get; set; }
    public DbSet<LinkTerm> LinkTerms { get; set; }

    # endregion

    public DbSet<Resource> Resources { get; set; }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Opus> Opuses { get; set; }
    public DbSet<ElementMatrix> ElementMatrix { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<MarketingPackage> MarketingPackages { get; set; }
    public DbSet<IsbnStore> Isbns { get; set; }
    public DbSet<ContributorRatio> ContributorRatios { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamRole> TeamRoles { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }

    public DbSet<WorkroomChat> WorkroomChats { get; set; }
    public DbSet<WorkitemChat> WorkitemChats { get; set; }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Sale> Sales { get; set; }

    # region Workflows

    public DbSet<WorkflowStepDefinition> WorkflowStepDefinitions { get; set; }
    public DbSet<Workflow> Workflows { get; set; }

    # endregion

    #endregion

    # region Service Tracking

    public DbSet<SearchTracking> SearchTracking { get; set; }

    public DbSet<Session> Sessions { get; set; }

    # endregion

    # region Marketing Tracking

    public DbSet<Imprint> Imprints { get; set; }

    public DbSet<ClickCount> ClickCount { get; set; }

    public DbSet<SalesTracking> SalesTracking { get; set; }

    # endregion

    protected override void OnModelCreating(DbModelBuilder modelBuilder) {

      modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
      modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

      base.OnModelCreating(modelBuilder);

      # region Configurations

      modelBuilder.Configurations.Add(new OpusConfiguration());

      # endregion

      # region -== Define n:n relations and appropriate table names ==-


      modelBuilder.Entity<User>().ToTable("Users", "Common");
      modelBuilder.Entity<Role>().ToTable("Roles", "Common");
      //modelBuilder.Entity<TexxtoorUserRole>().ToTable("UserRoles", "Common");
      modelBuilder.Entity<TexxtoorLogin>().ToTable("UserLogins", "Common");
      modelBuilder.Entity<TexxtoorUserClaim>().ToTable("UserClaims", "Common"); // no claims currently

      //modelBuilder
      modelBuilder.Entity<User>().HasMany<TexxtoorUserRole>(u => u.Roles);
      // help EF to map the crude IIdentity schema
      modelBuilder.Entity<TexxtoorUserRole>().HasKey(r => new { r.UserId, r.RoleId }).ToTable("UserRoles", "Common");

      modelBuilder
          .Entity<CmsMenuItem>()
          .HasMany<Role>(r => r.Roles)
          .WithMany(m => m.MenuItems)
          .Map(m => m.ToTable("MenuItems_x_Roles", "Cms")
              .MapLeftKey("MenuItemId")
              .MapRightKey("RoleId"));

      modelBuilder
          .Entity<CmsMenu>()
          .HasMany<Role>(r => r.Roles)
          .WithMany(m => m.Menus)
          .Map(m => m.ToTable("Menus_x_Roles", "Cms")
              .MapLeftKey("MenuId")
              .MapRightKey("RoleId"));

      modelBuilder
        .Entity<CmsMenu>()
        .HasMany<CmsMenuItem>(r => r.MenuItems)
        .WithMany(m => m.Menu)
        .Map(m => m.ToTable("Menus_x_MenuItems", "Cms")
            .MapLeftKey("MenuId")
            .MapRightKey("MenuItemId"));

      modelBuilder
        .Entity<Catalog>()
        .HasMany<Published>(f => f.Published)
        .WithMany(g => g.Catalogs)
        .Map(m => m.ToTable("Catalog_x_Published", "Reader")
                  .MapLeftKey("CatalogId")
                  .MapRightKey("PublishedId"));

      modelBuilder
        .Entity<Published>()
        .HasMany<ResourceFile>(f => f.ResourceFiles)
        .WithMany(g => g.Published)
        .Map(m => m.ToTable("Published_x_ResourceFiles", "Reader")
                  .MapLeftKey("PublishedId")
                  .MapRightKey("ResourceFileId"));

      modelBuilder
        .Entity<Published>()
        .HasMany<OrderMedia>(f => f.SupportedMedia)
        .WithMany(g => g.AllowsForPublished)
        .Map(m => m.ToTable("Published_x_SupportedMedia", "Reader")
                  .MapLeftKey("PublishedId")
                  .MapRightKey("OrderMediaId"));

      modelBuilder
        .Entity<Published>()
        .HasMany<TemplateGroup>(f => f.PreferredTemplateGroup)
        .WithMany(g => g.PublishedUsings)
        .Map(m => m.ToTable("Published_x_TemplateGroup", "Reader")
                  .MapLeftKey("PublishedId")
                  .MapRightKey("TemplateGroupId"));

      modelBuilder
        .Entity<Published>()
        .HasMany<User>(f => f.Authors)
        .WithMany(g => g.Published)
        .Map(m => m.ToTable("Published_x_Authors", "Reader")
                  .MapLeftKey("PublishedId")
                  .MapRightKey("UserId"));

      modelBuilder
        .Entity<ReaderGroup>()
        .HasMany<Theme>(t => t.Themes)
        .WithMany(g => g.ReaderGroups)
        .Map(m => m.ToTable("ReaderGroup_x_Themes", "Reader")
                  .MapLeftKey("ReaderGroupId")
                  .MapRightKey("ThemeId"));

      modelBuilder
        .Entity<ReaderGroup>()
        .HasMany<User>(u => u.Members)
        .WithMany(g => g.ReaderGroupsMember)
        .Map(m => m.ToTable("ReaderGroup_x_Members", "Reader")
                  .MapLeftKey("ReaderGroupId")
                  .MapRightKey("UserId"));

      modelBuilder
        .Entity<ReaderGroup>()
        .HasMany<User>(u => u.Admins)
        .WithMany(g => g.ReaderGroupsAdmin)
        .Map(m => m.ToTable("ReaderGroup_x_Admins", "Reader")
                  .MapLeftKey("ReaderGroupId")
                  .MapRightKey("UserId"));

      modelBuilder
        .Entity<TermSet>()
        .HasMany<Term>(u => u.Terms)
        .WithMany(g => g.TermSets)
        .Map(m => m.ToTable("TermSet_x_Term", "Author")
                  .MapLeftKey("TermSetId")
                  .MapRightKey("TermId"));

      modelBuilder
        .Entity<OrderProduct>()
        .HasMany<OrderMedia>(m => m.Media)
        .WithMany(p => p.Products)
        .Map(m => m.ToTable("OrderProduct_x_OrderMedia", "Shop")
                  .MapLeftKey("OrderProductId")
                  .MapRightKey("OrderMediaId"));

      modelBuilder
        .Entity<JobCategory>()
        .HasMany<JobAdvertisment>(r => r.JobAdvertisments)
        .WithMany(u => u.Categories)
        .Map(m => m.ToTable("JobAdvertisment_x_JobCategory", "JobPortal")
                    .MapLeftKey("JobAdvertismentId")
                    .MapRightKey("JobCategoryId"));


      # endregion -== Define n:n relations and appropriate table names ==-

      # region  -== Define 1:n relations and appropriate table names ==-

      modelBuilder
        .Entity<Invoice>()
        .HasRequired(i => i.Creditor)
        .WithMany()
        .WillCascadeOnDelete(false);

      modelBuilder
        .Entity<Invoice>()
        .HasRequired(i => i.Debitor)
        .WithMany()
        .WillCascadeOnDelete(false);

      //modelBuilder
      //  .Entity<TemplateGroup>()
      //  .HasRequired(i => i.Templates)
      //  .WithMany()
      //  .WillCascadeOnDelete(true);

      //modelBuilder
      //  .Entity<TemplateGroup>()
      //  .HasRequired(i => i.Admin)
      //  .WithMany()
      //  .WillCascadeOnDelete(false);

      //modelBuilder
      //  .Entity<TemplateGroup>()
      //  .HasOptional(i => i.Owner)
      //  .WithMany()
      //  .WillCascadeOnDelete(false);

      //modelBuilder
      //  .Entity<Template>()
      //  .HasRequired(i => i.Group)
      //  .WithMany()
      //  .WillCascadeOnDelete(false);

      # endregion  -== Define 1:n relations and appropriate table names ==-

      # region  -== Define 1:1/0 relations with appropriate keys ==-

      modelBuilder.Entity<OrderProduct>()
        .HasOptional(u => u.ShippingAddress)
        .WithRequired(p => p.OrderProduct)
        .Map(m => m.MapKey("OrderProductForShippingId"))
        .WillCascadeOnDelete(false);

      modelBuilder.Entity<OrderProduct>()
        .HasOptional(u => u.BillingAddress)
        .WithRequired(p => p.OrderProduct)
        .Map(m => m.MapKey("OrderProductForBillingId"))
        .WillCascadeOnDelete(false);

      modelBuilder.Entity<User>()
        .HasOptional(u => u.Profile)
        .WithRequired(p => p.User)
        .Map(m => m.MapKey("ProfileId"));

      modelBuilder.Entity<Opus>()
        .HasOptional(u => u.Published)
        .WithOptionalDependent(p => p.SourceOpus)
        .Map(m => m.MapKey("PublishedId"));

      modelBuilder.Entity<Published>()
        .HasOptional(u => u.Imprint)
        .WithOptionalDependent(p => p.Published)
        .Map(m => m.MapKey("Imprint_Id"));

      // we need to add this explicitly because the PreviousVersion relation fools EF 
      //modelBuilder.Entity<Element>()
      //  .HasOptional(u => u.Parent)
      //  .WithMany()
      //  .Map(c => c.MapKey("Parent_Id"));

      //modelBuilder.Entity<Snippet>()
      //  .HasOptional(u => u.Predecessor)
      //  .WithOptionalDependent()
      //  .Map(c => c.MapKey("Predecessor_Id"));


      # endregion  -== Define 1:1 relations with appropriate keys ==-

      # region -== Cascading Delete ==-

      // remove addresses if profile is being deleted
      modelBuilder.Entity<UserProfile>()
        .HasMany(a => a.Addresses)
        .WithRequired(p => p.Profile)
        .WillCascadeOnDelete(true);

      # endregion

    }

    # region API

    public ObjectResult<T> ExecuteQuery<T>(string query, params object[] p) {
      var ctx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext;
      return ctx.ExecuteStoreQuery<T>(query, p);
    }

    public void ApplyValues<T>(T entity) where T : class {
      var ctx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext;
      var eset = ctx.MetadataWorkspace.GetEntityContainer(ctx.DefaultContainerName, DataSpace.CSpace)
        .BaseEntitySets.Single(es => es.ElementType.Name == typeof(T).Name);
      var key = ctx.CreateEntityKey(eset.Name, entity);
      ctx.GetObjectByKey(key);
      ctx.ApplyCurrentValues<T>(eset.Name, entity);
    }

    public void Detach(object entity) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.Detach(entity);
    }

    public void Attach(object entity) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.Attach(entity as IEntityWithKey);
    }

    public object Clone(object entity) {
      var type = entity.GetType();
      var clone = Activator.CreateInstance(type);

      foreach (var property in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.SetProperty)) {
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(EntityReference<>)) continue;
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(EntityCollection<>)) continue;
        if (property.PropertyType.IsSubclassOf(typeof(EntityObject))) continue;

        if (property.CanWrite) {
          property.SetValue(clone, property.GetValue(entity, null), null);
        }
      }

      return clone;
    }

    public void LoadProperty(object entity, string navProperty) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.LoadProperty(entity, navProperty);
    }

    public void LoadProperty<T>(T entity, Expression<Func<T, object>> expression) where T : EntityBase {
      var exp = expression.Body;
      string name = null;
      if (exp.NodeType == ExpressionType.MemberAccess) {
        name = ((MemberExpression)exp).Member.Name;
      }
      if (exp.NodeType == ExpressionType.Convert) {
        name = ((MemberExpression)((UnaryExpression)exp).Operand).Member.Name;

      }
      if (!String.IsNullOrEmpty(name)) {
        LoadProperty(entity, name);
      }
    }

    public void LoadProperty<T>(T entity, params Expression<Func<T, object>>[] expressions) where T : EntityBase {
      foreach (var expression in expressions) {
        LoadProperty(entity, expression);
      }
    }

    public void UndoChanges() {
      foreach (var entry in this.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified)) {
        entry.State = EntityState.Unchanged;
      }
    }

    public void DeleteObject(object entity) {
      ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.DeleteObject(entity);
    }

    public IDbTransaction BeginTransaction() {
      var conn = ((IObjectContextAdapter)this).ObjectContext.Connection;
      // always start transaction on a fresh connection
      if (conn.State == ConnectionState.Open) {
        conn.Close();
      }
      if (conn.State == ConnectionState.Closed) {
        conn.Open();
      }
      return conn.BeginTransaction();
    }

    public int Save() {
      var message = new List<string>();
      try {
        if (DetectAndAdjustChanges()) {
          var result = SaveChanges();
          return result;
        }
      } catch (DbUpdateException uex) {
        Exception ex = uex;
        while (ex.InnerException != null) {
          message.Add(ex.InnerException.Message);
          ex = ex.InnerException;
        }
      } catch (DbEntityValidationException) {
        message.AddRange(GetValidationErrors().SelectMany(v => v.ValidationErrors.Select(i =>
          String.Format("'{0}' in Entity '{1}' for Property '{2}'",
            i.ErrorMessage, v.Entry.Entity, i.PropertyName))));
      }
      if (message.Any()) {
        var write = String.Join(" | ", message.ToArray());
        Debug.WriteLine(write);
        throw new DataException(write);
      }
      return 0;
    }

    # endregion


  }

}