var TRANSLATOR = (function (my) {

  my.initializeEditor = function (baseUrl, docId, snippetId, ribbonUrl) {
    this.baseUrl = baseUrl;
    this.documentId = docId;
    this.snippetId = snippetId;
    this.serviceUrl = {
      getSnippet: this.baseUrl + 'GetSnippet',

      getTranslation: this.baseUrl + 'GetTranslation',

      loadComments: this.baseUrl + 'LoadComments',
      saveComment: this.baseUrl + 'SaveComment',

      saveContent: this.baseUrl + 'SaveContent',

      saveDialog: this.baseUrl + 'SaveDialogData',
      loadDialog: this.baseUrl + 'GetDialogData'
    };
    this.locationUrl = {
      closeLocation: "",
      downloadLocation: "",
      chapterLocation: ""
    };
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
    my._attachEvents();
    my._initMenus();
  };
  my._attachEvents = function () {
    var $this = this;
    var isClosed;
    var faddedSnippetsForDragDrop;
    // re-implement all Ctrl-Keys to make the command invoker aware of our private undo stack
    shortcut.add("Ctrl+S", function () {
      $this.saveSnippet();
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
              case "close":
                window.location = $this.locationUrl.closeLocation;
                break;
            }
          }, 500);
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
        case "save":
          $this.saveSnippet();
          break;
      }
    });

    // Pane management    

    // All panes are initially visible
    $('.pane').show();
    my._setTools();
  };
  // this function is just to reduce the number of lines on activateevents and handles the static, one time handlers used on panes
  my._setTools = function () {
    var $this = this;

    $(document).on('keypress', '#translatedText', function() {
      $('#translatedCounter').text($(this).val().length);
    });

  };

  my._initMenus = function () {
    var $this = this;
    // work area
    my.loadDocument(function() {

    });
  };

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


  return my;
}(TRANSLATOR || {}));