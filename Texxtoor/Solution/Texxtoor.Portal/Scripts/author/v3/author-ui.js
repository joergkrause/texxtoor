var AUTHOR = (function (my) {

  var CaretManager = function () {

    var isSetCaretDown = true;
    var isinTestMode = false;
    var setCaretToStart = function (target) {
      var range;
      if (target[0].createTextRange) {
        range = target[0].createTextRange();
        range.collapse(true);
        range.moveStart('character', 0);
        range.moveEnd('character', 0);
        range.select();
      } else {
        target.focus();
        var sel = window.getSelection();
        if (sel.rangeCount > 0) {
          var anchor = target.find('p:first');
          range = sel.getRangeAt(0);
          if ($.browser.mozilla) {
            //var nodeLength = sel.anchorNode.childNodes.length;
            //var firstNode = nodeLength > 0 ? sel.anchorNode.childNodes[1] : sel.anchorNode;
            if (anchor.length > 0) {
              range.setStart(anchor.get(0), 1);
              range.setEnd(anchor.get(0), 1);
            }
          } else {
            if (anchor.length > 0) {
              range.setStart(anchor.get(0), 0);
              range.setEnd(anchor.get(0), 0);
            }
          }
          sel.removeAllRanges();
          sel.addRange(range);
        }
      }
    };
    
    function findCaretTarget(target, isDown) {
      var $this = this;
      switch (target.data('editor')) {
        case "ListingEditor":
          break;
        case "ImageEditor":
        case "TableEditor":
          target.find('input[data-item="' + target.data('item') + '"]').focus();
          break;
        case "HtmlEditor":
          var editor = target.find(".editor");
          if (isDown) {
            setCaretToStart(editor);
          } else {
            $this.setCaretToEnd(editor);
          }
          break;
        default:
          if (isDown) {
            setCaretToStart(target);
          } else {
            $this.setCaretToEnd(target);
          }
          break;
      }
      return true;
    }
    
    function setCaretToEnd(target) {
      var range;
      if (target.length > 0 && target[0].createTextRange) {
        range = editor[0].createTextRange();
        range.moveToElementText(target);
        range.collapse(true);
        range.moveStart('character', target[0].innerHTML.length - 1);
        range.moveEnd('character', target[0].innerHTML.length - 1);
        range.select();
      } else {
        target.focus();
        var sel = window.getSelection();
        if (sel.rangeCount > 0) {
          range = sel.getRangeAt(sel.rangeCount - 1);
          if ($.browser.mozilla) {
            var nodeLength = sel.anchorNode.childNodes.length;
            var lastNode = nodeLength > 0 ? sel.anchorNode.lastChild : sel.anchorNode;

            range.setStart(lastNode, lastNode.length);
            range.setEnd(lastNode, lastNode.length);
          } else {
            var anchor = target.find('p:last');
            if (anchor.length > 0) {
              range.setStart(anchor.get(0), Math.min(anchor.get(0).innerText.length, 1));
              range.setEnd(anchor.get(0), Math.min(anchor.get(0).innerText.length, 1));
            }
          }
          try {
            sel.removeAllRanges();
            sel.addRange(range);
          } catch(e) {
            //
          }
        }
      }
    }
    
    function setEditorCaret() {
      this.findCaretTarget($(".editableByTexxtoor[data-item='" + my.snippetId + "']"), isSetCaretDown);
      isSetCaretDown = false;
    }
    
    function wait(time) {
      return $.Deferred(function (dfd) {
        setTimeout(dfd.resolve, time);
      });
    }
    
    function canMoveCaret(e, obj, moveFunction) {
      var $this = this;
      if (isinTestMode) return;
      $this.isinTestMode = true;
      var $obj = obj;
      var currentPos = this.getCaretCharacterOffsetWithin($obj);
      var totalLength = $obj.text().length;
      if ((currentPos == 0 && (e.which == 37 || e.which == 38)) || ((totalLength == currentPos) && (e.which == 39 || e.which == 40))) {
        moveFunction();
      }
      return;
      var desiredPos = -1;
      // move further
      var eventObj;
      if (document.createEventObject) {
        eventObj = document.createEventObject();
        eventObj.keyCode = e.keyCode;
        $obj.get(0).fireEvent("onkeydown", eventObj);
      } else if (document.createEvent) {
        eventObj = document.createEvent("Events");
        eventObj.initEvent("keydown", true, true);
        eventObj.which = e.keyCode;
        $obj.get(0).dispatchEvent(eventObj);
      }

      //$this.wait(1).done(function() {
      desiredPos = $this.getCaretCharacterOffsetWithin($obj);
      if (desiredPos != currentPos) {
        var returnCode = e.which > 38 ? e.which - 2 : e.which + 2;
        if (document.createEventObject) {
          eventObj = document.createEventObject();
          eventObj.keyCode = returnCode;
          $obj.get(0).fireEvent("onkeydown", eventObj);
        } else if (document.createEvent) {
          eventObj = document.createEvent("Events");
          eventObj.initEvent("keydown", true, true);
          eventObj.which = returnCode;
          $obj.get(0).dispatchEvent(eventObj);
        }
      }
      if (desiredPos == currentPos) {
        //moveFunction();
      }
      isinTestMode = false;
      //});
    }

    function getCaretCharacterOffsetWithin(obj) {
      var win = document.defaultView || document.parentWindow;
      var sel, range, preCaretRange, caretOffset = 0;
      if (typeof win.getSelection != "undefined") {
        sel = win.getSelection();
        if (sel.rangeCount) {
          range = sel.getRangeAt(0);
          preCaretRange = range.cloneRange();
          preCaretRange.selectNodeContents(obj[0]);
          preCaretRange.setEnd(range.endContainer, range.endOffset);
          caretOffset = preCaretRange.toString().length;
        }
      } else if ((sel = document.selection) && sel.type != "Control") {
        range = document.selection.createRange();
        preCaretRange = document.body.createTextRange();
        preCaretRange.moveToElementText(obj[0]);
        preCaretRange.setEndPoint("EndToEnd", range);
        caretOffset = preCaretRange.text.length;
      }
      return caretOffset;
    }

    return {
      /********************** Caret Mngmt *************************/
      findCaretTarget: findCaretTarget,
      setCaretToEnd: setCaretToEnd,
      setEditorCaret: setEditorCaret,
      canMoveCaret: canMoveCaret,
      getCaretCharacterOffsetWithin: getCaretCharacterOffsetWithin
    };
  };

  my.caretManager = new CaretManager();

  var ScrollManager = function() {

    var scrollPosition, percentScrolled = 0;

    function scrollToSnippet(id) {
      var $this = this;
      $('#sn_block-' + id)[0].scrollIntoView();
    }
    
    function setScrollPosition(position) {
      var $this = this;
      scrollPosition = position;
    }
    
    function setElementPositionNext(element) {
      var $this = this;
      var top = $(".snippet-block:last").position().top + $(".snippet-block:last").outerHeight(true);
      element.css({
        top: top + 'px'
      });
      return element.appendTo($this.snippetContainer);
    }
    
    function setElementPositionPrev(element) {
      var $this = this;
      var top = $(".snippet-block:last").prev().position().top - element.outerHeight(true);
      element.css({
        top: top + 'px'
      });
    }
    
    function resetScrollPosition() {
      scrollPosition = $.cookie("ScrollPosition");
      var chapterId = $.cookie("Chapter");
      if (typeof scrollPosition != "undefined" && chapterId == this.chapterId) {
        this.setScrollPosition(this.scrollMaxPosition * scrollPosition);
      } else {
        this.setScrollPosition(0);
      }
    }

    return {
      scrollToSnippet: scrollToSnippet,
      setScrollPosition: setScrollPosition,
      setElementPositionNext: setElementPositionNext,
      setElementPositionPrev: setElementPositionPrev,
      resetScrollPosition: resetScrollPosition,
      destroyScrollPosition: function () {
        $.cookie("ScrollPosition", null);
      },
      saveScroll: function () {
        $.cookie("ScrollPosition", percentScrolled);
        $.cookie("Chapter", this.chapterId);
      },
      scrollBy: function (deltaY) {
        var $this = this;
        if ($this.contentLength == 0) return;
        var snippetHeight = $('#sn_block-' + $this.currentItem.id).height();
        if (snippetHeight == undefined) snippetHeight = $(".snippet-block:first").height();
        if (isNaN(percentScrolled)) percentScrolled = 0;
        var destY = percentScrolled * 100 + (deltaY) * (20.0 / snippetHeight) * $this.locations[$this.currentIndex].percent;
        if ($this.locations[$this.currentIndex].position > destY)
          destY = $this.locations[$this.currentIndex].position - 0.0001;
        else if ($this.locations[$this.currentIndex].position + $this.locations[$this.currentIndex].percent < destY)
          destY = $this.locations[$this.currentIndex].position + $this.locations[$this.currentIndex].percent + 0.0001;
        $this.setScrollPosition(destY * $this.scrollMaxPosition / 100);
      }
    };
  };

  my.scrollManager = new ScrollManager();

  my.setStatusBar = function (text, message) {
    if (this.statusBar != null && !message) {
      $(this.statusBar).html(text);
    }
    if (this.messageBar != null && message) {
      $(this.messageBar).html(text);
    }
  };

  my.setChapterButtons = function () {
    var $this = this;
    for (var counter = 0; counter < $this.chapterIdsList.length; counter++) {
      if ($this.chapterIdsList[counter] == $this.chapterId) {
        if (counter == 0) {
          if ($this.chapterIdsList.length == 1) {
            $(".ribbon-button[data-option='prev']").prop("disabled", true);
            $(".ribbon-button[data-option='next']").prop("disabled", true);
            return;
          } else {
            $(".ribbon-button[data-option='prev']").prop("disabled", true);
            $(".ribbon-button[data-option='next']").prop("disabled", false);
            return;
          }
        }
        if (counter == $this.chapterIdsList.length - 1) {
          $(".ribbon-button[data-option='prev']").prop("disabled", false);
          $(".ribbon-button[data-option='next']").prop("disabled", true);
          return;
        } else {
          $(".ribbon-button[data-option='prev']").prop("disabled", false);
          $(".ribbon-button[data-option='next']").prop("disabled", false);
          return;
        }
      }
    }
  };

  my.removeHighlightSnippet = function () {
    var $this = this;
    // remove highlight from all others
    $('div.editable_highlight').each(function () {
      $(this).hide();
    });
  };

  my.highlightSnippet = function () {
    var $this = this;
    $this.removeHighlightSnippet();
    // add highlight to current
    if ($('#leftbar').is(":visible"))
      $('div.editable_highlight[data-item="' + $this.snippetId + '"]').show();

    $('div.editable_highlight[data-item="' + $this.snippetId + '"]').height($('.editableByTexxtoor[data-item="' + $this.snippetId + '"]').height());
  };

  /********************** Tools Mngmt **************************/
  my.showLoader = function (msg) {
    var $this = this;
    $(".loader-msg").text(msg);
    $(".loader-layout").css({
      top: 0,
      left: 0,
      right: 0,
      bottom: 0
    }).fadeIn(200, "linear");
  };
  my.hideLoader = function () {
    $(".loader-layout").fadeOut(200, "linear");
  };
  /*# Action #*/
  my.formatCommand = function (src, cmd, args) {
    var $this = this;
    if ($this.snippetId) {
      if ($(src).attr('disabled') != "disabled") {
        $('#sn-' + $this.snippetId).htmlarea(cmd, args);
      }
    } else {
      alert('Cannot invoke this command, please select text snippet\n\nCommand: ' + cmd);
    }
  };
  my.updateTableOfContent = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.tableOfContent,
      data: { id: $this.documentId },
      type: 'GET',
      cache: false,
      datatype: 'html',
      success: function (idata) {
        var data = JSON.parse(idata);
        $('#tocpane').find('li').remove();
        for (var i = 0; i < data.length; i++) {
          $('<li/>')
            .append($('<a>')
              .attr('href', $this.locationUrl.chapterLocation + "?chapterId=" + data[i].Id)
              .data('item', data[i].Id)
              .text(data[i].Name)              
              ).appendTo($('#tocpane')
           );
        }
      }
    });
  };
  my.getDocumentProperties = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.getDocumentProperties,
      data: { id: $this.documentId },
      type: 'GET',
      cache: false,
      datatype: 'html',
      success: function (idata) {
        var data = JSON.parse(idata);
        $('#docPropsWordCount').text(data.wordCount);
        $('#docPropsCharacterCount').text(data.charCount);
      }
    });
  };
  my.resize = function () {
    $('.wrapper').css('height', document.body.clientHeight - $('#statusBarContainer').height() - $('#tools').height() - 87);
    var top = 0;
    $(".snippet-block").each(function () {
      $(this).css({
        top: top + 'px'
      });
      //top += $(this).outerHeight(true);
      top += $(this).innerHeight();
    });

    $('#body').height(top + (top / 2) + 1000);
    $('div.img').css('max-width', $('#body').width() - 100);
  };

  return my;
}(AUTHOR || {}));