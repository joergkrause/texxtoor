function _ListingWidget() {
  this.snippetObj;
}

_ListingWidget.prototype = {
  getWidgetHtml: function () {
    var isReadOnly = this.snippetObj.isReadOnly ? "readonly='readonly'" : "";
    var id = this.snippetObj.snippetId;
    this.snippetObj.title = !this.snippetObj.title ? "" : this.snippetObj.title;
    return '' +
        '<div class="editableByTexxtoor integratedEditor saveableByTexxtoor listingEditor" data-editor="ListingEditor" data-item="' + id + '" data-style="">' +
        '<textarea class="code canMoveCaret">' + this.snippetObj.content + '</textarea>' +
        '<div class="snippetTitle">' +
        '<span>' + this.snippetObj.listingLocalization + ' ' + this.snippetObj.genericChapterNumber + '.' + '<span class="snippetCounter">' + this.snippetObj.snippetCounter + '</span></span>' +
        '<input type="text" name="caption" value="' + this.snippetObj.title + '"  data-item="' + id + '" ' + isReadOnly + ' class="saveableCaption canMoveCaret editableCaption"/>' +
        '<input type="hidden" name="language" value="' + this.snippetObj.language + '"  data-item="' + id + '"/>' +
        '<input type="hidden" name="linenumbers" value="' + this.snippetObj.lineNumbers + '"  data-item="' + id + '"/>' +
        '<input type="hidden" name="syntaxhighlight" value="' + this.snippetObj.syntaxHighlight + '"  data-item="' + id + '"/>' +
        '</div></div>';
  }
};