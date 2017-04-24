using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Texxtoor.BusinessLayer;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Exceptions;
using Texxtoor.DataModels.Models.Cms;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BaseLibrary.Services {
  public class PageService : Manager<PageService> {

   
    public CmsPage LoadPage(string id) {
      try {
        int pageId = 0;
        if (Int32.TryParse(id, out pageId)) {
          return Ctx
              .Pages
              .Include(p => p.Menu)
              .Include(p => p.MenuItem)
              .FirstOrDefault(p => p.Id == pageId);
        } else {
          return Ctx
              .Pages
              .Include(p => p.Menu)
              .Include(p => p.MenuItem)
              .FirstOrDefault(p => p.Alias == id);
        }
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        throw new PageNotFoundException("The requested page was not found.");
      }
    }

    public CmsPage LoadAlias(string alias, string culture = null) {
      try {
        int id = 0;
        if (Int32.TryParse(alias, out id)) {
          return LoadPage(id.ToString());
        }
        if (culture == null) {
          culture = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
        }
        return Ctx.Pages
          .Include(p => p.Menu)
          .Include(p => p.MenuItem)
          .Single(p => p.Alias == alias && p.LocaleId == culture);
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        throw new PageNotFoundException("The requested page was not found.");
      }
    }

    public CmsPage LoadDefault() {
      try {
        return LoadPage("impress"); // TODO: Configurable
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        return LoadAll().FirstOrDefault();
      }
    }

    public IEnumerable<CmsPage> LoadAll() {
      return Ctx.Pages
        .OrderByDescending(p => p.ModifiedAt);
    }

    public IEnumerable<CmsPage> LoadAllPublished() {
      return Ctx.Pages
        .Where(p => p.Status == StatusCode.Published)
        .Select(p => p).OrderByDescending(p => p.ModifiedAt);
    }

    public IEnumerable<CmsPage> LoadAllForMenu(int menuId) {      
      return Ctx.Menus
        .Include(m => m.MenuItems)
        .Single(m => m.Id == menuId)
        .MenuItems
        .Select(m => m.Page);
    }

    public void Save(CmsPage entry, User user) {
      entry.ModifiedAt = DateTime.Now;
      EnsureFieldsAreFilled(entry);
      entry.Author = user;
      Ctx.SaveChanges();
    }

    public void CreateEntry(CmsPage entry, User user) {
      entry.ModifiedAt = DateTime.Now;
      entry.CreatedAt = DateTime.Now;
      entry.Author = user;
      EnsureFieldsAreFilled(entry);
      Ctx.Pages.Add(new CmsPage() {
        Alias = entry.Alias
      });
      Ctx.SaveChanges();
    }

    public void Delete(int id) {
      var result = Ctx.Pages.FirstOrDefault(p => p.Id == id);
      Ctx.Pages.Remove(result);
      Ctx.SaveChanges();
    }

    private static void EnsureFieldsAreFilled(CmsPage entry) {
      if (string.IsNullOrEmpty(entry.SeoTitle)) {
        entry.SeoTitle = entry.PageTitle;
      }

      if (string.IsNullOrEmpty(entry.MetaKeywords)) {
        entry.MetaKeywords = entry.SeoTitle;
      }

      if (string.IsNullOrEmpty(entry.MetaDescription)) {
        entry.MetaDescription = entry.PageTitle;
      }
    }


    public CmsMenu GetMenu(int id) {
      return Ctx.Menus.Find(id);
    }

    public IQueryable<CmsPage> FindPage(string cmsSearch, string culture) {
      var result = Ctx.Pages
        .Where(p => p.PageContent.Contains(cmsSearch) && (p.LocaleId == culture || p.LocaleId == null));
      return result;
    }
  }
}