function _TableWidget() {
  this.snippetObj;
}

_TableWidget.prototype = {
  getWidgetHtml: function () {
    var isReadOnly = this.snippetObj.isReadOnly ? "readonly='readonly'" : "";
    var id = this.snippetObj.snippetId;
    this.snippetObj.title = !this.snippetObj.title ? "" : this.snippetObj.title;
    return '' +
        '<div class="editableByTexxtoor tableEditor integratedEditor saveableByTexxtoor canMoveCaret" data-editor="TableEditor" data-style="" id="sn-' + id + '" data-item="' + id + '">' +
        '<div class="editor" style="overflow-x: scroll" data-content-editable="' + !(this.snippetObj.isReadOnly) + '">' + this.snippetObj.content + '</div>' +
        '<div class="snippetTitle"><span>' + this.snippetObj.tableLocalization + ' ' + this.snippetObj.genericChapterNumber + '.' + '<span class="snippetCounter">' + this.snippetObj.snippetCounter + '</span></span>' +
        '<input type="text" name="caption" value="' + this.snippetObj.title + '"  data-item="' + id + '" ' + isReadOnly + ' class="saveableCaption canMoveCaret editableCaption"/>' +
        '</div></div>';
  }
};