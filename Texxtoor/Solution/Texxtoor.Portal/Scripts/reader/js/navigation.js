// Class co//-----------------Navigation
function Navigation(jsonNavigation, book, itemsSize) {
  this.children = [];
  this.allNavigationItem = [];
  for (var i = 0; i < jsonNavigation.length; ++i) {
    this.children.push(new NavigationItem(jsonNavigation[i], book, this, itemsSize));
  }
  this.allNavigationItems = this.getAllNavigationItems();
}
Navigation.prototype = {
  children: null,
  allNavigationItems: null,
  nextItem: function (currentItem) {
    return this.allNavigationItems.where(function (item) {
      return item.playOrder > currentItem.playOrder && item.fragmentHref != currentItem.fragmentHref;
    }).first();
  },
  prevItem: function (currentItem) {
    return this.allNavigationItems.where(function (item) {
      return item.playOrder < currentItem.playOrder && item.fragmentHref != currentItem.fragmentHref;
    }).last();
  },
  getAllNavigationItems: function (item) {
    var $this = this;
    var result = [];
    if (item) {
      result.push(item);
      if (item.children) {
        item.children.each(function (child) {
          result.pushAll($this.getAllNavigationItems(child));
        });
      }
    } else {
      $this.children.each(function (child) {
        result.pushAll($this.getAllNavigationItems(child));
      });
    }
    return result.orderBy('playOrder');
  },
  getFragmentFromPlayOrder: function (playOrder) {
    return this.allNavigationItems.single(function (x) { return x.playOrder == playOrder });
  },
  getFragmentFromHref: function (fragmentHref) {
    return this.allNavigationItems.single(function (x) { return x.fragmentHref == fragmentHref });
  }
};
