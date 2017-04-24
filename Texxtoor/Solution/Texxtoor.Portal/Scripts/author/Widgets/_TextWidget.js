function _TextWidget() {
    this.snippetObj;
}

_TextWidget.prototype = {
    getWidgetHtml: function () {
        return '' +
            '<div class="editableByTexxtoor editableText integratedEditor saveableByTexxtoor textMenu canMoveCaret" contenteditable="false" data-editor="HtmlEditor" data-class="editableByTexxtoor" data-item="' + this.snippetObj.snippetId + '" id="sn-' + this.snippetObj.snippetId + '"><div class="editor" data-content-editable="' + !(this.snippetObj.readonly) + '">' + this.snippetObj.content + '</div>' +
            '</div>';
    }
};