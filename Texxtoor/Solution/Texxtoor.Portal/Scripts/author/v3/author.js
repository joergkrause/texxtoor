// Author: Joerg Krause, joerg@augmentedbooks.de
// Modified By: Zeeshan Chaudhary, zeeshan.chaudhary@yahoo.com
// Assume each widget comes with 3 data attributes:
// data-editor : The editor used to work client side, if empty "contenteditable" is used
// data-style : a style being copied into the iframe
// data-item : the snippet id

var AUTHOR = (function (my) {

  my.documentId = 0;
  my.chapterId = 0;
  my.snippetId = 0;
  my.serviceUrl = null;
  my.locationUrl = null;
  my.statusBar = null;
  my.messageBar = null;
  my.snippetContainer = null;
  my.range = null;
  my.isSaved = false;
  my.isLoaded = false;
  my.cropCoords = null;
  my.keepImageRatio = false;
  my.snippetStructure = null;
  my.contentData = [];
  my.direction = 1;
  my.jsonObj = null;
  my.sortableIn = 1;
  my.activeSearchElement = {};
  my.chapterIdsList = [];
  my.crop = null;
  my.isMoveBusy = false;
  my.isInserting = false;  

  /************************************************************\
  |******************* Preperation & Init *********************|
  \************************************************************/
  my.initSnippetContainer = function () {
    var $this = this;
    if ($.browser.opera) {
      document.designMode = "on";
    }
    $this.isLoaded = true;
    $this._attachEvents();
    $this._initMenus();
  }; 

  my.initSnippetEditor = function (snippetBlock) {
    var $this = this;
    // private functions used to activate snippet as current snippet in editor
    var editable = $(snippetBlock).find(".editableByTexxtoor");
    var id = $(editable).data("item");
    // editor specific activation
    switch ($(editable).data("editor")) {
      case "SidebarEditor":
        var header = $(editable).find('div.editor').find('header');
        $(editable).find('div.editor').data('type') == 99 ? header.attr("contenteditable", 'true') : header.attr("contenteditable", 'false');
        // fall through intentionally
      case "TableEditor":
      case "HtmlEditor":
        $(editable).aloha(); // TODO: Make this call AFTER all snippets have been loaded, Aloha can handle this
        break;
      case "ListingEditor":        
        var lnVal = $('#sn_block-' + id + ' input[name=linenumbers]').val();
        var txt = $(editable).find('textarea.code')[0];
        var code = CodeMirror.fromTextArea(txt, {
          lineNumbers: (lnVal == undefined ? false : lnVal.toLowerCase() === 'true'),
          mode: $('#sn_block-' + id + ' input[name=language]').val(),
          matchBrackets: true
        });
        code.on('change', function (cm, cmo) {
          var elm = $(cm.getTextArea()).parents('.editableByTexxtoor');
          $this.deferredSave(elm);
        });
        $(editable).data('CodeMirror', code);
        break;
      case "ImageEditor":
        //var img = $('#sn_block-' + id + ' div.img');
        //$(img).width($(".imagePane input[name='width']").val());
        //$(img).height($(".imagePane input[name='height']").val());
        break;
      default:
        // default live events
        break;
    }
    $this._initSnippetEditorButtons(snippetBlock);
  };
  my._initSnippetEditorButtons = function (snippetBlock) {
    var $this = this;
    var block = $(snippetBlock);
    var up = block.find('.nav-up');
    var down = block.find('.nav-down');
    // render is not for sections, hence we check first
    if (up.data('init-renderactive')) {
      if (up.data('init-renderactive')) {
        up.addClass('naviActive');
      } else {
        up.removeClass('naviActive');
      }
      if (down.data('init-renderactive')) {
        down.addClass('naviActive');
      } else {
        down.removeClass('naviActive');
      }
    }
    var left = block.find('.nav-left');
    var right = block.find('.nav-right');
    var deep = parseInt($this.getSnippetLevel(block)) + 1;
    var nxtSectionLevel = 0;
    // Check if the parent of the current element is not a chapter, if it is a chapter then we dont have to do any thing
    var sameLevelSnippets = $this.getAllSectionsAfterLevel(block, deep);
    if (sameLevelSnippets != undefined) {
      if (sameLevelSnippets.length > 0) {
        var currentObjLevel = $this.getSnippetLevel(sameLevelSnippets[0]);
        for (var counter = 0; counter < sameLevelSnippets.length; counter++) {
          if ((($(sameLevelSnippets).data("parentid") == $(block).data("parentid")) || $(sameLevelSnippets).data("parentid") == $this.chapterId) && deep <= currentObjLevel) {
            nxtSectionLevel = $this.getSnippetLevel(sameLevelSnippets[counter]);
          }
        }
      }
    }
    // applies to sections only, so not all snippets have these btns
    if (left.length == 1 && right.length == 1) {
      if (left.data('init-renderactive') && nxtSectionLevel < deep) {
        left.addClass('naviActive');
      } else {
        left.removeClass('naviActive');
      }
      if (right.data('init-renderactive') && nxtSectionLevel > deep) {
        right.addClass('naviActive');
      } else {
        right.removeClass('naviActive');
      }
    }
  };
  my.disableDropOnChildren = function (droppingElementSnippet) {
    var id = $(droppingElementSnippet).attr("id").replace("sn_block-", "");
    var block = $(droppingElementSnippet);
    var isChild = false;
    var snippetObj = $(block).find("span");
    var snippetObjClassName = $(snippetObj).attr("class");
    var snippetLevel = snippetObjClassName.replace("editableNumberLevel", "");
    var obj, objClassName, level;
    var snippetData = [];
    var faddedSnippetData = [];
    $.each($('.snippet-block'), function (idx, e) {
      snippetData.push(e);
    });
    for (var counter = 0; counter < snippetData.length; counter++) {
      if ($(snippetData[counter]).find(".editableSection") != undefined && $(snippetData[counter]).find(".editableSection").length > 0) {
        obj = $(snippetData[counter]).find("span");
        objClassName = $(obj).attr("class");
        level = objClassName.replace("editableNumberLevel", "");
      }
      if (isChild && snippetLevel >= level && $(snippetData[counter]).attr("id").replace("sn_block-", "") != id) {
        if ($(snippetData[counter]).find(".editableSection") != undefined && $(snippetData[counter]).find(".editableSection").length > 0) {
          break;
        }
      }
      if ($(snippetData[counter]).attr("id").replace("sn_block-", "") == id) {
        isChild = true;
      }
      if (isChild) {
        //$(snippetData[counter]).css("opacity", "0.5");
        $(snippetData[counter]).css('opacity', 0.50);
        faddedSnippetData.push(snippetData[counter]);
      }
    }
    return faddedSnippetData;
  };
  my.AutoSetSnippetEditorButtons = function () {
    if (!!my.proofMode) {
      // Proof reader cannot change structure
      $(".right-left, .up-down").css('opacity', '0.5');
      return;
    }
    //console.log("The reset is fired");
    var snippetData = [];
    this.isMoveBusy = false;
    // Creating variables to hold each level
    var section1 = [];
    var section11 = [];
    var section111 = [];
    var section1111 = [];
    var $this = this;
    var rb = $('#rightbar').offset();
    var faddedSnippetsForDragDrop;
    // Get all snippets       
    $.each($('.snippet-block'), function (idx, e) {
      //-> Start -> Remove drag drop classes and margins
      $(e).removeClass("moveTargetSelection");
      $(e).css("opacity", "1");
      $(e).find(".up-down").css("top", "0px");
      $(e).find(".right-left").css("top", "0px");
      //$(e).find(".up-down").addClass("ui-draggable");
      $(e).find(".up-down").draggable({
        containment: [96, rb.top + 87, 96, rb.top + $('#rightbar').height()],
        snapMode: "inner",
        snap: true,
        start: function (ee, u) {
          $this.setSnippetId(u.helper.data('item'));
          //var parentObject = $(e.target).parent();
          //console.log($(parentObject));
          //$(parentObject).css('border-bottom', '2px solid green');
          //$(e.target).css('border-bottom', '2px solid green');
          //console.log($(e.target));
        },
        stop: function () {
          //$(e.target).css('border-bottom', '0px');
          var afterId = $('.moveTargetSelection').data('item');
          if (afterId != $this.snippetId) {
            $this.move('m', $this.snippetId, afterId);
          } else {
            console.log("Initialize");
            $this.AutoSetSnippetEditorButtons();
          }
        },
        drag: function (ee, u) {
          var top = u.offset.top - 130 + $('div.wrapper').scrollTop(); // 130 == ribbon height
          $('.snippet-block').each(function () {
            var t = $(this).position().top;
            var h = $(this).height();
            $(this).find('.debugDisplay').html("T:" + t + " H: " + h);
            if (top >= t && top <= (t + h)) {
              //console.log("Class is added");
              $(this).addClass('moveTargetSelection');
            } else {
              $(this).removeClass('moveTargetSelection');
              //console.log("Class is removed");
            }
          });
        }
      });
      var currentSnippetID;
      $(e).find(".right-left").draggable({
        containment: [96, rb.top + 87, 96, rb.top + $('#rightbar').height()],
        snapMode: "inner",
        snap: true,
        start: function (ee, u) {
          currentSnippetID = $(ee.target).parent().attr("id").replace("sn_block-", "");
          faddedSnippetsForDragDrop = $this.disableDropOnChildren($(ee.target).parent());
          $this.setSnippetId(u.helper.data('item'));
          $(ee.target).css('border-bottom', '2px solid green');
        },
        stop: function (ee) {
          $(ee.target).css('border-bottom', '0px');
          console.log(currentSnippetID);
          var afterId = $('.moveTargetSelection').data('item');
          //if ($this.snippetId == 0)
          $this.snippetId = currentSnippetID;
          for (var counter = 0; counter < faddedSnippetsForDragDrop.length; counter++) {
            if (afterId == $(faddedSnippetsForDragDrop[counter]).attr("id").replace("sn_block-", "")) {
              alert("Warning! this move is not allowed");
              $this.AutoSetSnippetEditorButtons();
              return;
            }
          }
          if (afterId != $this.snippetId) {
            $this.move('m', $this.snippetId, afterId);
          } else {
            console.log("Initialize");
            $this.AutoSetSnippetEditorButtons();
          }
        },
        drag: function (ee, u) {
          var top = u.offset.top - 130 + $('div.wrapper').scrollTop(); // 130 == ribbon height
          $('.snippet-block').each(function () {
            var t = $(this).position().top;
            var h = $(this).height();
            $(this).find('.debugDisplay').html("T:" + t + " H: " + h);
            if (top >= t && top <= (t + h)) {
              //console.log("Class is added");
              $(this).addClass('moveTargetSelection');
            } else {
              $(this).removeClass('moveTargetSelection');
              //console.log("Class is removed");
            }
          });
        }
      });

      //->End
      snippetData.push(e);
    });
    for (var counter = 0; counter < snippetData.length; counter++) {
      // Check if the current node is a section
      if ($(snippetData[counter]).find(".editableSection") != undefined && $(snippetData[counter]).find(".editableSection").length > 0) {
        //-> Start -> trying to get the level for the section
        //var obj = $(snippetData[counter]).find("span");
        //var objClassName = $(obj).attr("class");
        //var level1 = objClassName.replace("editableNumberLevel", "");
        var level = $this.getSnippetLevel(snippetData[counter]);
        var left = $(snippetData[counter]).find('.nav-left');
        var right = $(snippetData[counter]).find('.nav-right');
        //-> End -> got the level for the section
        // Now traversing through each level
        switch (level) {
          case "2":
            {
              // This code is to move the sections up and down
              //if (section1 != null && section1.length > 0) {
              //    for (var innerCounter = 0; innerCounter < section1.length; innerCounter++) {
              //        // Check if there is a previous element with the same parent, so that we can show move up icons
              //        if ($(section1[innerCounter]).data("parentid") == $(snippetData[counter]).data("parentid")) {
              //            left.addClass('naviActive');
              //            break;
              //        }
              //        else
              //            left.removeClass('naviActive');
              //    }
              //}
              //else
              //    left.removeClass('naviActive');
              //// adding section to the section list
              //section1.push($(snippetData[counter]));

              // We cannot make this level section to go upward, however this section can be go downward
              //if it has some section behind to become the parent of this section after it is downgraded

              if (section1 != null && section1.length > 0) {
                for (var innerCounter = 0; innerCounter < section1.length; innerCounter++) {
                  // Check if there is a previous element with the same parent, so that we can associate this section after downgrade
                  if ($(section1[innerCounter]).data("parentid") == $(snippetData[counter]).data("parentid")) {
                    right.addClass('naviActive');
                    break;
                  } else
                    right.removeClass('naviActive');
                }
              } else
                right.removeClass('naviActive');

              // adding section to the section list
              section1.push($(snippetData[counter]));
              break;
            }
          case "3":
            {
              if (section11 != null && section11.length > 0) {
                for (var innerCounter = 0; innerCounter < section11.length; innerCounter++) {
                  // Check if there is a previous element with the same parent, so that we can associate this section after downgrade
                  if ($(section11[innerCounter]).data("parentid") == $(snippetData[counter]).data("parentid")) {
                    right.addClass('naviActive');
                    break;
                  } else
                    right.removeClass('naviActive');
                }
              } else
                right.removeClass('naviActive');
              // However, in each case, this item can always be upgraded to the level 2 So
              left.addClass('naviActive');

              // adding section to the section list
              section11.push($(snippetData[counter]));
              break;
            }
          case "4":
            {
              if (section111 != null && section111.length > 0) {
                for (var innerCounter = 0; innerCounter < section111.length; innerCounter++) {
                  // Check if there is a previous element with the same parent, so that we can associate this section after downgrade
                  if ($(section111[innerCounter]).attr("data-parentID") == $(snippetData[counter]).attr("data-parentID")) {
                    right.addClass('naviActive');
                    break;
                  } else
                    right.removeClass('naviActive');
                }
              } else
                right.removeClass('naviActive');
              // However, in each case, this item can always be upgraded to the level 2 So
              left.addClass('naviActive');
              // adding section to the section list
              section111.push($(snippetData[counter]));
              break;
            }
          case "5":
            {
              //if (section1111 != null && section1111.length > 0) {
              //    for (var innerCounter = 0; innerCounter < section1111.length; innerCounter++) {
              //        // Check if there is a previous element with the same parent, so that we can associate this section after downgrade
              //        if ($(section1111[innerCounter]).attr("data-parentID") == $(snippetData[counter]).attr("data-parentID")) {
              //            right.addClass('naviActive');
              //            break;
              //        }
              //        else
              //            right.removeClass('naviActive');
              //    }
              //}
              //else
              //    right.removeClass('naviActive');
              // However, there is no level below 5, so we will not add decrease here because it cannot be decreased
              right.removeClass('naviActive');
              left.addClass('naviActive');
              // adding section to the section list
              section1111.push($(snippetData[counter]));
              break;
            }
          default:
            {
              //it can be a chapter so we leave it
              break;
            }
        }
      }
        //current node is not a section
      else {
        var up = $(snippetData[counter]).find('.nav-up');
        var down = $(snippetData[counter]).find('.nav-down');
        // Check if the previous element is a section or not
        if (($(snippetData[counter - 1]).find(".editableSection") == undefined || $(snippetData[counter - 1]).find(".editableSection").length < 1)) {
          up.addClass('naviActive');
        } else
          up.removeClass('naviActive');
        // Check if the next element is a section or not
        if (($(snippetData[counter + 1]).find(".editableSection") == undefined || $(snippetData[counter + 1]).find(".editableSection").length < 1) && counter < snippetData.length - 1) {
          down.addClass('naviActive');
        } else
          down.removeClass('naviActive');
      }
    }
    // Do not delete this code because this code can be help ful in moving the sections up and down without changing the level

    //for (var counter = 0; counter < section1.length; counter++) {
    //    var right = $(section1[counter]).find('.nav-right');
    //    for (var innerCounter = 0; innerCounter < section1.length; innerCounter++) {
    //        // Check if there is any previous element on same level with same parent
    //        if ($(section1[innerCounter]).attr("data-parentID") == $(section1[counter]).attr("data-parentID") && innerCounter > counter) {
    //            right.addClass('naviActive');
    //            break;
    //        } else {
    //            right.removeClass('naviActive');
    //        }
    //    }
    //}
    //for (var counter = 0; counter < section11.length; counter++) {
    //    var right = $(section11[counter]).find('.nav-right');
    //    for (var innerCounter = 0; innerCounter < section11.length; innerCounter++) {
    //        // Check if there is any previous element on same level with same parent
    //        if ($(section11[innerCounter]).attr("data-parentID") == $(section11[counter]).attr("data-parentID") && innerCounter > counter) {
    //            right.addClass('naviActive');
    //            break;
    //        }
    //        else {
    //            right.removeClass('naviActive');
    //        }
    //    }
    //}
    //for (var counter = 0; counter < section111.length; counter++) {
    //    var right = $(section111[counter]).find('.nav-right');
    //    for (var innerCounter = 0; innerCounter < section111.length; innerCounter++) {
    //        // Check if there is any previous element on same level with same parent
    //        if ($(section111[innerCounter]).attr("data-parentID") == $(section111[counter]).attr("data-parentID") && innerCounter > counter) {
    //            right.addClass('naviActive');
    //            break;
    //        } else {
    //            right.removeClass('naviActive');
    //        }
    //    }
    //}
    //for (var counter = 0; counter < section1111.length; counter++) {
    //    var right = $(section1111[counter]).find('.nav-right');
    //    for (var innerCounter = 0; innerCounter < section1111.length; innerCounter++) {
    //        // Check if there is any previous element on same level with same parent
    //        if ($(section1111[innerCounter]).attr("data-parentID") == $(section1111[counter]).attr("data-parentID") && innerCounter > counter) {
    //            right.addClass('naviActive');
    //            break;
    //        } else {
    //            right.removeClass('naviActive');
    //        }
    //    }
    //}
  };

  /************************** Load ***************************/
  // get a list of all elements in the previous chapter along with length, also work out for newly created chapter
  my.loadSnippetStructureForNewChapter = function (callback) {
    var $this = this;
    $this.isLoaded = false;
    $this.jsonPost(
      $this.serviceUrl.loadSnippetStructureForNewChapter,
      {
        id: $this.documentId,
        chapterId: $this.chapterId
      }).done(function(data) {
        $this.snippetStructure = data;
        callback(data);
      });
  };
  my.loadChapter = function (option, isNextChapter) {
    var $this = this;
    $this.isLoaded = false;
    $this.showLoader(window.localize["Authoring"]["Loader_LoadChapter"]);
    $this.jsonPost(
      (isNextChapter) ? $this.serviceUrl.getNextChapterByService : $this.serviceUrl.getChapterByService,
      JSON.stringify({
        "id": $this.documentId,
        "chapterId": $this.chapterId,
        "dir": option
      })
    ).done(function(idata) {
      var idata;
      $this.showLoader(window.localize["Authoring"]["Loader_PrepareEditor"]);
      if (isNextChapter)
        idata = JSON.parse(idata.GetNextChapterResult);
      else
        idata = JSON.parse(idata.LoadChapterResult);
      $this._clearEditor();
      $('div#body').hide();
      if (idata.snippets.length > 0) {
        for (var counter = 0; counter < idata.snippets.length; counter++) {
          if (counter == 0)
            $this.loadCompleteChapter("chapter", idata.snippets[counter], 0);
          else
            $this.loadCompleteChapter(idata.snippets[counter].widgetName, idata.snippets[counter], idata.snippets[counter - 1]);
        }
      }
      if (!my.proofMode) {
        $this.setChapterButtons();
      }
      $this.AutoSetSnippetEditorButtons();
      $this.snippetId = idata.snippets[idata.snippets.length - 1].snippetId;
      $this.caretManager.setCaretToEnd($("sn_block-" + $this.snippetId));
      $this.refreshSnippetCounter("listingEditor");
      $this.refreshSnippetCounter("tableEditor");
      $this.refreshSnippetCounter("editableImage");
      $('div#body').show();
      $this.resize();
      $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
      $this.isLoaded = true;
      $this.hideLoader();
      return;
      // window.location = $this.locationUrl.chapterLocation + '?chapterId=' + idata.cid;
    });
  };
  /************************** Load ***************************/
  // get a list of all elements in the chapter along with length
  my.loadSnippetStructure = function (callback) {
    // get a list of all elements in the chapter along with length
    var $this = this;
    $this.isLoaded = false;
    $.ajax({
      url: $this.serviceUrl.loadSnippetStructure,
      data: {
        id: $this.documentId,
        chapterId: $this.chapterId
      },
      type: "GET",
      cache: false,
      dataType: "json",
      success: function (data) {
        $this.snippetStructure = data;
        callback(data);
      }
    });
  };
  // get a list of all chapters in the document
  my.loadAllChapterIds = function () {
    var $this = this;
    $.ajax({
      //url: $this.serviceUrl.getAllChapterIds,getAllChapterIdsByService
      url: $this.serviceUrl.getAllChapterIdsByService,
      data: JSON.stringify({
        "id": $this.documentId
      }),
      type: "POST",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (idata) {

        for (var counter in idata.GetAllChapterIdsResult)
          $this.chapterIdsList.push(idata.GetAllChapterIdsResult[counter]);

        $this.setChapterButtons();
      },
      error: function (data) {
        console.log(data);
      }
    });
  };
  my.loadSnippets = function () {
    var $this = this;    
    // Caller can decide loading any number of elements dynamically
    $this.showLoader(window.localize["Authoring"]["Loader_PrepareEditor"]);
    // prepare editor globally
    $this.initSnippetContainer();
    $.each($('.snippet-block'), function (idx, e) {
      // activate all snippets as editable
      $this.initSnippetEditor(e);
    });
    $this.scrollManager.resetScrollPosition();
    $this.resize();
    $this.AutoSetSnippetEditorButtons();
    $this.loadAllChapterIds();
    $this.hideLoader();
  };
  // Loading whole chapter
  my.loadCompleteChapter = function (type, snippet, prevSnippetId) {
    var $this = this;
    var widget = new _SingleWidget();
    widget.serviceUrl = $this.baseUrl;
    if (type == "chapter") {
      widget.snippetObj = snippet;
      $this._appendElementForChapter(widget.getWidgetHtml(), $("#sn_block-" + snippet.snippetId), type); // id is current element before inserting 
      $this.chapterId = snippet.snippetId;
    } else {
      $this.snippetId = snippet.snippetId;
      widget.snippetObj = snippet;
      if (type == "Image") {
        widget.serviceUrl = $this.serviceUrl.imagePath;
      }
      var $elm = $this._appendElement(widget.getWidgetHtml(), $("#sn_block-" + prevSnippetId.snippetId)); // id is current element before inserting 
      setTimeout(function() { $this.initSnippetEditor($elm); }, 1);
    }
  };

  /************************** Save ***************************/
  my.deferredSave = function (elm) {
    var $this = this;    
    if (elm.isSaved) return;    
    elm.isSaved = true;
    var id = $(elm).data('item');
    var hl = $('.editable_highlight_haschanged[data-item=' + id + ']');
    var ho = $('.editable_highlight[data-item=' + id + ']');
    hl.height(ho.height());
    $this.setStatusBar(window.localize["Authoring"]["Document_Changed"]);
    if (!hl.is(':visible')) {
      setTimeout(function () {
        console.info("Save Called");
        $this._save($(elm));
        hl.hide();
        elm.isSaved = false;
      }, 2000);
    }
    hl.show();
  };
  my.saveSnippet = function (callback) {
    var $this = this;
    console.info("SaveSnippet Called");
    if ($this.snippetId === undefined) return;
    var elm = $('.saveableByTexxtoor[data-item=' + $this.snippetId + ']');
    $this._save(elm, callback);
  };
  my._save = function (e, callback) {
    var $this = this;
    console.info("Save Internal Called");
    var elm = $(e);
    var content;
    $(".editor, .CodeMirror, .editableSection").removeHighlight();
    $this.scrollManager.saveScroll();
    switch (elm.data('editor')) {
      case "HtmlEditor":
      case "TableEditor":
        content = EDITOR.Core.activeEditable.getContents();
        break;
      case "ListingEditor":
        if ($(elm).data('CodeMirror') == null) {
          return false;
        }
        content = $(elm).data('CodeMirror').getValue();
        break;
      case "ImageEditor":
        content = $(elm).find('input').val();
        break;
      default:
        content = elm.html();
        break;
    }
    var id = elm.attr('data-item');
    $(".fakeDiv").remove();
    if (!id) {
      return false;
    }
    $.ajax({
      url: $this.serviceUrl.saveContentByService,
      data: JSON.stringify({
        'id': id,
        'documentId': $this.documentId,
        'content': content,
        'form': $('#sn_block-' + id + ' :input').serialize()
      }),
      type: "POST",
      cache: false,
      dataType: "json",
      contentType: "application/json",
      success: function (data) {
        if (data.sectionRefresh) {
          $('#chapterTitleInToolSet').html(data.sectionRefresh);
        }
        var today = new Date();
        $this.setStatusBar(window.localize["Authoring"]["Status_SavedAt"] + today.toLocaleTimeString());
        if ($(".fr-container").is(":visible")) {
          $this.setHighlightStyle();
        }
        if (callback != undefined && $.isFunction(callback)) {
          callback();
        }
      },
      error: function (data) {
        $this.setStatusBar(window.localize["Authoring"]["Error_SaveContent"] + data.msg);
      }
    });
    return true;
  };


  /********************* Snippet Mngmt *************************/

  // insert an element after the current snippet physically in the dom
  // append an element after the last element in editor container
  my._appendElement = function (snippetBlock, insertAfter) {
    var $this = this;
    var $elm = $(snippetBlock);
    if (!insertAfter) {
      $elm.appendTo($this.snippetContainer);
    } else {
      $elm.insertAfter($(insertAfter));
    }
    if (!$("#leftbar").is(":visible")) {
      $(".flowButton").hide();
    }
    if (!$("#rightbar").is(":visible")) {
      $(".naviButton").hide();
    }
    $this.setSnippetId($elm.data('item'));
    //$this.initSnippetEditor($elm);
    if ($(".fr-container").is(":visible"))
      $this.setSearchOption();
    $this.setStatusBar("&nbsp;");
    var $ed = $elm.find('.editor');
    if ($ed.html() == "<p></p>")
      $ed.html("<p>&nbsp;</p>");

    $this.caretManager.setEditorCaret();
    return $elm;
  };

  // If the the type of the element is chapter then insert it at the end of the document
  // append an element after the last element in editor container
  my._appendElementForChapter = function (snippetBlock, insertAfter, type) {
    var $this = this;
    if (type == "chapter") {
      // Insert Chapter at the end of the document elements
      //if (insertAfter)
      //    alert("Warning! You cannot insert a chapter inside another chapter. This chapter will automatically added to the end of the document");
      $(snippetBlock).appendTo($this.snippetContainer);
    } else {
      if (!insertAfter) {
        $(snippetBlock).appendTo($this.snippetContainer);
      } else {
        $(snippetBlock).insertAfter($(insertAfter));
      }
    }
    if (!$("#leftbar").is(":visible")) {
      $(".flowButton").hide();
    }
    if (!$("#rightbar").is(":visible")) {
      $(".naviButton").hide();
    }
    $this.setSnippetId($(snippetBlock).data('item'));
    $this.initSnippetEditor(snippetBlock);
    if ($(".fr-container").is(":visible"))
      $this.setSearchOption();
    $this.setStatusBar("&nbsp;");
    $this.caretManager.setEditorCaret();
  };

  // send a snippet to server storage for later access, includes a delete operation
  my.orphan = function () {
    var $this = this;
    var sid = $this.snippetId;
    $.ajax({
      url: $this.serviceUrl.orphanSnippet,
      data: {
        id: sid,
        delChildren: $('#incChildren').is(':checked')
      },
      type: "GET",
      cache: false,
      dataType: "json",
      success: function (data) {
        data = JSON.parse(data);
        var href = $("#tocpane li:last").prev().find("a").attr('href');
        $this.updateTableOfContent();
        $this.setStatusBar(data.msg);
        // update content store and remove physically
        $this.loadSnippetStructure(function (idata) {
          $('#sn_block-' + sid).fadeOut(200, function () {
            $('#sn_block-' + sid).remove();
          });
          if ($this.snippetId == $this.chapterId) {
            $this.setCurrentChapter(href);
            $this.scrollManager.setScrollPosition(0);
            return;
          }
          for (var i = 0; i < data.children.length; i++) {
            $('#sn_block-' + data.children[i]).fadeOut(200, function () {
              $(this).remove();
            });
          }
          $this._replaceSectionsAfter(data.snippetsData);
          $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
          $this.loadOrphanedSnippets();
        });

      },
      error: function (data) {
        $this.setStatusBar(window.localize["Authoring"]["Error_NoDelete"] + data.msg);
        // let's reappear to show user that something went wrong
      }
    });
  };
  my.loadOrphanedSnippets = function () {
    var $this = this;
    var ul = $('#orphanedSnippets');
    ul.empty();
    $.ajax({
      //url: $this.serviceUrl.orphanedSnippets,
      url: $this.serviceUrl.orphanedSnippetsByService,
      type: "GET",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (data) {
        data = JSON.parse(data);
        for (var i = 0; i < data.length; i++) {
          // <li class="ribbon-button" data-command="move" data-action="i" data-option="0">
          var li = $('<li>');
          li.html(data[i].Name);
          li.addClass('ribbon-button');
          li.appendTo(ul);
          li.data('option', data[i].Id);
          li.data('command', 'move');
          li.data('action', 'i');
        }
      },
      error: function (data) {
        $this.setStatusBar(window.localize["Authoring"]["Error_NoOrphan"] + data.msg);
      }
    });
  };
  my.insertOrphanedSnippet = function (id) {
    var $this = this;
    var sid = $this.snippetId;
    var widget = new _SingleWidget();
    $.ajax({
      url: $this.serviceUrl.insertOrphanedSnippet,
      // int documentId, int chapterId, int id, int afterSnippet
      data: {
        documentId: $this.documentId, 
        chapterId: $this.chapterId, 
        id: id,
        afterSnippet: sid
      },
      type: "GET",
      cache: false,
      dataType: "json",
      success: function (idata) {
        widget.snippetObj = idata.snippet;
        $this.snippetId = idata.id;
        var type = idata.type.toLowerCase();
        if (type == "listing")
          widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("listingEditor");
        if (type == "table")
          widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("tableEditor");
        if (type == "image")
          widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("editableImage");
        widget.serviceUrl = $this.serviceUrl.imagePath;
        var $elm = $this._appendElement(widget.getWidgetHtml(), $("#sn_block-" + id)); // id is current element before inserting 
        setTimeout(function () { $this.initSnippetEditor($elm); }, 1);
        $this.resize();
        $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
      },
      error: function (data) {
        $this.setStatusBar(window.localize["Authoring"]["Error_NoOrphan"] + data.msg);
      }
    });
  };
  // merge to snippets (plain text only)
  my.merge = function () {
    var $this = this;
    if ($('.snippet-block[id="sn_block-' + $this.snippetId + '"] .editableText')) {
      $this.move('g', $this.snippetId);
    }
  };
  // move a snippet to next or prev location or drop elsewhere
  my.move = function (action, sid, dropId) {
    var $this = this;
    if ($this.isMoveBusy) return;
    // Stop all other moves until this move is completed
    $this.isMoveBusy = true;
    if (sid == undefined) {
      sid = $this.snippetId;
    }
    if (sid == undefined) return;
    $this.setStatusBar(window.localize["Authoring"]["Moving_Snippet"]);
    $this.scrollManager.saveScroll();
    $this.setSnippetId(sid);
    var elm = $('.snippet-block[id="sn_block-' + $this.snippetId + '"]');
    var from; // remove element from dom preserving content
    switch (action) {
      case 'd':
        // down step
        var next = elm.next('.snippet-block');
        // Check if there is any last element where the user is trying to shift the snippet
        if ($(next).attr("id") == undefined) {
          alert("Warning! this move is not allowed");
          // Alow moves again and reset the snippet
          $this.AutoSetSnippetEditorButtons();
          // Set cursor again
          $this.caretManager.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
          return;
        }
        from = elm.detach();
        from.insertAfter(next);
        break;
      case 'u':
        // up step
        var prev = elm.prev('.snippet-block');
        console.log($(prev).attr("id").replace("sn_block-", ""));
        // Check if user is trying to move any snippet to above chapter
        if ($this.chapterId == $(prev).attr("id").replace("sn_block-", "")) {
          alert("Warning! this move is not allowed");
          // Alow moves again and reset the snippet
          $this.AutoSetSnippetEditorButtons();
          // Set cursor again
          $this.caretManager.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
          return;
        }
        from = elm.detach();
        from.insertBefore(prev);
        break;
      case 'm':
        // drag move target
        var trgt = $('#sn_block-' + dropId);
        // there must be some dropid after which the moved item will be insterted, also the drop id must not be as moved element id.
        if (dropId == undefined || dropId == sid) {
          alert("Warning! this move is not allowed");

          // Alow moves again and reset the snippet
          $this.AutoSetSnippetEditorButtons();
          // Set cursor again
          $this.caretManager.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
          return;
        }
        //if ($(elm).find(".editableSection") != undefined && $(elm).find(".editableSection").length > 0) {
        //    alert("Warning! this move is not allowed");
        //    // Alow moves again and reset the snippet
        //    $this.AutoSetSnippetEditorButtons();
        //    // Set cursor again
        //    $this.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
        //    return;
        //}
        // Check if the section is being moved
        if ($(elm).find(".editableSection") != undefined && $(elm).find(".editableSection").length > 0) {
          this.isMoveBusy = false;
          $this.changeLevel("s", $this.snippetId, dropId);
          return;
        }
        // This code will help if it is required to block user to move the snippet under the chapter
        //if ($this.chapterId == $(trgt).attr("id").replace("sn_block-", "")) {
        //    alert("Warning! this move is not allowed");
        //    $this.AutoSetSnippetEditorButtons();
        //    // Alow moves again
        //    $this.isMoveBusy = false;
        //    // Set cursor again
        //    $this.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
        //    return;
        //}
        from = elm.detach();
        from.insertAfter(trgt);
        break;
      case 'g':
        var merge = elm.next('.snippet-block');
        merge.remove();
        break;
    }
    $.ajax({
      //url: $this.serviceUrl.move,
      url: $this.serviceUrl.moveByService,
      data: JSON.stringify({
        id: $this.documentId,
        chapterId: $this.chapterId,
        sectionId: $this.snippetId,
        dropId: dropId,
        move: action,
        withChildren: $('#movewithchildren').val()
      }),
      type: "POST",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (data) {
        // only if server side was successfully changed we refresh view
        //$this.loadSnippetStructure(function () {
        //$this._replaceSectionsAfter(data.snippetsData);
        $this.scrollManager.scrollToSnippet($this.snippetId);
        $this.setStatusBar("Snippet moved");
        // Make moving available again
        $this.AutoSetSnippetEditorButtons();
        // Setting focus on appropriate on moving item
        $this.caretManager.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);
        //setTimeout(function () {
        //    // Make moving available again
        //    $this.AutoSetSnippetEditorButtons();
        //    // Setting focus on appropriate on moving item
        //    $this.setCaretToEnd($('.snippet-block[id="sn_block-' + $this.snippetId + '"] div.editor'), true);

        //}, 2200);
        //});
      },
      error: function (data) {
        $this.setStatusBar(data);
      }
    });
  };
  my.changeLevel = function (action, sid, dropId) {
    if (this.isMoveBusy)
      return;
    console.log("is move busy in change level " + this.isMoveBusy);
    this.isMoveBusy = true;
    var $this = this;
    if (sid == undefined) {
      sid = $this.snippetId;
    }
    if ($this.snippetId == undefined) return;
    $this.setSnippetId(sid);
    $.ajax({
      url: $this.serviceUrl.moveByService,
      type: "POST",
      data: JSON.stringify({
        id: $this.documentId,
        chapterId: $this.chapterId,
        sectionId: sid,
        dropId: dropId,
        move: action
      }),
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (idata) {
        var data = JSON.parse(idata.MoveResult);
        // only if server side was successfully changed we refresh view
        if (data.msg != undefined) {
          alert(data.msg);
          $this.AutoSetSnippetEditorButtons();
          return;
        }
        $this.loadSnippetStructure(function () {
          // element has still same Id, so we just call and replace
          $this._replaceSectionsAfter(data.snippetsData);
          setTimeout(function () {
            $this.AutoSetSnippetEditorButtons();
            // Setting focus on appropriate on moving item

            $this.caretManager.setCaretToEnd($('#sn_block-' + $this.snippetId + ' div.editor'), false);
          }, 2000);
        });
        if (dropId > 0)
          $this._refreshChangedSections(data.snippetsData, dropId);
      },
      error: function (data) {
        $this.setStatusBar(data);
        // TODO: Refresh
      }
    });
  };
  // move section snippets to their right place after drag and drop
  my._refreshChangedSections = function (sids, dropid) {
    var $this = this;
    // check if there are some snippets to be refreshed
    if (sids.length > 0 && dropid > 0) {
      for (var i = 0; i < sids.length; i++) {
        if (i == 0) {
          from = $('#sn_block-' + sids[i]).detach();
          from.insertAfter($('#sn_block-' + dropid));
        } else {
          from = $('#sn_block-' + sids[i]).detach();
          from.insertAfter($('#sn_block-' + sids[i - 1]));
        }
      }
    }
  };
  // after level operations replace all sections, one by one, after the current element, if necessary
  my._replaceSectionsAfter = function (sids) {
    var $this = this;
    $this.contentData.length = 0;
    // for all section after current we ask server whether a replacement is necessary
    var calls = [];
    var id;
    // store all ajax calls in an array
    for (var i = 0; i < sids.length; i++) {
      id = sids[i];
      calls.push(
        $.ajax({
          url: $this.serviceUrl.getSnippet,
          data: { id: id.snippetId },
          type: "GET",
          cache: false,
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          success: function (idata) {
            var data = JSON.parse(idata);
            $this.contentData[data.snippetId] = data;
          }
        })
      );
    }
    // now call all at once and wait for completion
    $.when.apply($, calls).done(function () {
      for (var j = 0; j < sids.length; j++) {
        id = sids[j];
        // detect the previous block so we have an anchor after removing the current to add it again
        var idata = $this.contentData[id.snippetId];
        var widget = new _SingleWidget();
        widget.snippetObj = idata;
        $('#sn_block-' + idata.snippetId).replace(widget.getWidgetHtml()); // replace is plugin defined in authorroom.js
        $this._initSnippetEditorButtons($('#sn_block-' + id));
      }
      $this.contentData.length = 0;
    });
  };
  my.setSnippetId = function (id) {
    if (isNaN(id)) return;
    var $this = this;
    if ($this.snippetId == id) return;
    $this.snippetId = id;
    $this.jsonObj = null;
    $this.jsonObj = $.parseJSON($("#sn_block-" + id + " input[name='properties']").val());
    $this.setStatusBar(id, true);
    $this.highlightSnippet();    
  };
  my.getAllSectionsAfterLevel = function (objSnippet, level) {
    var sectionSnippets = [];
    var isSnippetFound = false;
    $.each($('.snippet-block'), function (idx, e) {
      if (isSnippetFound) {
        var currentelementLevel = this.getSnippetLevel(e);
        if (currentelementLevel == level) {
          sectionSnippets.push(e);
        }
      }
      // First let the loop work to find the current snippet
      if ($(e) == $(objSnippet)) {
        isSnippetFound = true;
      }
    });
  };
  my.getSnippetLevel = function (snippetObj) {
    return (parseInt($(snippetObj).attr("data-levelid")) - 1);
  };
  my.getNextSnippet = function (snippetObj) {
    var isSnippetFound = false;
    $.each($('.snippet-block'), function (idx, e) {
      if (isSnippetFound) {
        return (e);
      }
      // First let the loop work to find the current snippet
      if ($(e) == $(snippetObj)) {
        isSnippetFound = true;
      }
    });
  };
  my.removeAndGetNewSnippet = function () {
    var $this = this;
    var index = $this.chapterIdsList.indexOf($this.chapterId);
    var newChapterId = 0;
    $this.chapterIdsList.splice(index, 1);
    var difference = ($this.chapterIdsList.length - 1) - index;
    if (difference < 1) {
      newChapterId = $this.chapterIdsList[$this.chapterIdsList.length - 1];
    } else {
      newChapterId = $this.chapterIdsList[index];
    }
    $this.chapterId = newChapterId;
    $this.loadChapter("1", false);
  };
  my.getNumberOfSnippet = function (type) {
    var $this = this;
    var cnt = 1;
    $.each($('.snippet-block'), function (idx, e) {
      if ($(e).next("." + type + "").length > 0) {
        cnt++;
      }
    });
    return cnt;
  };
  my.refreshSnippetCounter = function (type) {
    var $this = this;
    var cnt = 1;
    $.each($('.snippet-block'), function (idx, e) {
      if ($(e).find("." + type + "").length > 0) {        
        $(e).find('.snippetTitle span.snippetCounter').text(cnt++);
      }
    });
  };

  my.insertCommand = function (type, variation, data) {
    var $this = this;    
    var widget = new _SingleWidget();
    var oldChapterId = $this.chapterId;
    my.scrollManager.saveScroll();
    $this.saveSnippet();
    var id = $this.snippetId;
    if (($("#sn_block-" + id + " .editor").html() == "<p></p>" || $("#sn_block-" + id + " .editor").html() == "<p>&nbsp;</p>")) {
      alert("Warning! You cannot insert a new text snippet as you have an empty snippet left.");
      $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
      $this.caretManager.setEditorCaret();
      return;
    }
    $this.setStatusBar("Snippet inserting");
    $.ajax({
      url: $this.serviceUrl.insertSnippetByService,
      data: JSON.stringify({
        documentId: $this.documentId,
        chapterId: $this.chapterId,
        id: id,
        type: type,
        variation: variation,
        data: data
      }),
      type: "POST",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      //contentType: "xml",
      success: function (jsonData) {
        //var idata = jsonData.InsertSnippetResult.replace(/"/g, '');
        var idata = JSON.parse(jsonData.InsertSnippetResult);
        if (idata.id == 0) {
          alert(idata.msg);
          return;
        }
        $this.setStatusBar("Snippet Inserted..." + idata.msg);
        var t = new Date().now;
        widget.snippetObj = idata.snippet;
        if (type == "chapter") {          
          $this._clearEditor();
          $this.chapterId = idata.relocateTo;
          $this.updateTableOfContent();
          $this._appendElementForChapter(widget.getWidgetHtml(), $("#sn_block-" + id), type); // id is current element before inserting 
          $this.resize();
          $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
          var indexOfLastChapter = $this.chapterIdsList.indexOf(oldChapterId);
          if (indexOfLastChapter == $this.chapterIdsList.length - 1)
            $this.chapterIdsList.push($this.chapterId);
          else
            $this.chapterIdsList.splice(indexOfLastChapter + 1, 0, $this.chapterId);
          $this.setChapterButtons();
        } else {
          $this.snippetId = idata.id;
          if (type == "listing")
            widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("listingEditor");
          if (type == "table")
            widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("tableEditor");
          if (type == "img")
            widget.snippetObj.snippetCounter = $this.getNumberOfSnippet("editableImage");
          widget.serviceUrl = $this.serviceUrl.imagePath;
          var $elm = $this._appendElement(widget.getWidgetHtml(), $("#sn_block-" + id)); // id is current element before inserting 
          setTimeout(function () { $this.initSnippetEditor($elm); }, 1);
          $this.resize();
          $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
        }
        $this.AutoSetSnippetEditorButtons();
        $this.updateWidgetTools();
        $this.undoManager.register(null, my.insertCommand, [type, variation, data, null], "Insert " + type, null, null, null, "");
      },
      error: function (idata) {
        $this.setStatusBar("Not inserting...");
        $this.hideLoader();
      }
    });
  };
  my.deleteCommand = function (sid) {
    var $this = this;
    var isChapterDeleted = false;
    if (sid == $this.chapterId) {
      if ($this.chapterIdsList.length <= 1) {
        alert("Your document has only one chapter, it is not allowed to delete all chapters from a document");
        return;
      } else if (!confirm("Do you want to delete this chapter? Please confirm!"))
        return;
      isChapterDeleted = true;
    } else {
      // assure that's right
      $('#sn_block-' + sid).css({
        border: '2px dotted red'
      });
      if (!confirm("Do you want to delete this element? Please confirm!")) {
        $('#sn_block-' + sid).css({
          border: ''
        });
        return;
      }
    }
    $this.scrollManager.saveScroll();
    // instantly hide for best user experience
    $this.setStatusBar("Deleting snippet");
    // move the cursor to next 
    var nxt = $('#sn_block-' + sid).next('.snippet-block');    
    if (nxt.length == 0) {
      nxt = $('#sn_block-' + sid).prev('.snippet-block');
    }
    $this.setSnippetId(nxt.data('item'));
    $.ajax({
      url: $this.serviceUrl.deleteSnippetByService,
      data: {
        id: sid,
        delChildren: $('#delChildren').is(':checked')
      },
      type: "GET",
      cache: false,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (idata) {
        if (idata == null) {
          alert("An error occured deleting this snippet. Refresh (F5) and try again.");
          return;
        }
        var data = JSON.parse(idata);
        var href = $("#tocpane li:last").prev().find("a").attr('href');
        $this.updateTableOfContent();
        $this.setStatusBar(data.msg);
        // update content store and remove physically
        //$this.loadSnippetStructure(function (idata) {
        if (isChapterDeleted) {
          $this.removeAndGetNewSnippet();
          //$this.setCurrentChapter(href);
          //$this.scrollManager.setScrollPosition(0);
          return;
        } else {
          $('#sn_block-' + sid).fadeOut(200, function () {
            $('#sn_block-' + sid).remove();
            for (var i = 0; i < data.children.length; i++) {
              $('#sn_block-' + data.children[i]).remove();
            }
            $this.AutoSetSnippetEditorButtons();
            $this.scrollManager.setScrollPosition($this.scrollManager.scrollPosition);
            $this.caretManager.setEditorCaret();
            $this.refreshSnippetCounter("listingEditor");
            $this.refreshSnippetCounter("tableEditor");
            $this.refreshSnippetCounter("editableImage");
          });
        }
        var type = $('#sn_block-' + sid).find('.editableByTexxtoor').data('editor');
        $this.undoManager.register(null, my.insertCommand, [type, null, null], "Delete " + type);
      },
      error: function (data) {
        $this.setStatusBar("Not deleted..." + data.msg);
        // let's reappear to show user that something went wrong
      }
    });
  };
  my.redirectToSnippet = function (chapter, id) {
    var $this = this;
    if ($this.chapterId == chapter) {
      $this.setSnippetId(id);
      $this.caretManager.setEditorCaret();
    } else {
      window.location.href = $this.locationUrl.chapterLocation + "?chapterId=" + chapter;
    }
  };

  my.highlightText = function () {
    var $this = this;
    $this.highlights = null;
    $this.hIndex = 0;
    if ($this.snippetId == null)
      $this.setSnippetId(parseInt($(".snippet-block:first").find(".editableByTexxtoor").attr("data-item")));

    $this.fr.find("#find input").bind("keyup paste", function () {
      var i = this;
      clearTimeout($this.timeoutId);
      $this.timeoutId = setTimeout(function () {
        $this.highlights = $this.search(i);
        $this.hIndex = 0;
        $this.activeSearchElement = { snippetId: $this.snippetId, position: $this.hIndex };
        if ($this.highlights == undefined) return;
        if ($this.highlights.length == 0) {
          $this.searchSnippetId($("#find input").val());
          return;
        }
        $this.setSnippetId(parseInt($("span.highlight:first").parents(".editableByTexxtoor").attr("data-item")));
        $this.activeSearchElement = { snippetId: $this.snippetId, position: $this.hIndex };
        $this.setHighlightStyle();
      }, 200);
    });
    $this.fr.find("#r-replace").bind('click', function () {
      if ($(".loader-layout").is(":visible") && !$this.isLoaded) return;
      if ($this.highlights.length == 0) return;
      $($this.highlights[$this.hIndex]).text($this.fr.find("#replace input").val());
      var e = $($this.highlights[$this.hIndex]).closest(".editor, .CodeMirror").parent();
      e = e.length > 0 ? e : $($this.highlights[$this.hIndex]).closest('.editableSection');
      $this.deferredSave(e);
      $(".editor, .CodeMirror, .editableSection").removeHighlight();
    });
    $this.fr.find("#f-next").bind('click', function () {
      if ($(".loader-layout").is(":visible")) return;
      // $this.direction = 0;
      if (!$('#sn_block-' + $this.activeSearchElement.snippetId).is(":visible")) {
        $this.scrollManager.scrollToSnippet($this.activeSearchElement.snippetId);
        return;
      }
      $this.direction = 1;
      if ($this.hIndex < $('#sn_block-' + $this.activeSearchElement.snippetId).find("span.highlight").length - 1) {
        $this.hIndex++;
        $this.activeSearchElement.position = $this.hIndex;
        $this.setHighlightStyle();
      } else {
        var highlights = $('#sn_block-' + $this.activeSearchElement.snippetId).nextAll(".snippet-block").find("span.highlight");
        if (highlights.length != 0) {
          $this.hIndex = 0;
          $this.activeSearchElement.position = $this.hIndex;
          $this.activeSearchElement.snippetId = $(highlights[0]).parents(".editableByTexxtoor").attr("data-item");
          $this.setHighlightStyle();
        } else {
          $this.searchSnippetId($("#find input").val());
        }
      }
    });
    $this.fr.find("#f-prev").bind('click', function () {
      if ($(".loader-layout").is(":visible") && !$this.isLoaded) return;
      // $this.direction = 0;
      if (!$('#sn_block-' + $this.activeSearchElement.snippetId).is(":visible")) {
        $this.scrollManager.scrollToSnippet($this.activeSearchElement.snippetId);
        return;
      }
      $this.direction = -1;
      if ($this.hIndex > 0) {
        $this.hIndex--;
        $this.activeSearchElement.position = $this.hIndex;
        $this.setHighlightStyle();
      } else {
        var highlights = $('#sn_block-' + $this.activeSearchElement.snippetId).prevAll(".snippet-block").find("span.highlight");
        if (highlights.length != 0) {
          $this.hIndex = $(highlights[0]).parents(".snippet-block").find("span.highlight").length - 1;
          $this.activeSearchElement.position = $this.hIndex;
          $this.activeSearchElement.snippetId = $(highlights[0]).parents(".editableByTexxtoor").attr("data-item");
          $this.setHighlightStyle();
        } else {
          $this.searchSnippetId($("#find input").val());
        }
      }
    });
    $this.fr.find("#f-close").bind('click', function () {
      $(".editor, .CodeMirror, .editableSection").removeHighlight();
      $this.fr.slideUp(100, function () {
        $this.clearFR();
      });
    });
  };
  my.setHighlightStyle = function () {
    var $this = this;
    $this.highlights = $this.search($("#find input"));
    if (typeof $this.highlights === 'undefined' || $this.highlights == null || $this.highlights.length == 0) {
      return;
    }
    $this.direction = 0;
    $($this.highlights).css('background', 'yellow').removeClass("active");
    var active = $('#sn_block-' + $this.activeSearchElement.snippetId).find("span.highlight").eq($this.activeSearchElement.position);
    if (active.length == 0) return;
    active.css('background', '#FF7200').addClass("active");
    $this.setSnippetId(parseInt($("span.highlight.active").parents(".editableByTexxtoor").attr("data-item")));
    if (active.position().top + active.height() + active.parents(".snippet-block").position().top > $this.snippetContainer.height()) {
      $this.direction = 0;
      $this.pageDown();
    } else if (active.parents(".snippet-block").position().top + active.position().top < 0) {
      $this.direction = 0;
      $this.pageUp();
    }
  };

  // resize of window needs the content container to be set the new height to make browser scrollbars working
  my.clearFR = function () {
    var $this = this;
    $(".editor, .CodeMirror, .editableSection").removeHighlight();
    $this.fr = $(".fr-wrapper");
    $this.fr.find("#find input").val('');
    $this.fr.find("#replace input").val('');
    $this.fr.find("#find input").unbind("keyup");
    $this.fr.find("#f-close, #f-next, #f-prev, #r-replace").unbind("click");
  };

  /*# Listing #*/
  my.setListingEditorOption = function (cmd, args) {
    var $this = this;
    $('input[name=' + cmd + '-' + $this.snippetId + ']').val(args);
    var elm = $(".editableByTexxtoor[data-item='" + $this.snippetId + "']");
    var editor = $(elm).data('CodeMirror');
    switch (cmd) {
      case "language":
        editor.setOption('mode', args);
        $('#sn_block-' + $this.snippetId + ' input[name=syntaxhighlight]').val(true);
        $("#syntaxhighlight").attr("checked", "checked");
        break;
      case "syntaxhighlight":
        editor.setOption('mode', args == false ? 'text' : $('#sn_block-' + $this.snippetIdinput + 'input[name=language]').val());
        break;
      case "linenumbers":
        editor.setOption('lineNumbers', args);
        ;
        break;
    }
    $this.saveSnippet();
  };

  my.getQueryStringValue = function (paramName) {
    paramName = paramName.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + paramName + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
  };
  my.pasteHtmlAtCaret = function (html, r) {
    var editable = $("#sn_block-" + my.snippetId).find(".editableByTexxtoor");
    if (editable.length == 1) {
      Editor.Paste
    }
  };

  return my;
}(AUTHOR || {}));