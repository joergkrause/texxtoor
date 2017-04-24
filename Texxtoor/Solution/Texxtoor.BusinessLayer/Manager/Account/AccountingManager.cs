using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models;
using Texxtoor.DataModels.Models.Author;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.JobPortal;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;
using Texxtoor.ViewModels.Author;

namespace Texxtoor.BusinessLayer {

  public class AccountingManager : Manager<AccountingManager> {

    # region --== Invoicing ==--


    public IList<Invoice> GetInvoices(bool outgoing, string userName) {
      var invs = Ctx.Invoices
        .Where(i => ((i.Creditor.UserName == userName && !outgoing) || (i.Debitor.UserName == userName && outgoing)) && i.Paid == false)
        .ToList();
      return invs;
    }

    /// <summary>
    /// Get all internal transaction for user
    /// </summary>
    /// <param name="userName">User Name</param>
    /// <returns></returns>
    public List<Account> GetAllTransactionsForUser(string userName) {
      var accounts = Ctx.Accounts
                     .Where(a => (a.Owner.UserName == userName))
                     .OrderByDescending(a => a.CreatedAt)
                     .ToList();
      return accounts;
    }

    public List<Account> GetTransactionsForUser(string userName, TransactionType type, decimal amount, string description) {
      var checkDesc = String.IsNullOrEmpty(description);
      var accounts = Ctx.Accounts
                     .Where(a =>
                       a.Owner.UserName == userName &&
                       (checkDesc || a.Description.Contains(description)) &&
                       (a.TransactionType == type || type == TransactionType.All)
                       )
                     .OrderByDescending(a => a.CreatedAt)
                     .ToList()
                     .Where(a => a.Amount == amount || amount == 0)
                     .ToList();
      return accounts;
    }

    /// <summary>
    /// get current credit amount for user
    /// </summary>
    /// <param name="userName">User Name</param>
    /// <returns></returns>
    public decimal GetCurrentCreditAmountForUser(string userName) {

      decimal? creditAmount = Ctx.Accounts
                      .Where(a => (a.Owner.UserName == userName) && (a.TransactionType != TransactionType.FromBankAccount))
                      .Sum(a => (decimal?)a.Amount);

      return creditAmount ?? 0;
    }

    /// <summary>
    /// Add internal transaction details.
    /// </summary>
    /// <param name="amount">Amount</param>
    /// <param name="type">Type of transaction</param>
    /// <param name="userName">User Name</param>
    /// <returns></returns>
    public void AddAccountTransactions(decimal amount, TransactionType type, string userName) {
      var user = Manager<UserManager>.Instance.GetUserByName(userName);
      var acc = new Account {
        Owner = user,
        Description = String.Format("Your withdraw amount to the PayPal Account"),
        Amount = amount,
        TransactionType = type
      };
      Ctx.Accounts.Add(acc);
      SaveChanges();
    }

