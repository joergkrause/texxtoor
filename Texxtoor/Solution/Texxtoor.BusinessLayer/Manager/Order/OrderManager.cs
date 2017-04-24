using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.Utilities.Storage;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Exceptions;
using Texxtoor.DataModels;
using Texxtoor.DataModels.Models.Common;
using Texxtoor.DataModels.Models.Content;
using Texxtoor.DataModels.Models.Reader.Content;
using Texxtoor.DataModels.Models.Reader.Orders;
using Texxtoor.DataModels.Models.Users;

namespace Texxtoor.BusinessLayer {

    /// <summary>
    /// A summary of functions related to the order process (users can order media), excluding payment.
    /// </summary>
    public class OrderManager : Manager<OrderManager> {

        /// <summary>
        /// Return product with Id for particular user.
        /// </summary>
        /// <param name="id">Product Id</param>
        /// <param name="userName">User's logon name</param>
        /// <returns>Returns product if exists or <c>null</c> if Id does not match.</returns>
        public Product GetProductForUser(int id, string userName) {
            var result = Ctx.Products
                      .Include(p => p.Owner)
                      .Include(p => p.Work)
                      .FirstOrDefault(p => p.Id == id && p.Owner.UserName == userName);
            return result;
        }

        /// <summary>
        /// Creates a new, detached <see cref="OrderProduct"/> for further processing.
        /// </summary>
        /// <param name="product">Product this order is derived from</param>
        /// <param name="userName">Assigned user</param>
        /// <returns>An <see cref="OrderProduct"/> instance.</returns>
        public OrderProduct CreateOrderProduct(Product product, string userName) {
            var op = new OrderProduct();
            using (var scope = Ctx.BeginTransaction()) {
                CreateOrderProductInternal(op, product, userName);
                // associate this product with the new orderproduct
                op.SourceProduct = product;
                scope.Commit();
            }
            return op;
        }

        public OrderProduct CreateOrderProductScopeless(Product product, string userName) {
            var op = new OrderProduct();
            CreateOrderProductInternal(op, product, userName);
            return op;
        }

        private void CreateOrderProductInternal(OrderProduct op, Product product, string userName) {
            // scope product and address management
            using (var scope = Ctx.BeginTransaction()) {

                var web = Ctx.OrderMedias.FirstOrDefault(m => m.Name == "Web" && m.Available);
                op.Media = new List<OrderMedia>(new[] { web });
                var user = Ctx.Users
                  .Include(u => u.Profile.Addresses)
                  .Single(u => u.UserName == userName);
                op.Owner = user;

                // orders are based on specific product
                product.CopyProperties<Product>(op,
                  o => o.Colored,
                  o => o.Content,
                  o => o.Dedication,
                  o => o.Issue,
                  o => o.Name,
                  o => o.Proprietor,
                  o => o.SubTitle,
                  o => o.Title,
                  o => o.Owner,
                  o => o.Work);
                op.Store = new OrderStore {
                    FullFillment = FullFillmentState.Created,
                    Name = String.Format(ControllerResources.OrderManager_CreateOrderProductInternal_Order_for__0_, op.Name)
                };

                var sa = new OrderShippingAddress();
                var ba = new OrderInvoiceAddress();
                var userAddresses = user.Profile.Addresses.OfType<AddressBook>().ToList().Where(a => a.GetType() == typeof(AddressBook));
                var hasDefault = user.Profile.Addresses.Any(a => a.Default);
                var hasInvoice = user.Profile.Addresses.Any(a => a.Invoice);
                // we assume that there is only one/no Default and one/no Invoice
                var defaultAddress = hasDefault ? userAddresses.Single(a => a.Default) : userAddresses.FirstOrDefault(a => a.GetType() == typeof(AddressBook));
                var invoiceAddress = hasInvoice ? userAddresses.Single(a => a.Invoice) : userAddresses.FirstOrDefault(a => a.GetType() == typeof(AddressBook));
                if (defaultAddress != null) {
                    defaultAddress.CopyProperties<AddressBook>(sa,
                      d => d.City,
                      d => d.Country,
                      d => d.Name,
                      d => d.Region,
                      d => d.StreetNumber,
                      d => d.Zip
                      );
                    sa.OrderProduct = op;
                    sa.Profile = user.Profile;
                    sa.Default = true;
                    Ctx.AddressBook.Add(sa);
                    op.ShippingAddress = sa;
                } else {
                    throw new NoAddressException() { UserId = user.Id, EntityId = op.Id, EntityName = "OrderProduct" };
                }
                // prepare billing address
                if (invoiceAddress != null) {
                    defaultAddress.CopyProperties<AddressBook>(ba,
                      d => d.City,
                      d => d.Country,
                      d => d.Name,
                      d => d.Region,
                      d => d.StreetNumber,
                      d => d.Zip
                      );
                    ba.OrderProduct = op; // assignment makes the address invisible for user and prevents further changes
                    ba.Profile = user.Profile;
                    ba.Invoice = true;
                    op.BillingAddress = ba;
                    Ctx.AddressBook.Add(ba);
                } else {
                    throw new NoAddressException() { UserId = user.Id, EntityId = op.Id, EntityName = "OrderProduct" };
                }
                Ctx.OrderProducts.Add(op);
                SaveChanges();
                scope.Commit();
            }
        }

