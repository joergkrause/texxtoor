function _SectionWidget() {
    this.snippetObj;
}

_SectionWidget.prototype = {
    getWidgetHtml: function () {
        return ''+
            '<div><h' + (this.snippetObj.levelId - 1) + ' contenteditable="' + !(this.snippetObj.isReadOnly) + '" class="editableByTexxtoor editableSection integratedEditor saveableByTexxtoor canMoveCaret" data-editor="SectionEditor" data-item="' + this.snippetObj.snippetId + '">' + this.snippetObj.content + '</h' + (this.snippetObj.levelId - 1) + '><span class="editableNumberLevel' + (this.snippetObj.levelId - 1) + '" >' + this.snippetObj.genericChapterNumber + this.snippetObj.sectionNumberChain + '</span>' +
            '</div>';
    }
};