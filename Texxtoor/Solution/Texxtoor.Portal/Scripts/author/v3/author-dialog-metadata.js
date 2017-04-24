var AUTHOR = (function (my) {

  my.insertSemantic = function(v) {
    var $this = this;
    if (!$this.snippetId) return;    
    $('#sn-' + $this.snippetId).htmlarea(v);
  };
  my.insertTerm = function (type, v, title, range) {    
    var $this = this;
    if (!my.snippetId) return;
    $('#sn-' + my.snippetId).focus().click();
    var selectionData = $('#sn-' + $this.snippetId).htmlarea('getSelectedHTML');
    var selection = range ? range.textContent : selectionData.html;
    var trailingSpace = range ? "&#160;" : (selectionData.space == " " ? "&#160;" : "");
    if (selection.length == 0) {
      selection = title;
    }
    var targetElement = "span";
    var cls = "";
    // get the element for this term type
    switch (type) {
    case 'abbreviation':
      targetElement = "abbr";
      break;
    case 'cite':
      targetElement = "cite";
      break;
    case 'idiom':
      targetElement = "em";
      break;
    case 'variable':
      targetElement = "var";
      break;
    case 'definitions':
      targetElement = "dfn";
      break;
    case 'links':
      targetElement = "a";
      cls = "externallink";
      break;
    }
    if (range) {
      $('#sn-' + $this.snippetId).htmlarea('selectRange', range);
    }
    $('#sn-' + $this.snippetId).htmlarea('pasteHTML', '<' + targetElement + ' class="' + cls + '" contenteditable="false" title="' + title + '" data-item="' + v + '" data-type="' + type + '">' + selection + '</' + targetElement + '>' + trailingSpace);
    my.deferredSave($('#sn-' + $this.snippetId));
    my.undoManager.register($('#sn-' + $this.snippetId), my.deleteTerm, [$this.snippetId, v], "Insert Term '" + type + "'");
  };
  my.deleteTerm = function (id, item) {
    // not undoable, this is for undo only
    var term = $('#sn-' + id + ' [data-item=' + item + ']');
    var text = term.text();
    term.replace(text);
  };
  my.setMetaData = function(obj) {
    var $this = this;
    if ($this.snippetId == null) {
      $(obj).attr("checked", false);
      return;
    }
    var index = $(obj).closest("div").index();
    var itemIndex;

    if ($(obj).is("li")) {
      itemIndex = $(obj).index();
    } else {
      itemIndex = $(obj).closest("li").index();
    }
    var item = $('.metaDataDialog[data-item="' + $this.snippetId + '"]').find("table tr:eq(1) td:eq(" + index + ") input:eq(" + itemIndex + ")");
    item.attr("checked", "checked");
    $(obj).is("li") ? $(obj).find("input").attr("checked", "checked") : $(obj).attr("checked", "checked");
  };
  my.setMetaDataRibbonMenu = function() {
    var $this = this;
    $('.metaDataDialog[data-item="' + $this.snippetId + '"]').find("table tr:eq(1) input").each(function(index) {
      if ($(this).attr("checked") == "checked") {
        $("li.sem:eq(" + index + ") input").attr("checked", $(this).attr("checked"));
      } else {
        $("li.sem:eq(" + index + ") input").removeAttr("checked");
      }
    });
  };
  return my;
}(AUTHOR || {}));