        /// <summary>
        /// Get a specific <see cref="OrderProduct"/>. Includes Media and Store.
        /// </summary>
        /// <param name="orderProductId">Id to retrieve</param>
        /// <param name="properties"></param>
        /// <returns>The instance or <c>null</c> if the Id couldn't be found.</returns>
        public OrderProduct GetOrderProduct(int orderProductId, params Expression<Func<OrderProduct, object>>[] properties) {
            var op = Ctx.OrderProducts
              .Include(o => o.Media)
              .Include(o => o.ShippingAddress)
              .Include(o => o.BillingAddress)
              .FirstOrDefault(p => p.Id == orderProductId);
            if (op == null || properties == null)
                return op;
            foreach (var expression in properties) {
                Ctx.LoadProperty(op, expression);
            }
            return op;
        }

        /// <summary>
        /// Get a list of all medias and select the one currently set in the orderproduct.
        /// </summary>
        /// <param name="op">The current orderproduct</param>
        /// <returns></returns>
        public MultiSelectList GetMediaSelectList(OrderProduct op = null) {
            var medias = Ctx.OrderMedias.Where(m => m.Available).ToList();
            var ms = new MultiSelectList(medias, "Id", "Name", op == null ? Enumerable.Empty<int>() : op.Media.Select(m => m.Id).ToList());
            return ms;
        }

        public IEnumerable<OrderMedia> GetMedias(int[] ids) {
            var medias = Ctx.OrderMedias.Where(m => ids.Contains(m.Id) && m.Available);
            return medias;
        }

        public IEnumerable<OrderMedia> GetMedias() {
            return Ctx.OrderMedias.ToList();
        }

        /// <summary>
        /// Set the media Id for the <see cref="OrderProduct"/> and save to database.
        /// </summary>
        /// <param name="orderProductId"></param>
        /// <param name="ids">The currently selected media Ids</param>
        /// <param name="userName"></param>
        public IEnumerable<OrderMedia> SetMediaForOrderProduct(int orderProductId, int[] ids, string userName) {
            var op = Ctx.OrderProducts
              .Include(o => o.Media)
              .FirstOrDefault(o => o.Id == orderProductId && o.Owner.UserName == userName);
            op.Media.Clear();
            if (ids != null) {
                foreach (var media in ids.Select(t => Ctx.OrderMedias.Find(t))) {
                    op.Media.Add(media);
                }
            }
            SaveChanges();
            return op.Media;
        }

