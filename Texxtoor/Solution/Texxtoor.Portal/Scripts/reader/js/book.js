//-----------------Book (Work)
function Book(jsonBook, reader) {
  var itemsSize = [];
  for (var i = 0; i < jsonBook.ItemHref.length; ++i) {
    itemsSize[jsonBook.ItemHref[i]] = jsonBook.ItemSize[i];
  }
  this.reader = reader;
  this.ssid = reader.ssid;
  this.bookId = jsonBook.BookId;
  this.title = jsonBook.Title;
  this.authors = jsonBook.Authors;
  this.css = [];
  this.bookmarks = [];
  this.comments = [];
  this.navigation = new Navigation(jsonBook.Navigation, this, itemsSize);
  this.loadCss(jsonBook.ItemHref);
}

Book.prototype = {
  reader: null,
  ssid: null,
  bookId: null,
  title: null,
  authors: null,
  navigation: null,
  bookmarks: null,
  parentCommentsCache: null,
  css: null,
  comments: null,
  loadCss: function (itemsHref) {
    var $this = this;
    var cssItemsHref = itemsHref.where(function (x) { return x.search(/\.css\b/g) > -1 ? true : false; });
    var intervalId = null;
    cssItemsHref.each(function (x) {
      $.ajax({
        type: 'GET',
        cache: false,
        data: { ssid: $this.ssid, bookId: $this.bookId, fragmentHref: x },
        url: $this.reader.readerServiceUrl.getBookFragment,
        dataType: 'json',
        success: function (data) {
          $this.css.push(data);
        },
        error: function () {
          clearInterval(intervalId);
        }
      });
    });
    intervalId = setInterval(function () {
      if ($this.css.length == cssItemsHref.length) {
        clearInterval(intervalId);
        if ($.isFunction($this.onLoadCss))
          $this.onLoadCss($this.css);
      }
    }, 200);
  },
  getParentComments: function (success) {
    var $this = this;
    $.ajax({
      type: 'GET',
      cache: false,
      data: { ssid: $this.ssid, bookId: $this.bookId, withContent: false },
      url: $this.reader.readerServiceUrl.getParentComments,
      dataType: 'json',
      success: function (data) {
        $this.parentCommentsCache = data;
        if (data && $.isFunction(success)) {
          success.call($this, data);
        }
      }
    });
  },
  getComments: function (navItemId, success) {
      var $this = this;
      $.ajax({
      type: 'GET',
      cache: false,
      dataType: 'json',
      contentType: 'application/json',
      data: {
        ssid: $this.ssid,
        bookId: $this.bookId,
        fragmentHref: navItemId
      },
      url: $this.reader.readerServiceUrl.getComments,
      success: function (data) {
        if (!data) data = [];
        data = data.where(function (item) {
          return !(item.Parent);
        });
        var mapComments = function (comments) {
          var that = this;
          return comments.each(function (item) {
            var comment = new Comment();
            comment.Id = item.Id;
            comment.ParentId = item.Parent;
            comment.Content = item.Content;
            comment.Subject = item.Subject;
            comment.FragmentCfi = item.FragmentCfi;
            comment.NavigationItem = navItemId;
            comment.Owner = item.Owner;
            if (item.Children)
              comment.Children = mapComments.call(that, item.Children);
            return comment;
          });
        };
        if ($.isFunction(success)) {
          success.call($this, mapComments.call($this, data));
        }
      }
    });
  },
  addComment: function (comment, success) {
    var $this = this;
    $.ajax({
      type: 'POST',
      cache: false,
      contentType: 'application/json',
      data: JSON.stringify(comment),
      url: $this.reader.readerServiceUrl.addComment + '?ssid=' + $this.ssid,
      success: function (data) {
        if ($.isFunction(success))
          success.call($this);
      }
    });
  },
  getBookmarks: function (success) {
    var $this = this;
    if (this.bookmarks && !this.bookmarks.isEmpty()) {
      if ($.isFunction(success))
        success.call(this, this.bookmarks);
      return;
    }    
    $.ajax({
      type: 'GET',
      cache: false,
      data: { ssid: $this.ssid, bookId: $this.bookId },
      contentType: 'application/json',
      dataType: 'json',
      url: $this.reader.readerServiceUrl.getBookmarks,
      success: function (bookmarks) {
        $this.bookmarks = [];
        if (bookmarks) {
          $this.parseBookmarks(bookmarks, $this.navigation.allNavigationItems);
          $this.bookmarks.orderBy('navigationItem.playOrder');
        }
        if ($.isFunction(success))
          success.call($this, $this.bookmarks);
      }
    });
  },
  addBookmark: function (bookmark) {
    var $this = this;
    var isCreated = $this.bookmarks.single(function (x) { return (x.navigationItem.playOrder == bookmark.navigationItem.playOrder && x.fragmentCfi == bookmark.fragmentCfi) });
    if (isCreated) return;
    $this.bookmarks.push(bookmark);
    $this.bookmarks = $this.bookmarks.orderBy('navigationItem.playOrder');
    $.ajax({
      type: 'POST',
      cache: false,
      data: JSON.stringify({ BookId: $this.bookId, FragmentCfi: bookmark.fragmentCfi }),
      url: $this.reader.readerServiceUrl.commitBookmark + '?ssid=' + $this.ssid,
      dataType: 'json',
      contentType: 'application/json',
      success: function (data) {
        alert('The bookmark was added successfully');
      }
    });
  },
  deleteBookmark: function (bookmarkId, success) {
    var $this = this;
    this.bookmarks = this.bookmarks.where(function (item) { return item.id != bookmarkId });
    $.ajax({
      type: 'GET',
      cache: false,
      data: { ssid: $this.ssid, bookmarkId: bookmarkId },
      url: $this.reader.readerServiceUrl.deleteBookmark,
      dataType: 'json',
      success: function (data) {
        if ($.isFunction(success))
          success.call($this, data);
      }
    });
  },
  onLoadCss: null,
  parseBookmarks: function (bookmarks, navItems) {
    for (var i = 0; i < navItems.length; ++i) {
      var tmpBookmarks = bookmarks.where(function (x) {
        if (x.FragmentCfi)
          return x.FragmentCfi == navItems[i].fragmentHref + navItems[i].hrefHash ? true : false;
        return false;
      });
      for (var j = 0; j < tmpBookmarks.length; ++j) {
        this.bookmarks.push(new Bookmark(navItems[i], tmpBookmarks[j].FragmentCfi, tmpBookmarks[j].Id));
      }
    }
  }
};
