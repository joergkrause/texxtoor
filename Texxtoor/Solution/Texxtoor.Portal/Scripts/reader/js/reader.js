//-----------------Reader
function Reader(baseUrl, signOutUrl) {
  this.searchResults = [];
  this.readerServiceUrl = {
    signIn: baseUrl + 'SignIn',
    signOut: signOutUrl,
    getBookList: baseUrl + 'GetBookList',
    getBookFragment: baseUrl + 'GetBookFragment',
    commitBookmark: baseUrl + 'CommitBookmark',
    getBookmarks: baseUrl + 'GetBookmarks',
    deleteBookmark: baseUrl + 'DeleteBookmark',
    addComment: baseUrl + 'AddComment',
    getComments: baseUrl + 'GetComments',
    getWork: baseUrl + 'GetWork',
    getParentComments: baseUrl + 'GetParentComments',
    getBookResource: baseUrl + 'GetBookResource'
  }
}

Reader.prototype = {
  ssid: null,
  searchResults: null,
  readerServiceUrl: null,
  signIn: function (name, password, success) {
    var $this = this;
    $.ajax({
      type: 'GET',
      cache: false,
      data: { uname: name, password: password },
      url: $this.readerServiceUrl.signIn,
      dataType: 'json',
      success: function (data) {
        if (data) {
          $this.ssid = data;
          if ($.isFunction(success))
            success.call($this, data);
        }
      }
    });
  },
  signOut: function (success) {
    var $this = this;
    $.ajax({
      type: 'GET',
      cache: false,
      dataType: 'json',
      data: { ssid: $this.ssid },
      url: $this.readerServiceUrl.signOut,
      success: function (data) {
        if ($.isFunction(success))
          success.call($this, data);
      }
    });
  },
  signOff: function (success) {
    var $this = this;
    window.location = $this.readerServiceUrl.signOut;
  },
  // Get the complete works structure with all chapters and children references
  getWork: function (bookId, result) {
    var book = this.searchResults.single(function (x) { return x.bookId == bookId; });
    if (book && $.isFunction(result)) {
      result(book);
      return;
    }
    var $this = this;
    $.ajax({
      type: 'GET',
      cache: false,
      dataType: 'json',
      data: { ssid: $this.ssid, bookId: bookId },
      url: $this.readerServiceUrl.getWork,
      success: function (data) {
        if (data) {
          var book = new Book(data, $this)
          $this.searchResults.push(book);
          if ($.isFunction(result)) result(book); // init reader ui
        }
      }
    });
  },
};