        /// <summary>
        /// Return previous orders with specified state.
        /// </summary>
        /// <remarks>
        /// Orders are finally stored in OrderStore, with an <see cref="OrderProduct"/> attached.
        /// </remarks>
        /// <param name="userName">Username for which to retrieve orders.</param>
        /// <param name="fullfillmentState"> </param>
        /// <returns>List of <see cref="OrderStore"/> objects. List may be empty.</returns>
        public IQueryable<OrderProduct> GetOrders(string userName, FullFillmentState fullfillmentState = FullFillmentState.NoneOrAll) {
            var result = Ctx.OrderProducts
              .Where(o => o.Owner.UserName == userName);
            if (fullfillmentState != FullFillmentState.NoneOrAll) {
                result = result.Where(o => o.Store.FullFillment == fullfillmentState);
            }
            return result.OrderByDescending(p => p.CreatedAt);
        }

        /// <summary>
        /// Return all products of the specified user.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IQueryable<Product> GetProductsForUser(string userName) {
            var result = Ctx.Products
              .Include(o => o.Work)
              .Where(p => p.Owner.UserName == userName)
              .OrderByDescending(p => p.CreatedAt);
            return result;
        }

        /// <summary>
        /// Returns the ordered products for user, with or with out fullfillment data.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="stored"></param>
        /// <returns></returns>
        public IQueryable<OrderProduct> GetOrderProductsForUser(string userName, bool stored) {
            var ops = Ctx.OrderProducts
              .Where(p => p.Owner.UserName == userName)
              .OrderByDescending(p => p.CreatedAt)
              .ThenBy(p => p.Name);
            return ops;
        }

