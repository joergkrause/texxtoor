var AUTHOR = (function (my) {
  my.loadRibbonImages = function () {
    $.ajax({
      url: my.serviceUrl.getThumbnails,
      data: {
        id: my.documentId,
        w: 32, h: 32
      },
      cache: false,
      dataType: 'json',
      contentType: 'application/json',
      success: function (idata) {
        var data = JSON.parse(idata);
        my.createRibbonImages(data.ribbonImages, data.ribbonTitle);
      }
    });
  };
  my.createRibbonImages = function (ribbonImagesObj, title) {
    var $this = this;
    var ribbonImages = new _RibbonImages();
    ribbonImages.resourceObj = ribbonImagesObj;
    ribbonImages.title = title;
    var html = ribbonImages.getWidgetHtml();
    $("#documentImages").empty();
    $("#documentImages").html(html);
  };
  /*# Tools UI #*/
  my.updateWidgetTools = function () {
    var $this = this;
    var snippetObj = $("#sn_block-" + $this.snippetId);
    var isSection = $(snippetObj).find(".editableSection").length > 0;
    var isImage = $(snippetObj).find(".editableImage").length > 0;
    var isListing = $(snippetObj).find(".listingEditor").length > 0;
    var isTable = $(snippetObj).find(".tableEditor").length > 0;
    var deep;
    var nxtSectionLevel = 0;
    deep = parseInt($this.getSnippetLevel(snippetObj)) + 1;
    // Check if the parent of the current element is not a chapter, if it is a chapter then we dont have to do any thing
    var sameLevelSnippets = $this.getAllSectionsAfterLevel(snippetObj, deep);
    if (sameLevelSnippets != undefined) {
      if (sameLevelSnippets.length > 0) {
        var currentObjLevel = $this.getSnippetLevel(sameLevelSnippets[0]);
        for (var counter = 0; counter < sameLevelSnippets.length; counter++) {
          if ((($(sameLevelSnippets).data("parentid") == $(snippetObj).data("parentid")) || $(sameLevelSnippets).data("parentid") == $this.snippetId) && deep <= currentObjLevel) {
            nxtSectionLevel = $this.getSnippetLevel(sameLevelSnippets[counter]);
          }
        }
      }
    }
    if ($this.snippetId != $this.chapterId) {
      !((nxtSectionLevel == 0) ? (deep >= 2) : (2 >= nxtSectionLevel - 2)) ? $(".ribbon-button[data-action='section']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='section']").removeAttr("disabled").fadeTo("10", "1.0");
      !((nxtSectionLevel == 0) ? (deep >= 3) : (3 >= nxtSectionLevel - 2)) ? $(".ribbon-button[data-action='subsection']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='subsection']").removeAttr("disabled").fadeTo("10", "1.0");
      !(deep >= 4) ? $(".ribbon-button[data-action='subsubsection']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='subsubsection']").removeAttr("disabled").fadeTo("10", "1.0");
      !(deep >= 5) ? $(".ribbon-button[data-action='subsubsubsection']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='subsubsubsection']").removeAttr("disabled").fadeTo("10", "1.0");
    } else {
      $(".ribbon-button[data-action='section']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='subsection']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='subsubsection']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='subsubsubsection']").attr("disabled", "disabled").fadeTo("10", "0.4");
    }
    if (isImage) {
      $(".isImage").removeAttr("disabled").fadeTo("10", "1.0");
    } else {
      $(".isImage").attr("disabled", "disabled").fadeTo("10", "0.4");
    }
    if (isTable) {
      $(".isTable").removeAttr("disabled").fadeTo("10", "1.0");
    } else {
      $(".isTable").attr("disabled", "disabled").fadeTo("10", "0.4");
    }
    if (isSection || isImage) {
      $('.semanticPane .semanticElements').attr("disabled", "disabled").fadeTo("10", "0.4");
      !($(snippetObj).find('.nav-left').hasClass('naviActive')) ? $(".ribbon-button[data-action='l']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='l']").removeAttr("disabled").fadeTo("10", "1.0");
      !($(snippetObj).find('.nav-right').hasClass('naviActive')) ? $(".ribbon-button[data-action='r']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='r']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='u']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='d']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='bold']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='italic']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='underline']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='subscript']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='superscript']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='insertUnorderedList']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='insertOrderedList']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='indent']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='outdent']").attr("disabled", "disabled").fadeTo("10", "0.4");
    } else {
      $('.semanticPane .semanticElements').removeAttr("disabled").fadeTo("10", "1.0");
      !($(snippetObj).find('.nav-up').hasClass('naviActive')) ? $(".ribbon-button[data-action='u']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='u']").removeAttr("disabled").fadeTo("10", "1.0");
      !($(snippetObj).find('.nav-down').hasClass('naviActive')) ? $(".ribbon-button[data-action='d']").attr("disabled", "disabled").fadeTo("10", "0.4") : $(".ribbon-button[data-action='d']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='l']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='r']").attr("disabled", "disabled").fadeTo("10", "0.4");
      $(".ribbon-button[data-action='bold']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='italic']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='underline']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='subscript']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='superscript']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='insertUnorderedList']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='insertOrderedList']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='indent']").removeAttr("disabled").fadeTo("10", "1.0");
      $(".ribbon-button[data-action='outdent']").removeAttr("disabled").fadeTo("10", "1.0");
    }
    //var htmlArea = $(snippetObj).aloha();
    //if (htmlArea) {
    //  //htmlArea.Command ...
    //  //$(snippetObj).htmlarea('qc', 'bold') && $(snippetObj).htmlarea('qc', 'bold').state ? $(".ribbon-button[data-action='bold']").css('background-color', 'orange') : $(".ribbon-button[data-action='bold']").css('background-color', '');
    //  //$(snippetObj).htmlarea('qc', 'italic') && $(snippetObj).htmlarea('qc', 'italic').state ? $(".ribbon-button[data-action='italic']").css('background-color', 'orange') : $(".ribbon-button[data-action='italic']").css('background-color', '');
    //  //$(snippetObj).htmlarea('qc', 'underline') && $(snippetObj).htmlarea('qc', 'underline').state ? $(".ribbon-button[data-action='underline']").css('background-color', 'orange') : $(".ribbon-button[data-action='underline']").css('background-color', '');
    //  //$(snippetObj).htmlarea('qc', 'subscript') && $(snippetObj).htmlarea('qc', 'subscript').state ? $(".ribbon-button[data-action='subscript']").css('background-color', 'orange') : $(".ribbon-button[data-action='subscript']").css('background-color', '');
    //  //$(snippetObj).htmlarea('qc', 'superscript') && $(snippetObj).htmlarea('qc', 'superscript').state ? $(".ribbon-button[data-action='superscript']").css('background-color', 'orange') : $(".ribbon-button[data-action='superscript']").css('background-color', '');
    //  var inList = false;
    //  if ($(snippetObj).htmlarea('qc', 'insertUnorderedList') && $(snippetObj).htmlarea('qc', 'insertUnorderedList').state) {
    //    $(".ribbon-button[data-action='unorderedList']").css('background-color', 'orange');
    //    inList |= true;
    //  } else {
    //    $(".ribbon-button[data-action='unorderedList']").css('background-color', '');
    //  }
    //  if ($(snippetObj).htmlarea('qc', 'insertOrderedList') && $(snippetObj).htmlarea('qc', 'insertOrderedList').state) {
    //    $(".ribbon-button[data-action='orderedList']").css('background-color', 'orange');
    //    inList |= true;
    //  } else {
    //    $(".ribbon-button[data-action='orderedList']").css('background-color', '');
    //  }
    //  if (inList) {
    //    $(".ribbon-button[data-action='indent']").removeAttr("disabled").fadeTo("10", "1.0");
    //    $(".ribbon-button[data-action='outdent']").removeAttr("disabled").fadeTo("10", "1.0");
    //  } else {
    //    $(".ribbon-button[data-action='indent']").attr("disabled", "disabled").fadeTo("10", "0.4");
    //    $(".ribbon-button[data-action='outdent']").attr("disabled", "disabled").fadeTo("10", "0.4");
    //  }
    //}
    //if (!(($(snippetObj).find(".editableText") != undefined && $(snippetObj).find(".editableText").length > 0)
    //  && ($($this.getNextSnippet(snippetObj)).find(".editableText") != undefined
    //    && $($this.getNextSnippet(snippetObj)).find(".editableText").length > 0))) {
    //  $(".ribbon-button[data-action='mergetext']").attr("disabled", "disabled").fadeTo("10", "0.4");
    //} else {
    //  $(".ribbon-button[data-action='mergetext']").removeAttr("disabled").fadeTo("10", "1.0");
    //}
  };
  my.closeAllPanes = function () {
    // placeholder function
  };
  my.loadSemanticData = function () {
    var $this = this;
    // ask server for semantic lists (abbreviation, variable, ...)
    var type = ['abbreviation', 'cite', 'idiom', 'variable', 'definition', 'link'];
    $.each(type, function (i, e) {
      // order doesn't matter
      $.ajax({
        url: $this.serviceUrl.getSemanticLists,
        data: {
          id: $this.documentId,
          type: e
        },
        type: "GET",
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (idata) {
          if (idata == null) {
            return;
          }
          var data = JSON.parse(idata);
          var ul = $('ul[data-type=' + e + ']');
          $.each(data, function (idx, res) {
            $('<li/>').data('option', e).data('value', res.Value).data('command', 'insert').data('action', 'term').addClass('ribbon-button').text(res.Text).appendTo(ul);
          });
        }
      });
    });
  };
  my.loadSidebarTypes = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.getSidebarType,
      type: "GET",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (idata) {
        if (idata == null) {
          return;
        }
        var data = JSON.parse(idata);
        var ul = $('ul[data-type=sidebartypes]');
        $.each(data, function (idx, res) {
          $('<li/>').data('item', res.Id).data('value', res.Name).text(res.Name).appendTo(ul);
        });
      }
    });
  };
  return my;
}(AUTHOR || {}));