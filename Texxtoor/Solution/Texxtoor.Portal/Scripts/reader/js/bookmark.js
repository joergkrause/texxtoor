//-----------------Bookmark
function Bookmark(navigationItem, fragmentCfi, id) {
  this.navigationItem = navigationItem;
  this.fragmentCfi = fragmentCfi;
  this.id = id;
}

Bookmark.prototype = {
  id: null,
  fragmentCfi: null,
  navigationItem: null
};