    public BillingSummary CreateBillingSummary(int id, string userName) {
      var bs = new BillingSummary();
      var op = ProjectManager.Instance.GetOpus(id, userName);
      var tm = ProjectManager.Instance.GetTeamMembersOfOpus(op).ToList();
      // is this user a contributor?
      bs.Opus = op;
      bs.TeamLead = tm.First(t => t.TeamLead).Member;
      bs.UserIsContributor = tm.Any(t => t.TeamLead && t.Member.UserName == userName);
      var cinv = Ctx.Invoices
        .Where(i => i.Creditor.UserName == userName && i.Paid == false)
        .SelectMany(i => i.Positions);
      var dinv = Ctx.Invoices
        .Where(i => i.Debitor.UserName == userName && i.Paid == false)
        .SelectMany(i => i.Positions);
      bs.CreditorBalance = cinv.Any() ? cinv.Sum(p => p.Amount) : 0m;
      bs.DebitorBalance = dinv.Any() ? dinv.Sum(p => p.Amount) : 0m;
      bs.OutgoingInvoices = cinv.Count();
      bs.IncomingInvoices = dinv.Count();
      return bs;
    }
    /// <summary>
    ///  Give virtual credit/Debit amount to respective authors
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="debitorName"></param>
    /// <param name="creditorName"></param>
    /// <param name="transactionType"></param>
    /// <returns></returns>
    public bool CreditAmount(decimal amount, string debitorName, string creditorName, TransactionType transactionType, string transactionNo) {
      //Entry for Credit amount to Invoice Generator's Account  from virtual currency balance

      var debitor = Manager<UserManager>.Instance.GetUserByName(debitorName);
      var accountDebitorEntry = new Account {
        Amount = amount,
        Owner = debitor,
        TransactionType = transactionType,
        CreatedAt = DateTime.Now,
        Description = "Virtual Currency Transfer",
        TransactionNo = transactionNo

      };

      Ctx.Accounts.Add(accountDebitorEntry);
      SaveChanges();

      //Entry for debit amount from virtual currency balance of Creditor's Account
      var creditor = Manager<UserManager>.Instance.GetUserByName(creditorName);
      var accountCreditorEntry = new Account {
        Amount = ((-1) * amount),
        Owner = creditor,
        TransactionType = transactionType,
        CreatedAt = DateTime.Now,
        Description = "Virtual Currency Transfer",
        TransactionNo = transactionNo

      };

      Ctx.Accounts.Add(accountCreditorEntry);
      SaveChanges();

      return true;
    }
    /// <summary>
    /// Update status of Invoice (Paid/Unpaid)
    /// </summary>
    public bool UpdateInvoiceStatus(Invoice inv, int invoiceId, string userName) {
      try {
        var o = GetInvoice(invoiceId, userName);

        inv.CopyProperties<Invoice>(o,
          m => m.BillingDate,
          m => m.CreatedAt,
          m => m.Creditor,
          m => m.Debitor,
          m => m.Deleted,
          m => m.DueDays,
          m => m.Paid,
          m => m.Positions,
          m => m.TaxPercentage,
          m => m.ModifiedAt);

        o.Paid = true;
        SaveChanges();
      } catch (DbEntityValidationException e) {
        foreach (var eve in e.EntityValidationErrors) {
          Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
              eve.Entry.Entity.GetType().Name, eve.Entry.State);
          foreach (var ve in eve.ValidationErrors) {
            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                ve.PropertyName, ve.ErrorMessage);
          }
        }
        throw;
      }
      return true;
    }

    /// <summary>
    /// Creates a detached and prefilled <see cref="Invoice"/> object.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userName"> </param>
    /// <returns></returns>
    public object CreateInvoice(int id, string userName) {
      var op = ProjectManager.Instance.GetOpus(id, userName);
      var tl = ProjectManager.Instance.GetTeamMembersOfOpus(op)
        .First(tm => tm.TeamLead)
        .Member;
      var inv = new Invoice {
        Debitor = Manager<UserManager>.Instance.GetUserByName(userName),
        Creditor = tl,
        DueDays = 30,
        WithTax = false,
        TaxPercentage = 0M,
        Paid = false,
        BillingDate = DateTime.Now,
        Positions = new List<InvoicePosition>(new InvoicePosition[] { new InvoicePosition { Position = 0, Amount = 50M, Text = "Finishing Milestone" } }),
        Text = String.Format("New Invoice for book project '{0}'", op.Name)
      };
      return inv;
    }

    /// <summary>
    /// Adds a detached Invoice object to the database.
    /// </summary>
    /// <param name="newInv"></param>
    /// <param name="sendMail"></param>
    public void CreateInvoice(Invoice newInv, bool sendMail) {
      Ctx.Invoices.Add(newInv);
      var r = SaveChanges();
      if (r > 0 && sendMail) {
        var msg = new Message {
          Body = "You got a new invoice from your contributor",
          Subject = "You got a new invoice from your contributor",
          Sender = newInv.Debitor,
          Receiver = newInv.Creditor
        };
        Manager<UserManager>.Instance.AddMessage(new List<int>(new int[] { newInv.Creditor.Id }), msg, newInv.Debitor.UserName);
      }
    }

    public bool DeleteInvoice(int invoiceId, string userName) {
      var inv = Ctx.Invoices.FirstOrDefault(i => i.Id == invoiceId && i.Debitor.UserName == userName);
      if (inv != null) {
        inv.Deleted = true;
        SaveChanges();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Get invoice and check where user is either creditor or debitor.
    /// </summary>
    /// <param name="invoiceId"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public Invoice GetInvoice(int invoiceId, string userName) {
      return Ctx.Invoices.FirstOrDefault(i => i.Id == invoiceId && (i.Creditor.UserName == userName || i.Debitor.UserName == userName));
    }


    # endregion

    # region --== Share handling ==--


    public void HandleRevenueShare(int orderProductId) {
      using (var scope = Ctx.BeginTransaction()){
        var product = Ctx.OrderProducts.Find(orderProductId);
        // get the real revenues
        // Get the work this product is derived from
        var work = product.Work;
        // Get the working fragments
        var wf = work.Fragments.Traverse(w => w.FrozenFragment.Published != null);
        // pull all team data from all fragments' frozen fragment counterparts
        var published = wf.Select(w => w.FrozenFragment.Published).Distinct();
        // gross revenues
        var price = product.PriceBase;
        // split shares
        var registerSales = new List<Sale>();
        var salesDate = DateTime.Now;
        foreach (var publ in published){
          var team = publ.SourceOpus.Project.Team;
          Ctx.LoadProperty(publ, p => p.ContributorRatios);
          var cr = publ.ContributorRatios;
          var product1 = product;
          if (cr.Count() != team.Members.Count()){
            // if we come here the team has no ratios, hence we divide equally
            var memberCount = team.Members.Count();
            var amnt = Math.Round(price/memberCount, 2);
            var acc = team.Members.Select(m => new Account{
              Owner = m.Member,
              Description =
                String.Format(ControllerResources.AccountingManager_HandleRevenueShare_Your_share_amount_for__0_,
                              product1.Name),
              Amount = amnt,
              TransactionType = TransactionType.Sale
            }).ToList();
            acc.ForEach(a => Ctx.Accounts.Add(a));
            acc.ForEach(a => Ctx.Sales.Add(new Sale{
              Day = salesDate,
              OrderCount = !product.Subscription,
              UserRevenues = a.Amount,
              TotalRevenues = price,
              OrderProduct = product,
              DownloadCount = false,
              ReaderCount = false,
              SubscriptionCount = product.Subscription,
              SourceOpus = GetSourceOpus(product),
              Owner = a.Owner
            }));
          }
          else{
            // there is some explicit ratio
            foreach (var acc in from member in team.Members
                                let memberCr = cr.First(c => c.Contributor.UserName == member.Member.UserName)
                                where memberCr.ShareType == ShareType.Ratio
                                let userShare = price*(memberCr.ValueOrRatio/100)
                                let user = member.Member
                                select new Account{
                                  Owner = user,
                                  Description =
                                    String.Format(
                                      ControllerResources.AccountingManager_HandleRevenueShare_Your_share_amount_for__0_,
                                      product1.Name),
                                  Amount = userShare,
                                  TransactionType = TransactionType.Sale
                                }){
              // payment to author
              Ctx.Accounts.Add(acc);
              // sales statistics for authors
              Ctx.Sales.Add(new Sale{
                Day = salesDate,
                OrderCount = !product.Subscription,
                UserRevenues = acc.Amount,
                TotalRevenues = price,
                OrderProduct = product,
                DownloadCount = false,
                ReaderCount = false,
                SubscriptionCount = product.Subscription,
                SourceOpus = GetSourceOpus(product),
                Owner = acc.Owner
              });
            }
          }
        }
        SaveChanges();
        scope.Commit();
      }
    }

    private Opus GetSourceOpus(DataModels.Models.Reader.Orders.OrderProduct product) {
      throw new NotImplementedException();
    }


    # endregion

    public IEnumerable<Sales> GetTotalSales(Revenues.DateFilter filter, bool onlyLeading, string userName) {
      List<int> opuses = null;
      if (onlyLeading) {
        var leaderRoles = new ContributorRole[] { ContributorRole.Author };
        var projects = ProjectManager.Instance.GetProjectForRoles(leaderRoles, userName).ToList();
        opuses = projects.SelectMany(p => p.Opuses).Select(o => o.Id).ToList();
      }
      else {
        opuses = ProjectManager.Instance.GetProjectsWhereUserIsMember(userName).SelectMany(p => p.Opuses).Select(o => o.Id).ToList();
      }
      var query = from s in Ctx.Sales
                  join o in Ctx.Opuses on s.SourceOpus equals o into r
                  where s.Owner.UserName == userName
                  select s;
      var sales = query
        .ToList()
        .Select(s => new Sales {
          Day = s.Day,
          AuthorRevenues = s.UserRevenues,
          ProjectRevenues = s.TotalRevenues,
          ReaderCount = s.ReaderCount ? 1 : 0,
          SubscriptionCount = s.SubscriptionCount ? 1 : 0,
          PrintCount = s.OrderCount ?  1 : 0,
          DownloadCount = s.DownloadCount ? 1 : 0,
          ProjectName = s.SourceOpus.Name,
        });
      Func<Sales, bool> pFilter = s => true;
      switch (filter) {
        case Revenues.DateFilter.All:
          break;
        case Revenues.DateFilter.ToDay:
          pFilter = s => s.Day <= DateTime.Today.Subtract(TimeSpan.FromDays(7));
          break;
        case Revenues.DateFilter.LastWeek:
          pFilter = s => s.Day <= DateTime.Today.Subtract(TimeSpan.FromDays(14));
          break;
        case Revenues.DateFilter.LastMonth:
          pFilter = s => s.Day <= DateTime.Today.Subtract(TimeSpan.FromDays(60));
          break;
        case Revenues.DateFilter.LastYear:
          pFilter = s => s.Day.Year == 2011;
          break;
      }
      var aggregate = sales
        .Where(pFilter)
        .GroupBy(s => s.ProjectName)
        .Select(g => new Sales {
          AuthorRevenues = g.Sum(r => r.AuthorRevenues),
          ProjectRevenues = g.Sum(r => r.ProjectRevenues),
          PrintCount = g.Sum(r => r.PrintCount),
          SubscriptionCount = g.Sum(r => r.SubscriptionCount),
          ReaderCount = g.Sum(r => r.ReaderCount),
          ProjectName = g.Max(r => r.ProjectName)
        });
      return aggregate;
    }
  }
}
