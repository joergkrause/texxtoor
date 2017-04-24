var AUTHOR = (function (my) {

  my.initializeEditor = function (baseUrl, docId, chapterId, editorTitle, uploadServiceUrl, ribbonUrl) {
    this.baseUrl = baseUrl;
    this.documentId = docId;
    this.chapterId = chapterId;
    this.crop = new ImageCrop(this);
    this.editorTitle = editorTitle;
    this.uploadServiceUrl = uploadServiceUrl;
    this.serviceUrl = {
      outerCSS: ['/Content/css/author.css', '/Scripts/author/jqmath/jqmath-0.2.0.css', 'http://fonts.googleapis.com/css?family=UnifrakturMaguntia'],

      loadSnippetStructure: this.baseUrl + 'GetContentStructure',
      loadSnippetStructureForNewChapter: this.baseUrl + 'GetContentStructureForNewChapter',// This event to be called when a new chapter is added to the document

      getSnippet: this.baseUrl + 'GetSnippet',
      getNextChapterByService: this.baseUrl + 'GetNextChapter',
      getChapterByService: this.baseUrl + 'LoadChapter',
      getAllChapterIdsByService: this.baseUrl + 'GetAllChapterIds', // Getting the information of all chapters in the document

      insertSnippetByService: this.baseUrl + 'InsertSnippet',
      deleteSnippetByService: this.baseUrl + 'DeleteSnippet',

      moveByService: this.baseUrl + 'Move',

      searchSnippetId: this.baseUrl + 'SearchSnippetId',

      orphanSnippet: this.baseUrl + 'OrphanSnippet',
      orphanedSnippetsByService: this.baseUrl + 'OrphanedSnippets',
      insertOrphanedSnippet: this.baseUrl + 'InsertOrphanedSnippet',

      getSemanticLists: this.baseUrl + 'SemanticLists',
      getThumbnails: this.baseUrl + 'GetThumbnails',
      imagePath: this.baseUrl + 'GetImage',
      getSidebarType: this.baseUrl + 'GetSidebarType',
      imageUploadByService: this.uploadServiceUrl + 'UploadImage',

      loadComments: this.baseUrl + 'LoadComments',
      saveComment: this.baseUrl + 'SaveComment',

      saveContent: this.baseUrl + 'SaveContent',
      saveContentByService: this.baseUrl + 'SaveContent',
      saveReorganizedTree: this.baseUrl + 'SaveReorganizedTree',

      tableOfContent: this.baseUrl + 'Toc',
      getDocumentProperties: this.baseUrl + 'GetDocumentProperties',

      saveDialog: this.baseUrl + 'SaveDialogData',
      loadDialog: this.baseUrl + 'GetDialogData'
    };
    this.locationUrl = {
      closeLocation: "",
      htmlLocation: "",
      epubLocation: "",
      pdfLocation: "",
      publishLocation: "",
      downloadLocation: "",
      chapterLocation: ""
    };
    this.statusBar = null;
    this.snippetContainer = null;
    // invoke on document ready
    $().Ribbon({
      theme: 'office2013',
      backstage: false,
      baseUrl: ribbonUrl
    });
    $('.ribbon').on('mousedown', function () {
      // prevent moving focus from editors to ribbon
      return false;
    });
    $('.dialog-popup').dialog({ autoOpen: false, resizable: false, width: '650', height: 'auto', modal: true });
    setTimeout(function () {
      my.loadSnippetStructure(function () {
        $("ul.ribbon").show();
        $("ul.menu").show();
        my.loadSnippets(false);
        my.loadChapter("", false);

      });
    }, 20);
    // load dynamic data in pane
    this.updateTableOfContent();
    this.getDocumentProperties();
    this.loadSemanticData();
    this.loadOrphanedSnippets();
    this.loadRibbonImages();
    this.loadSidebarTypes();
  };
  my._clearEditor = function () {
    $("#editor").empty();
  };
  my._attachEvents = function () {
    var $this = my;
    var isClosed;
    var faddedSnippetsForDragDrop;
    // re-implement all Ctrl-Keys to make the command invoker aware of our private undo stack
    shortcut.add("Ctrl+S", function () {
      $this.saveSnippet();
    });
    // global ENTER behavior
    $(document).on('submit', 'form.editor', function (event) {
      return false;
    });
    // special functions
    $(document).on('keydown', '.editableSection', function (event) {
      // prevent ENTER and add a paragraph instead
      if (event.which == 13) {
        $this.saveSnippet(); // Save current element before adding new one
        $this.insertCommand('text');
        return false;
      }
      return true;
    });
    $(document).on('keydown', '.editableText', function (event) {
      console.info("keydown --> .editableText");
      // prevent ENTER and add a paragraph instead
      switch (event.which) {
        case 9:
          var cmd = event.shiftKey ? 'outdent' : 'indent';
          var btn = $('[data-command=format][data-action=' + cmd + ']');
          my.formatCommand(btn, cmd);
          event.stopPropagation();
          event.preventDefault();
        case 13:
          $(this).find("div.editor p").each(function (index, el) {
            if (index > 0) {
              $(el).removeClass("editableByTexxtoor .editor > p:before");
            }
          });
          break;
        case 66: // B (Bold)
        case 73: // I (Italics)
        case 85: // U (Underline)
        case 89: // Y (Redo)
        case 90: // Z (Undo)
          if (event.ctrlKey) {
            // stop hotkeys to have this handled by ourselfes
            event.stopPropagation();
            return false;
          }
      }
      return true;
    });
    // elements in the ribbon which provide values
    $(document).on("change", ".imagePane input[name='width']", $this.setImageSize);
    $(document).on("change", ".imagePane input[name='height']", $this.setImageSize);
    // global paste
    $(document).on('paste', '.editableSection, div.editor, .editableCaption', function (e) {
      console.info("paste --> .editableSection");
      var pastedText = undefined;
      if (window.clipboardData && window.clipboardData.getData) { // IE
        pastedText = window.clipboardData.getData('Text');
      } else if (e.clipboardData && e.clipboardData.getData) {
        pastedText = e.clipboardData.getData('text/plain');
      }
      $this.pasteHtmlAtCaret(pastedText);
      e.stopPropagation();
      e.preventDefault();
      return false; // Prevent the default handler from running.
    });

    // make a snippet the active one
    $(document).on('click keyup', ".saveableByTexxtoor", function () {
      $this.setSnippetId($(this).data('item'));
      $this.updateWidgetTools();
    });
    // forward events to handle auto save for all occurences
    $(document).on('paste', ".saveableByTexxtoor", function (e) {
      if ($this.isLoaded) {
        $this.deferredSave($(e.currentTarget));
      }
    });
    $(document).on('keypress', ".saveableByTexxtoor, input.saveableCaption", function (e) {
      console.info("keypress --> input.saveableCaption");
      if ($this.isLoaded) {
        $this.deferredSave($(e.currentTarget));
      }
    });
    // cursor move strategy (allowing the user to use arrow keys to go from one snippet to another)
    $(document).on('keyup', ".canMoveCaret", function (e) {
      console.info(e.keyCode);
      if ((e.keyCode == 8 || e.keyCode == 46) && !e.shiftKey && !e.ctrlKey) {
        // Del, Bckspc
        $this.deferredSave($(e.currentTarget));
      }
      if (e.keyCode >= 37 && e.keyCode <= 40 && !e.shiftKey && !e.ctrlKey) {
        $this.caretManager.canMoveCaret(e, $(this), function () {
          var target;
          if (e.keyCode == 40 || e.keyCode == 39) {
            // down
            target = $(e.srcElement).parents('.snippet-block').nextAll('.snippet-block:first').find(".canMoveCaret");
            console.info((target == null) ? "NULL" : target.html());
            console.info(target.html());
            if (target.length > 0) {
              $this.caretManager.findCaretTarget(target, true);
            }
          } else {
            // up
            target = $(e.srcElement).parents('.snippet-block').prevAll('.snippet-block:first').find(".canMoveCaret");
            console.info((target == null) ? "NULL" : target.html());
            if (target.length > 0) {
              $this.caretManager.findCaretTarget(target, false);
            }
          }
        });
      }
      return true;
    });
    // right bar navigation and drag drop
    $(document).on('click', ".nav-up", function () {
      console.log($(this).attr("class"));
      if ($(this).attr("class").lastIndexOf("naviActive") != -1)
        $this.move('u', $(this).data('item'));
      else
        alert("Warning: this move is not allowed");
    });
    $(document).on('click', ".nav-down", function () {
      console.log($(this).attr("class"));
      if ($(this).attr("class").lastIndexOf("naviActive") != -1)
        $this.move('d', $(this).data('item'));
      else
        alert("Warning: this move is not allowed");
    });
    $(document).on('click', ".nav-left", function () {
      console.log($(this).attr("class"));
      if ($(this).attr("class").lastIndexOf("naviActive") != -1)
        $this.changeLevel('l', $(this).data('item'));
      else
        alert("Warning: this move is not allowed");
    });
    $(document).on('click', ".nav-right", function () {
      console.log($(this).attr("class"));
      if ($(this).attr("class").lastIndexOf("naviActive") != -1)
        $this.changeLevel('r', $(this).data('item'));
      else
        alert("Warning: this move is not allowed");
    });
    var rb = $('#rightbar').offset();
    $(".up-down").draggable({
      containment: [96, rb.top + 87, 96, rb.top + $('#rightbar').height()],
      snapMode: "inner",
      snap: true,
      start: function (e, u) {
        $this.setSnippetId(u.helper.data('item'));
        $(e.target).css('border-bottom', '2px solid green');
      },
      stop: function (e) {
        $(e.target).css('border-bottom', '0px');
        var afterId = $('.moveTargetSelection').data('item');
        if (afterId != $this.snippetId) {
          $this.move('m', $this.snippetId, afterId);
        } else {
          $this.AutoSetSnippetEditorButtons();
        }
      },
      drag: function (e, u) {
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
    var currentSnippetID = 0;
    $(".right-left").draggable({
      containment: [96, rb.top + 87, 96, rb.top + $('#rightbar').height()],
      snapMode: "inner",
      snap: true,
      start: function (e, u) {
        currentSnippetID = $(e.target).parent().attr("id").replace("sn_block-", "");
        faddedSnippetsForDragDrop = $this.disableDropOnChildren($(e.target).parent());
        $this.setSnippetId(u.helper.data('item'));
        $(e.target).css('border-bottom', '2px solid green');
      },
      stop: function (e) {
        $(e.target).css('border-bottom', '0px');
        console.log(currentSnippetID);
        var afterId = $('.moveTargetSelection').data('item');
        if ($this.snippetId == 0)
          $this.snippetId = currentSnippetID;
        console.log(afterId);
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
      drag: function (e, u) {
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
    $this.snippetContainer = $("#editor");
    $this.statusBar = $('#statusBar');
    $this.messageBar = $('#messageBar');
    // popup dialogs invoked from ribbon
    $(document).on('click', '.popContainer', function () {
      var current = $(this).find(".popContainerPopup");
      current.toggle('fast');
      var h = current.parent().height();
      current.height(h);
      $(".popContainerPopup").each(function () {
        if ($(this).data('item') != current.data('item')) {
          $(this).hide();
        }
      });
    });
    //$('.snippet-block').contextMenu({ menu: 'insertSnippetMenu' });
    $(window).bind('resize', function () {
      $this.resize();
    });
    // this handles links and inline elements' click behavior
    var isCtrl = false;
    $(window).bind('keyup', function () {
      isCtrl = false;
      $(".fakeDiv").remove();
    });
    $(window).bind('keydown', function (e) {
      isCtrl = e.ctrlKey;
    });
    $(document).on('mouseover', '.editor a, .editor abbr, .editor var, .editor cite, .editor dfn, .editor em, .editor span.isindex', function (e) {
      if (!isCtrl) return;
      $(".fakeDiv").remove();
      var a = $(this);
      $('<div class="fakeDiv" style="position: absolute; background: white; opacity: 0.5; cursor: pointer; z-index: 100000;">&nbsp</div>')
        .appendTo('body')
        .css({
          width: $(this).outerWidth() + 'px',
          height: $(this).outerHeight() + 'px',
          top: $(this).offset().top + 'px',
          left: $(this).offset().left + 'px'
        })
        .mouseleave(function () {
          $(this).remove();
          isCtrl = false;
        })
        .mousemove(function () { if (!isCtrl) $(this).remove(); })
        .click(function () {
          $(this).remove();
          isCtrl = false;
          var type = a.data('type');
          switch (type) {
            case 'abbreviation':
            case 'variable':
            case 'cite':
            case 'definition':
            case 'idiom':
            case 'index':
              a.replace(a.contents());
              //var v = a.data('value');
              //var t = a.data('title');
              //my.undoManager.register(a, my.insertTerm, [type, v, t, range], "Delete Term " + type);
              my.undoManager.register(a, $('#sn-' + $this.snippetId).htmlarea('undo'), null, "Delete Term " + type);
              break;
            default:
              // internal links
              if (a.data('chapter')) {
                $this.redirectToSnippet(parseInt(a.data('chapter')), parseInt(a.data('snippet')));
              }
              break;
          }
        });
    });
    // meta item handler
    $(document).on('click', 'img.metaDataText', function () {
      $('.metaHover').remove();
      $this.loadComments($(this).data('item'));
    });
    // handle the small popup on left bar items to show number of comments
    $(document).on('mouseenter', 'img.metaDataText', function () {
      $('.metaHover').remove();

      var infoDiv = $('<div>')
        .addClass('metaHover')
        .css({
          top: $(this).offset().top,
          left: $(this).offset().left + 16
        })
        .html('<h3>Comments</h3><small>Click icon for details. Click anywhere to close.</small><br/><br/>')
        .click(function () {
          $(this).remove();
        })
        .appendTo($('body'));
      var id = $(this).data('item');
      $.each([{ t: 'me', n: 'private Comments' }, { t: 'team', n: 'Team Comments' }, { t: 'reader', n: 'Reader Comments' }], function (i, e) {
        $.ajax({
          url: $this.serviceUrl.loadComments,
          data: {
            id: $this.documentId,
            snippetId: id,
            target: e.t
          },
          type: "GET",
          cache: true,
          dataType: "json",
          success: function (idata) {
            var data = JSON.parse(idata.LoadCommentsResult);
            infoDiv.html(infoDiv.html() + '<b>' + data.Comments.length + "</b> " + e.n + " <br>");
          }
        });
      });
      $('.metaHover').on('mouseleave', function () { $(this).remove(); });
    });

    // tool handler
    $(document).on('click', '.ribbon-button', function () {

      var $el = $(this);
      // command has been disabled somewhere up in the hierarchy
      if ($el.parents('[disabled]').length > 0) {
        return false;
      }
      var options = {};
      options.action = $el.data('action');
      options.option = $el.data('option');
      options.command = $el.data('command');
      switch (options.command) {
        case "orb":
          $this.showLoader(window.localize["Authoring"]["Loader_Close"]);
          setTimeout(function () {
            switch (options.action) {
              case "close":
                window.location = $this.locationUrl.closeLocation;
                break;
              case "html":
                window.location = $this.locationUrl.htmlLocation;
                break;
              case "epub":
                window.location = $this.locationUrl.epubLocation;
                break;
              case "pdf":
                window.location = $this.locationUrl.pdfLocation;
                break;
              case "publish":
                window.location = $this.locationUrl.publishLocation;
                break;
              case "chapter":
                window.location = $this.locationUrl.chapterLocation + options.option;
                break;
              case "download":
                window.location = $this.locationUrl.downloadLocation;
                break;
            }
          }, 500);
          break;
        case "show":
          switch (options.action) {

          }
          break;
        case "figure":
          switch (options.action) {
            case "upload":
              $this.figureUpload();
              break;
            case "select":
              $this.figurePicker();
              break;
            default:
              $this.figureCommand(options.action);
              break;
          }
          break;
        case "format":
          switch (options.action) {
            case 'clear':
              $this.undoManager.clear(); //undo mgr
              break;
            case 'undo':
              $this.undoManager.undo(); //undo mgr
              break;
            case 'redo':
              $this.undoManager.redo(); //undo mgr
              break;
            default:
              $this.formatCommand($el, options.action, options.option);
              break;
          }
          break;
        case "find":
          $this.findCommand();
          break;
        case "replace":
          $this.replaceCommand();
          break;
        case "insert":
          switch (options.action) {
            case "table":
              var data = $('input:text[name=rows]').val().toString() + ',' + $('input:text[name=cols]').val().toString();
              $this.insertCommand('table', options.option, data);
              break;
            case "semantic":
              $this.insertSemantic($el.data('value'));
              break;
            case "term":
              $this.insertTerm(options.option, $el.data('value'), $el.text());
              break;
            case "img":
              $this.insertCommand(options.action, null, $el.data('item'));
              break;
            default:
              $this.insertCommand(options.action, null, null);
              break;
          }
          break;
        case "showpopup":
          $('div.internalLinkDialog, div.externalLinkDialog').hide();
          switch (options.action) {
            case "globalcomments":
              $this.loadComments($this.documentId);
              break;
            case "comments":
              $this.loadComments($this.snippetId);
              break;
            case "internal":
            case "external":
              $this.showlinkDialog(options.action);
              break;
            case "properties":
              $this.showProperties();
              break;
          }
          break;
        case "showpane":
          switch (options.action) {
            case "naviButton":
              $("#rightbar").toggle();
              break;
            case "flowButton":
              $("#leftbar").toggle();
              break;
          }
          $('.pane').hide();
          $("." + options.action).toggle();
          if ($("." + options.action).is(":visible")) {
            $("." + options.action).find("a").click();
          }
          break;
        case "shownavipane":
          if ($("." + options.action).is(":visible")) {
            $("." + options.action).css({ display: 'none' });
            if (options.action == 'flowButton') {
              $('div.editable_highlight_haschanged').css({ display: 'none' });
              $this.removeHighlightSnippet();
            }
          } else {
            $("." + options.action).css({ display: 'block' });
          }
          break;
        case "save":
          $this.saveSnippet();
          break;
        case "delete":
          $this.deleteCommand($this.snippetId);
          break;
        case "move":
          switch (options.action) {
            case 'd':
            case 'u':
              $this.move(options.action);
              break;
            case 'l':
            case 'r':
              $this.changeLevel(options.action);
              break;
            case 'o':
              $this.orphan(options.action);
              break;
            case 'i':
              $this.insertOrphanedSnippet(options.option);
              break;
            case 'mergetext':
              $this.merge(options.action);
              break;
            case 'reorganize':
              $this.showReorganize();
              break;
            case 'chapter':
              $this.loadChapter(options.option, true);
              break;
          }
          break;
      }
    });

    // Pane management    
    $('.tablePane').on('click', function () {
      $(".tablePane input[type='text']").spinner({
        min: 0,
        max: 100,
        step: 1,
        increment: 'fast'
      });
    });
    // open equation pane when editing an equation
    $(document).on('click', ".equationContainer :not(img)", function () {
      $('.pane').hide();
      $('.equationPane').show().find('a').click();
    });
    // show pane when editor focused
    // SECTION
    $(document).on('click', '.editableSection', function () {
      if ($('.equationPane').is(':visible')) return;
      $('.pane').hide();
      if (!$('ul.menu li:first ul').is(':visible')) {
        $('ul.menu').find("li:first a").click();
      }
    });
    // TABLE
    $(document).on('click', '.tableEditor', function () {
      $('.pane').hide();
      $('.tablePane').show();
    });
    // IMAGE
    $(document).on('click', '.editableImage', function () {
      var obj = $.parseJSON($(this).closest('.editableImage').find(".imageEditor input[name=properties]").val());
      clearTimeout($this.imgResizeTimeout);
      $(".imagePane input[name='width']").val(obj.ImageWidth);
      $(".imagePane input[name='height']").val(obj.ImageHeight);
      if (!$('.imagePane').is(':visible')) {
        $('.pane').hide();
        $('.imagePane').show();
      }
      if (obj.KeepSize == true) {
        $(".imagePane input[type='text']").spinner('disable');
        $("#keepsize").attr("checked", "checked");
      } else {
        $(".imagePane input[type='text']").spinner('enable');
        $("#keepsize").removeAttr("checked");
      }
    });
    var unbindChange = function () {
      $(".ratio-input").unbind("change");
      $(".ratio-input").attr("disabled", "disabled");
    };
    var setRatio = function (ratio) {
      $this.crop.setRatio(ratio);
    };
    $(document).on('change', "#ratio-none", function () {
      setRatio(0);
      unbindChange();
    });
    $(document).on('change', "#ratio-keep", function () {
      setRatio($this.crop.aspectRatio);
      unbindChange();
    });
    $(document).on('change', "#imageCropDialog #ratio-set", function () {
      $(".ratio-input").removeAttr("disabled");
      setRatio(parseInt($(".ratio-input:eq(0)").val()) / parseInt($(".ratio-input:eq(1)").val()));
      $(".ratio-input").bind("change", function () {
        setRatio(parseInt($(".ratio-input:eq(0)").val()) / parseInt($(".ratio-input:eq(1)").val()));
      });
    });
    $(document).on('change', "input[name=crop-width]", function () {
      $this.crop.setSelect([parseInt($(this).val()) / $this.crop.scaleX + $this.crop.cropCoords.x, $this.crop.cropCoords.y2]);
    });
    $(document).on('change', "input[name=crop-height]", function () {
      $this.crop.setSelect([$this.crop.cropCoords.x2, parseInt($(this).val()) / $this.crop.scaleY + $this.crop.cropCoords.y]);
    });

    // TEXT
    $(document).on('click', ".editableText", function () {
      if ($('.equationPane ul').is(":visible")) return;
      if ($('.imagePane ul').is(":visible") || $('.listingPane ul').is(":visible") || $('.tablePane ul').is(":visible")) {
        isClosed = true;
        $this.resize();
      }
      $('.pane').hide();
      if (!$('ul.menu li:first ul').is(':visible')) {
        $('ul.menu').find("li:first a").click();
      }
    });
    $(".imagePane input[name='width']").bind('blur change keyup', function () {
      $this.figureCommand('setsize', $(this));
    });
    $(".imagePane input[name='height']").bind('blur change keyup', function () {
      $this.figureCommand('setsize', $(this));
    });
    // LISTING
    $(document).on('click', '.listingEditor', function () {
      if (!$('.listingPane').is(':visible')) {
        $('.pane').hide();
        $('.listingPane').show();
      }
      var id = $(this).data('item');
      if ($("#sn_block-" + id + " input[name=syntaxhighlight]").val().toLowerCase() === "true") {
        $("#syntaxhighlight").attr("checked", "checked");
      } else {
        $("#syntaxhighlight").removeAttr("checked");
      }
      if ($("#sn_block-" + id + " input[name=linenumbers]").val().toLowerCase() === "true") {
        $("#linenumbers").attr("checked", "checked");
      } else {
        $("#linenumbers").removeAttr("checked");
      }
    });
    // SIDEBAR
    $(document).on('click', 'ul[data-type=sidebartypes] > li', function () {
      var name = $(this).data('value');
      var id = $(this).data('item');
      var sidebar = $(".editableByTexxtoor[data-item='" + $this.snippetId + "']");
      var header = sidebar.find(".editor").children("header");
      header.text(name);
      sidebar.find("input[name=sidebartype]").val(id);
      // 99 is custom
      id == 99 ? header.attr("contenteditable", 'true') : header.attr("contenteditable", 'false');
    });
    // All panes are initially invisible
    $('.pane').hide();
    my._setTools();
  };
  // this function is just to reduce the number of lines on activateevents and handles the static, one time handlers used on panes
  my._setTools = function () {
    var $this = this;
    // Image
    $(".imagePane input[type='text']").spinner({
      min: 0,
      max: 9999,
      step: 1,
      increment: 'fast'
    });

    // Equations
    $('#mathSrc').keyup(function () { $this.doMathSrc(); }).mouseup(function () { $this.doMathSrc(); }).focusout(function () {
      $('#previewMath').hide();
    });
    $('#insMath').mousedown(function () {
      var sid = $this.snippetId;
      if (sid == undefined) {
        alert('Select a text snippet where you want the equation to paste in.');
      } else {
        $('div.editableByTexxtoor[data-item="' + sid + '"]').htmlarea('pasteEquation', $this.doMathSrc(false));
        $this.deferredSave($('div.editableByTexxtoor[data-item="' + sid + '"]'));
        $('#previewMath').hide();
        $('#mathSrc').val('');
        $('.equationPane').parent().find('li:first a').click();
        $('.equationPane').hide();
      }
      return false;
    });
    $('li.equationPane span[title]').click(function () {
      $('#mathSrc').val($('#mathSrc').val() + $(this).html());
      my.doMathSrc();
    });
  };
  my._initMenus = function () {
    var $this = this;
    var textSnippetMenu = new _TextSnippetMenu();
    $.contextMenu({
      selector: '.textMenu',
      zIndex: 1000,
      build: function ($trigger, e) {
        textSnippetMenu.snippetId = $($trigger.context).attr("id").replace("sn-", "");
        textSnippetMenu.objUndoManager = $this.undoManager;
        textSnippetMenu.objAuthor = $this;
        // Here we add the code to customize the menu before it appears
        return {
          callback: function (key, options) {
            var m = "clicked: " + key;
            window.console && console.log(m) || alert(m);
          },
          items: textSnippetMenu.initializeMenu()
        };
      }
    });

    //$('.editor').contextMenu(textSnippetMenu.initializeMenu(), { theme: 'vista' });

    // this loads the semantic lists for the first time, not sure whether this is the best place

  };
  return my;
}(AUTHOR || {}));