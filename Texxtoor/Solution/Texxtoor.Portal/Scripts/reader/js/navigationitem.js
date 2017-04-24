//-----------------Navigation Item
function NavigationItem(jsonNavigationItem, book, parent, itemsSize) {
  var $this = this;
  var content = jsonNavigationItem.Content;
  if (content.indexOf('#') >= 0) {
    this.hrefHash = content.substring(content.indexOf('#'), content.length);
    content = content.substring(0, content.indexOf('#'));
    this.size = 0;
  }
  else {
    this.size = itemsSize[content];
    this.hrefHash = '';
  }
  if (!this.size)
    this.size = 0;
  if (jsonNavigationItem.Children) {
    this.parse(jsonNavigationItem.Children, book, this, itemsSize);
  }

  this.parent = parent;
  this.book = book;
  this.labelText = jsonNavigationItem.LabelText;
  this.playOrder = jsonNavigationItem.PlayOrder;
  this.metaId = jsonNavigationItem.MetaId;
  this.fragmentHref = content;
}

NavigationItem.prototype = {
  book: null,
  labelText: null,
  playOrder: null,
  metaId: null,
  fragmentHref: null,
  hrefHash: null,
  fragment: null,
  parent: null,
  children: null,
  size: null,
  parse: function (items, book, parent, itemsSize) {
    this.children = [];
    this.parent = parent;
    for (var i = 0; i < items.length; ++i) {
      this.children.push(new NavigationItem(items[i], book, this, itemsSize));
    }
  },
  getFragment: function (success) {
    if (this.fragment) {
      if ($.isFunction(success)) {
        success.call(this, this.fragment.clone().data('NavigationItem', this));
        return;
      }
    }
    var loader = new Loader();
    var $this = this;
    $.ajax({
      type: 'GET',
      cache: false,
      data: { ssid: $this.book.ssid, bookId: $this.book.bookId, fragmentHref: $this.fragmentHref },
      url: $this.book.reader.readerServiceUrl.getBookFragment,
      dataType: 'json',
      success: function (data) {
        if (data) {
          var dataStr = data;
          dataStr = dataStr.replace(/<link[^\/>]*\/?>/i, '');
          dataStr = dataStr.replace(/src=/gi, 'temp$&');
          var html = $(dataStr);
          if (html) {
            var fragment = html.appendTo('<div class="book-fragment"></div>').closest('.book-fragment');
            fragment.find('link').remove();
            var imgCount = 0;
            $('img', fragment).each(function () {
              imgCount++;
              var img = $(this);
              var funcComplete = function () {
                if (img[0].complete) {
                  imgCount--;
                } else {
                  setTimeout(funcComplete, 100);
                }
              };
              img.attr('src', $this.book.reader.readerServiceUrl.getBookResource + "?bookId=" + $this.book.bookId + "&ssid=" + $this.book.ssid + "&fragmentHref=" + $(this).attr('tempsrc'));
              setTimeout(funcComplete, 100);
            });
            $this.fragment = fragment;
            var funcAllImagesLoaded = function () {
              if (imgCount == 0) {
                loader.hide();
                if ($.isFunction(success)) {
                  success.call($this, fragment.clone().data('NavigationItem', $this));
                }
              } else {
                setTimeout(funcAllImagesLoaded, 100);
              }
            };
            setTimeout(funcAllImagesLoaded, 100);
          }
        }
      },
      beforeSend: loader.show
    });
  }
};