        /// <summary>
        /// Calculate the final pricing based on rules and marketing package. Does not cover production cost.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public decimal CalculateProductPrice(int orderProductId) {
            var op = Ctx.OrderProducts
              .Include(o => o.Work)
              .Include(o => o.Work.Fragments)
              .Include(o => o.Work.Fragments.Select(f => f.FrozenFragment))
              .Include(o => o.Work.Published)
              .Include(o => o.Work.Published.Marketing)
              .FirstOrDefault(p => p.Id == orderProductId);
            switch (op.Work.Extern) {
                case WorkType.External:
                    // custom upload with no production relation, always zero
                    return 0M;
                case WorkType.Custom:
                    // multiple chapters from several published text associated through fragments
                    var published = op.Work.Fragments.Select(f => f.FrozenFragment.Published).ToList();
                    return published.Sum(publ => publ.Marketing == null ? 0 : publ.Marketing.BasePrice / publ.FrozenFragments.Count());
                case WorkType.Published:
                    // one published text directly associated
                    return op.Work.Published.Marketing.BasePrice;
            }
            return 0;
        }

        /// <summary>
        /// Remove a currently unfullfilled product from list of orders. Once bound to an order store, it cannot be removed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Name of removed product or <c>null</c> if not found or already processed.</returns>
        public string DeleteOrderProduct(int orderProductId) {
            var op = Ctx.OrderProducts.FirstOrDefault(p => p.Store == null && p.Id == orderProductId);
            if (op != null) {
                var result = op.Name;
                Ctx.OrderProducts.Remove(op);
                SaveChanges();
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// After all steps are down through order process we make the product and related data persistent in the store.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public OrderStore CreateOrderStore(OrderProduct op, string userName, dynamic fullFillmentState) {
            var user = Ctx.Users.First(u => u.UserName == userName);
            // get the address base
            Ctx.LoadProperty(user.Profile, "Addresses");
            var addresses = user.Profile.Addresses;
            // make permanent 
            var billingAddress = addresses.FirstOrDefault(a => a.Invoice == true);
            var shippingAddress = addresses.FirstOrDefault(a => a.Invoice == false);
            if (shippingAddress == null && billingAddress != null) {
                shippingAddress = billingAddress;
            } else {
                if (billingAddress == null && shippingAddress != null) {
                    billingAddress = shippingAddress;
                }
            }
            var permbillAddress = new OrderInvoiceAddress {
                Invoice = true,
                Default = true
            };
            billingAddress.CopyProperties<AddressBook>(permbillAddress,
              a => a.City,
              a => a.Country,
              a => a.Region,
              a => a.StreetNumber,
              a => a.Zip,
              a => a.Profile,
              a => a.Name);
            Ctx.OrderInvoiceAddresses.Add(permbillAddress);
            var permshipAddress = new OrderShippingAddress {
                Invoice = false,
                Default = true
            };
            shippingAddress.CopyProperties<AddressBook>(permshipAddress,
              a => a.City,
              a => a.Country,
              a => a.Region,
              a => a.StreetNumber,
              a => a.Zip,
              a => a.Profile,
              a => a.Name);
            Ctx.OrderShippingAddresses.Add(permshipAddress);
            op.Store.FullFillment = FullFillmentState.Created;
            op.Store.TransactionBag = new JavaScriptSerializer().Serialize(fullFillmentState);
            op.Owner = user;
            SaveChanges();
            return op.Store;
        }

        /// <summary>
        /// Returns for a list of works the related orderproduct entries, if one exists.
        /// </summary>
        /// <param name="works"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IDictionary<Work, OrderProduct> GetOrdersForWorks(IEnumerable<Work> works, string userName) {
            var result = new Dictionary<Work, OrderProduct>();
            // user's orders
            var ops = Ctx.OrderProducts.Where(op => op.Owner.UserName == userName).ToList();
            var worksList = works.ToList();
            foreach (var order in ops) {
                // found a work from list in the orders
                var hasWork = worksList.FirstOrDefault(w => w.Id == order.Work.Id);
                if (hasWork != null && !result.ContainsKey(order.Work)) {
                    result.Add(hasWork, order);
                }
            }
            return result;
        }


        public void SetSubscriptionForOrder(int orderProductId, string userName, bool subscribe,
          DateTime? startSubscription = null, DateTime? endSubscription = null) {
            var op = Ctx.OrderProducts.FirstOrDefault(o => o.Id == orderProductId && o.Owner.UserName == userName);
            if (op == null)
                return;
            if (op.Subscription && startSubscription.HasValue && endSubscription.HasValue) {
                op.Store.BeginSubscription = startSubscription.Value;
                op.Store.EndSubscription = endSubscription.Value;
            } else {
                op.Subscription = false;
            }
            var medias = String.Join(", ", op.Media.Select(m => m.Name).ToArray());
            op.Name = String.Format("{0} as {3} on media {1} for user '{2}'",
              op.Work.Name,
              medias,
              userName,
              subscribe ? "Subscription" : "Regular Order");
            SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public void SendTransactionMail(string userName) {
            var user = Ctx.Users.Single(u => u.UserName == userName);
            Ctx.Messages.Add(new Message {
                Sender = null,    // Null is the system itself
                Receiver = user,
                Subject = String.Format("Your Transaction Details"),
                Body = String.Format("Thank you, {0} for Purchasing the Product Your Transaction are Below.", user.UserName)
            });
            SaveChanges();
        }

        /// <summary>
        /// Return the currently assigned private files. Mostly based on orders.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IQueryable<UserFile> GetUserFiles(string userName) {
            var ufs = Ctx.UserFiles
              .Where(u => u.Owner.UserName == userName && u.Deleted == false)
              .OrderByDescending(u => u.CreatedAt);
            return ufs;
        }

        public IQueryable<UserFile> GetRecycleBin(string userName) {
            var ufs = Ctx.UserFiles
              .Where(u => u.Owner.UserName == userName && u.Deleted == true)
              .OrderByDescending(u => u.CreatedAt);
            return ufs;
        }

        public void DeleteUserFilePermanently(int userFileId, string userName) {
            var uf = Ctx.UserFiles
              .FirstOrDefault(u => u.Owner.UserName == userName && u.Id == userFileId);
            if (uf == null)
                return;
            var id = uf.ResourceId;
            Ctx.UserFiles.Remove(uf);
            if (SaveChanges() != 1)
                return;
            using (var blob = BlobFactory.GetBlobStorage(id, BlobFactory.Container.UserFolder)) {
                blob.Remove();
            }
        }

        public void DeleteUserFile(int userFileId, string userName) {
            var uf = Ctx.UserFiles
              .FirstOrDefault(u => u.Owner.UserName == userName && u.Id == userFileId);
            if (uf == null)
                return;
            uf.Deleted = true;
            uf.Owner = Ctx.Users.Single(u => u.UserName == userName);
            SaveChanges();
        }

        public UserFile GetUserFile(int id, string userName) {
            return Ctx.UserFiles.FirstOrDefault(f => f.Id == id && f.Owner.UserName == userName);
        }

        public void RecoverUserFile(int userFileId, string userName) {
            var uf = Ctx.UserFiles
              .FirstOrDefault(u => u.Owner.UserName == userName && u.Id == userFileId);
            if (uf == null)
                return;
            uf.Deleted = false;
            SaveChanges();
        }

        public decimal SetFinalPrice(int orderProductId, decimal basePrice, decimal productionCost, decimal countryAndRegionAdjustment, string userName) {
            var op = Ctx.OrderProducts
              .Include(o => o.Media)
              .Include(o => o.ShippingAddress)
              .Include(o => o.BillingAddress)
              .Single(o => o.Id == orderProductId && o.Owner.UserName == userName);
            op.PriceBase = basePrice;
            op.RuleDistance = productionCost;
            op.RuleFactor = countryAndRegionAdjustment;
            SaveChanges();
            return op.RealPrice;
        }

        public OrderProduct GetOrderProductForProduction(int orderProductId) {
            var op = Ctx.OrderProducts
              .Include(o => o.Media)
              .Include(o => o.ShippingAddress)
              .Include(o => o.BillingAddress)
              .Include(o => o.Work)
              .Include(o => o.Work.Published)
              .Include(o => o.Work.Published.SourceOpus)
              .Single(p => p.Id == orderProductId);
            return op;
        }

        public void SetShippingAddressFromPayment(int orderProductId, OrderShippingAddress shippingAddress) {
            var op = Ctx.OrderProducts.Find(orderProductId);
            if (op != null) {
                shippingAddress.CopyProperties<AddressBook>(op.ShippingAddress,
                  s => s.Name,
                  s => s.City,
                  s => s.StreetNumber,
                  s => s.Country
                  );
            }
            SaveChanges();
        }

        public void CalculateProductionCost(IEnumerable<OrderMedia> media, XDocument costDoc, bool colored, decimal basePrice, out decimal price, out decimal subscription, uint amount = 1) {
            if (costDoc == null) {
                throw new ArgumentNullException("costDoc");
            }
            if (!media.Any()) {
                price = 0;
                subscription = 0;
                return;
            }
            // each media has a distinct production price, which is added to the author selling price
            var prodCost = costDoc.Root
                    .Elements("type").Where(e => media.Any(m => m.Name == e.Attribute("name").Value))
                    .Sum(e =>
                      // hard factors
                      ((e.Element("base").NullSafeDecimal() / 100) + basePrice + (e.Element("cover").NullSafeDecimal() / 100))
                      // add value for colored if that matters
                      * (colored ? e.Element("factorcolor").NullSafeDecimal() : 1)
                      // multiple amount if that matters in production (print only, usually)
                      * (e.Element("factorvolume").NullSafeBool().GetValueOrDefault() ? amount : 1)
                      // distributor costs, such as app store margins
                      * (e.Element("distributor").NullSafeDecimal()));
            var subDisCost = costDoc.Root
                    .Elements("type").Where(e => media.Any(m => m.Name == e.Attribute("name").Value))
                    .Average(e => (e.Element("subscriptiondiscount").NullSafeDecimal()));
            price = prodCost;
            subscription = prodCost * subDisCost;
        }


        public void SetFullfillmentState(int id, FullFillmentState fullFillmentState) {
            var op = GetOrderProduct(id);
            op.Store.FullFillment &= fullFillmentState;
            SaveChanges();
        }
    }
}
