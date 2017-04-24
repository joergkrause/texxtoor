function _SidebarWidget() {
  this.snippetObj;
}

_SidebarWidget.prototype = {
  getWidgetHtml: function () {
    var id = this.snippetObj.snippetId;
    return '' +
        '<div class="integratedEditor saveableByTexxtoor" ' +
        'data-editor="SidebarEditor" data-class="editableByTexxtoor" data-style="background-color:#fff;" data-item="' + id + '" id="' + id + '">' +
        '<div class="editor" data-type="' + this.snippetObj.sidebarType + '">' +
        '<header class="canMoveCaret" contenteditable="' + this.snippetObj.isEditableContent + '">' +
        '' + this.snippetObj.headerContent + '</header>' +
        '<aside class="canMoveCaret editableByTexxtoor editableText" data-item="' + id + '" data-content-editable="' + !(this.snippetObj.isReadOnly) + '">' + this.snippetObj.asideContent + '' +
        '</aside></div><input name="sidebartype" type="hidden" value="' + this.snippetObj.sidebarType + '" data-item="' + id + '" /></div>';
  }
};