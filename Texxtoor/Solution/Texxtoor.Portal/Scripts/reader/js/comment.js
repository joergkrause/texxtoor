//-----------------Comment // BookComment on server side
function Comment() {
  this.Children = [];
}

Comment.prototype = {
  // comment id, can be empty on add
  Id: null,
  // book id
  BookId: 0,
  // parent comment (if it's an answer)
  ParentId: 0,
  // internal description, such as NOTE, COMM, color
  Subject: '',
  // comment's text
  Content: '',
  // only for reading, an array of Comment objects
  Children: [],
  // for position inside an item
  FragmentCfi: '',
  // the item the comment refers to
  NavigationItem: ''
};
