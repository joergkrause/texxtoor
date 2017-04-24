var DESIGNER = (function (my) {

  my.initializeEditor = function (baseUrl, projectId, resourceId, ribbonUrl) {
    this.baseUrl = baseUrl;
    this.projectId = projectId;
    this.resourceId = resourceId;
    this.serviceUrl = {

      loadComments: this.baseUrl + 'LoadComments',
      saveComment: this.baseUrl + 'SaveComment',

      loadSvg: this.baseUrl + 'LoadSvg',
      saveSvg: this.baseUrl + 'SaveSvg',
      saveImage: this.baseUrl + 'SaveImage',

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
    my.isLoaded = true;
    my._attachEvents();
    my._initMenus();
  };
  my._attachEvents = function () {
    var $this = this;
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
          $this.showLoader(window.localize["Designer"]["Loader_Close"]);
          setTimeout(function () {
            switch (options.action) {
              case "close":
                window.location = $this.locationUrl.closeLocation;
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
          switch (options.action) {
            case "globalcomments":
              $this.loadComments($this.documentId);
              break;
            case "comments":
              $this.loadComments($this.snippetId);
              break;
            case "properties":
              $this.showProperties();
              break;
          }
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
  };

  my._initMenus = function () {
    var $this = this;
    // context menus

  };
  return my;
}(DESIGNER || {}));