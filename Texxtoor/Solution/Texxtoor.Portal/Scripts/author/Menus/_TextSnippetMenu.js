function _TextSnippetMenu() {
    this.snippetId;
    this.objUndoManager;
    this.objAuthor;
}

_TextSnippetMenu.prototype = {
    //flags: [bold = false, italic = false, underline = false, subscript= false, superscript = false, bullets = false, numbers = false, indent = true, outdent = true, undo = true, redo = true],
    flags: { bold: false, italic: false, underline: false, subscript: false, superscript: false, bullets: false, numbers: false, undo: true, redo: true, moveup: false, movedown: false },
    initializeMenu: function () {
        var $this = this;
        $this.setFlags();
        var textSnippetMenuItems = {
            "bold": { name: ($this.flags.bold) ? "<b>Bold</b>" : "Bold", disabled: $this.getSelectionHtml() == "", callback: function (key, options) { $(options.$trigger).htmlarea('bold'); }/*, icon: "edit"*/ },
            "italic": { name: ($this.flags.italic) ? "<b>Italic</b>" : "Italic", disabled: $this.getSelectionHtml() == "", callback: function (key, options) { $(options.$trigger).htmlarea('italic'); }/*, icon: "cut" */ },
            "underline": { name: ($this.flags.underline) ? "<b>Underline</b>" : "Underline", disabled: $this.getSelectionHtml() == "", callback: function (key, options) { $(options.$trigger).htmlarea('underline'); }/*, icon: "copy" */ },
            "sep1": "---------",
            "subscript": { name: ($this.flags.subscript) ? "<b><span>X<sub>2</sub></span></b>" : "<span>X<sub>2</sub></span>", disabled: $this.getSelectionHtml() == "", callback: function (key, options) { $(options.$trigger).htmlarea('subscript'); }/*, icon: "paste" */ },
            "superscript": { name: ($this.flags.superscript) ? "<b><span>X<sup>2</sup></span></b>" : "<span>X<sup>2</sup></span>", disabled: $this.getSelectionHtml() == "", callback: function (key, options) { $(options.$trigger).htmlarea('superscript'); }/*, icon: "delete" */ },
            "sep2": "---------",
            "undo": { name: "Undo", callback: function (key, options) { $this.objUndoManager.undo(); }/*, icon: "copy" */ },
            "redo": { name: "Redo", callback: function (key, options) { $this.objUndoManager.redo(); }/*,disabled:true, icon: "copy" */ },
            "sep3": "---------",
            "formatting": {
                "name": "Formatting",
                "items": {
                    "bullets": { name: "Bullets", callback: function (key, options) { $(options.$trigger).htmlarea('insertUnorderedList'); }/*, icon: "copy" */ },
                    "numbers": { name: "Numbers", callback: function (key, options) { $(options.$trigger).htmlarea('insertOrderedList'); }/*, icon: "copy" */ }
                    //"sep4": "---------",
                    //"indent": { name: "Indent", callback: function (key, options) { $(options.$trigger).htmlarea('indent'); }/*, icon: "copy" */ },
                    //"outdent": { name: "Outdent", callback: function (key, options) { $(options.$trigger).htmlarea('outdent'); }/*, icon: "copy" */ }
                }
            },
            "move": {
                "name": "Move",
                "items": {
                    "moveup": { name: "Move up", disabled: (!$this.flags.moveup), callback: function (key, options) { $this.objAuthor.move('u', $("#sn_block-" + this.snippetId).find(".up-down").find(".nav-up").data('item')); }/*, icon: "copy" */ },
                    "movedown": { name: "Move down", disabled: (!$this.flags.movedown), callback: function (key, options) { $this.objAuthor.move('d', $("#sn_block-" + this.snippetId).find(".up-down").find(".nav-down").data('item')); }/*, icon: "copy" */ }
                }
            }
        };
        return textSnippetMenuItems;
    },
    getSelectionHtml: function () {
        var html = "";
        if (typeof window.getSelection != "undefined") {
            var sel = window.getSelection();
            if (sel.rangeCount) {
                var container = document.createElement("div");
                for (var i = 0, len = sel.rangeCount; i < len; ++i) {
                    container.appendChild(sel.getRangeAt(i).cloneContents());
                }
                html = container.innerHTML;
            }
        } else if (typeof document.selection != "undefined") {
            if (document.selection.type == "Text") {
                html = document.selection.createRange().htmlText;
            }
        }
        return html;
    },
    setFlags: function () {
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'bold') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'bold').state ? this.flags.bold = true : this.flags.bold = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'italic') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'italic').state ? this.flags.italic = true : this.flags.italic = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'underline') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'underline').state ? this.flags.underline = true : this.flags.underline = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'subscript') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'subscript').state ? this.flags.subscript = true : this.flags.subscript = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'superscript') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'superscript').state ? this.flags.superscript = true : this.flags.superscript = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'insertUnorderedList') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'insertUnorderedList').state ? this.flags.bullets = true : this.flags.bullets = false;
        $("#sn_block-" + this.snippetId).htmlarea('qc', 'insertOrderedList') && $("#sn_block-" + this.snippetId).htmlarea('qc', 'insertOrderedList').state ? this.flags.numbers = true : this.flags.numbers = false;
        $("#sn_block-" + this.snippetId).find(".up-down").find(".nav-up").hasClass("naviActive") ? this.flags.moveup = true : this.flags.moveup = false;
        $("#sn_block-" + this.snippetId).find(".up-down").find(".nav-down").hasClass("naviActive") ? this.flags.movedown = true : this.flags.movedown = false;
    }
};