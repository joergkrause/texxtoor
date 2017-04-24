var AUTHOR = (function (my) {

  my.findCommand = function () {
    var $this = this;
    $this.clearFR();
    if (!$this.fr.is(":visible")) {
      $this.fr.find("#replace").hide();
      $this.fr.slideDown(100, function () {
        $this.fr.find("#find input").focus();
      });
      $this.highlightText();
    } else if ($this.fr.is(":visible") && $this.fr.find("#replace").is(":visible")) {
      $this.fr.slideUp(100, function () {
        $this.fr.find("#replace").hide();
        $this.fr.slideDown(100, function () {
          $this.fr.find("#find input").focus();
        });
        $this.highlightText();
      });
    } else {
      $(".editor").removeHighlight();
      $this.fr.slideUp(100, function () {
        $this.fr.find("#replace").show(function () {
          $this.fr.find("#find input").focus();
        });
      });
    }
  };
  my.replaceCommand = function () {
    var $this = this;
    $this.clearFR();
    if ($this.fr.is(":visible") && !$this.fr.find("#replace").is(":visible")) {
      $this.fr.slideUp(100, function () {
        $this.fr.find("#replace").show();
        $this.fr.slideDown(200, function () {
          $this.fr.find("#find input").focus();
        });
        $this.highlightText();
      });

    } else if (!$this.fr.is(":visible")) {
      $this.fr.slideDown(100, function () {
        $this.fr.find("#find input").focus();
      });
      $this.highlightText();
    } else {
      $(".editor, .CodeMirror, .editableSection").removeHighlight();
      $this.fr.slideUp(100, function () {
        $this.fr.find("#replace").show();
        $this.fr.find("#find input").focus();
      });
    }
  };
  my.search = function (e) {
    var $this = this;
    if ($(e).val().length == 0) {
      $(".editor, .CodeMirror, .editableSection").removeHighlight();
      return null;
    }
    $(".editor, .CodeMirror, .editableSection").removeHighlight().highlight($(e).val());
    return $(".editor, .CodeMirror, .editableSection").find(".highlight");
  };
  my.searchSnippetId = function (searchValue) {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.searchSnippetId,
      data: {
        id: $this.documentId,
        chapterId: $this.chapterId,
        snippetId: $this.snippetId,
        value: searchValue,
        direction: $this.direction
      },
      type: "POST",
      cache: false,
      dataType: "json",
      success: function (data) {
        if (data.Id == 0) return;
        $this.setSnippetId(data.Id);
        $this.activeSearchElement.snippetId = data.Id;
        $this.snippetId = data.Id;
        $this.scrollManager.scrollToSnippet(data.Id);
      },
      error: function (data) {
      }
    });
  };
  my.setSearchOption = function () {
    this.highlights = this.search($("#find input"));
    if (this.direction == -1)
      this.activeSearchElement.position = this.hIndex = $('#sn_block-' + this.activeSearchElement.snippetId).find("span.highlight").length - 1;
    else if (this.direction == 1)
      this.activeSearchElement.position = this.hIndex = 0;
    this.setHighlightStyle();
    this.isLoaded = true;
  };
  return my;
}(AUTHOR || {}));