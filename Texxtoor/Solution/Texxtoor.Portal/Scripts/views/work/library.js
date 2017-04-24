function LibraryFunctions(urls) {
  this.actionUrls = urls;
  $(document).on('click', '#btnCloseDetails', function () {
    $('#main').show();
    $('#showPublished').hide();
  });
}

LibraryFunctions.prototype = {

  publishedId: 0,
  selectionSource: 0,

  actionUrls: {
    showDetails: '',
    showWorkDetails: '',
    createChange: '',
    tableOfContentPublished: '',
    tableOfContentWork: '',
    assignFragment: '',
    rateContent: '',
    rateContentStars: '',
    miniReader: '',
  },

  // id = publishedId
  showDetails: function (id) {
    var $this = this;
    $this.publishedId = id;
    $.ajax({
      url: $this.actionUrls.showDetails,
      data: { id: $this.publishedId, main: false },
      type: 'GET',
      cache: false,
      dataType: 'html',
      contentType: 'text/html',
      success: function (data) {
        $('#main').hide();
        $('#showPublished').html(data);
        $('#showPublished').show();
        $('#editBook').hide();
        $('#changeCollection').html(data);
        // if work derived from published
        $this.attachHandler($this.publishedId);
      }
    });
  },

  // id = workId
  showWorkDetails: function (id) {
    var $this = this;
    $.ajax({
      url: $this.actionUrls.showWorkDetails,
      data: { id: id },
      type: 'GET',
      dataType: 'html',
      success: function (data) {
        $('#main').hide();
        $('#showPublished').html(data);
        $('#showPublished').show();
        $('#editBook').hide();
        $('#changeCollection').html(data);
      }
    });
  },

  attachHandler: function (id, forWork) {
    var $this = this;
    $this.publishedId = id;
    $('#moreReviewLink').click(function () {
      $('#moreReview_form').slideToggle('fast');
      return false;
    });
    $('#moreReview_form').css('display', 'none');
    if (!forWork) {
      $this.createCollectionTreePublished();
    } else {
      $this.createCollectionTreeWork();
    }
    //$('#workDetailsTab').tabs().css({ 'resize': 'none', 'min-height': '300px' });
    // rating
    $('.rating-field').click(function () {
      var v = $(this).data('value');
      $.ajax({
        url: $this.actionUrls.rateContent,
        data: {
          id: $this.publishedId,
          v: v
        },
        success: function (data) {
          $('.rating-field').css('cursor', 'default');
          $('.rating-field').unbind('click');
          toastr.success(data.msg);
          $.ajax({
            url: $this.actionUrls.rateContentStars,
            data: { id: $this.publishedId },
            success: function (data) {
              $('#ratingStars').html(data);
            }
          });
        }
      });
    });
  },

  openReader: function () {
    var $this = this;
    location.href = '/Reader/ReaderApp/' + $this.publishedId + '?type=published';
  },

  createCollectionTreePublished: function () {
    var $this = this;
    var url = $this.actionUrls.tableOfContentPublished;
    $('#res_TocBar').jstree({
      "plugins": ["themes", "json_data", "ui", "types"],
      "themes": {
        "theme": "texxtoor-author",
        "dots": false,
        "icons": false
      },
      "json_data": {
        "ajax": {
          type: "GET",
          url: url,
          cache: false,
          data: { "id": $this.publishedId },
          success: function (result) {
          }
        }
      },
      "types": {
        "max_depth": 2,
        "max_children": -2,
        "valid_children": ["folder"],
        "types": {
          "file": {
            valid_children: "none",
            icon: "icon-file"
          },
          "folder": {
            valid_children: ["folder", "file"],
            icon: "icon-folder"
          }
        }
      }
    });
  },

  createCollectionTreeWork: function (id) {
    var $this = this;
    var url = $this.actionUrls.tableOfContentWork;
    $('#res_TocBar').jstree({
      "plugins": ["themes", "json_data", "ui", "types"],
      "themes": {
        "theme": "texxtoor-author",
        "dots": false,
        "icons": false
      },
      "json_data": {
        "ajax": {
          type: "GET",
          url: url,
          cache: false,
          data: { "id": id },
          success: function (result) {
          }
        }
      },
      "types": {
        "max_depth": 2,
        "max_children": -2,
        "valid_children": ["folder"],
        "types": {
          "file": {
            valid_children: "none",
            icon: "icon-file"
          },
          "folder": {
            valid_children: ["folder", "file"],
            icon: "icon-folder"
          }
        }
      }
    });
  },

  createChange: (function () {
    var $this = this;

    // private functions
    function loadTargetCollection(data) {
      $('#main').hide();
      $('#showPublished').hide();
      $('#editBook').hide();
      $('#changeCollection').html(data);
      $("#changeCollection").fadeIn('fast');
      $('#changeCollectionCancelButton').click(function () {
        $("#changeCollection").fadeOut();
        $('#main').show();
      });
      $('#collectionSelection').change(function () {
        var selectionSource = Number($(this).val());
        if (selectionSource > 0) {
          loadSourceCollection(selectionSource);
        } else {
          $('#workCollection').empty();
        }
      });
      $('a.removeItem').click(function () {
        $(this).parents('li').remove();
      });
      attachDragDropTarget();
    }

    function loadSourceCollection(source) {
      $.ajax({
        url: lib.actionUrls.getWorkCollection,
        data: {
          id: source || 0
        },
        type: 'GET',
        cache: false,
        dataType: "html"
      }).done(function (data) {
        $('#workCollection').html(data);
        attachDragDropSource();
      });
    }

    function attachDragDropSource() {
      var sortableIn;
      // attach events to handle drag'n'drop
      $('ul.chapterList li').click(function () {
        $(this).toggleClass('selected');
      });
      var lastClick, diffClick;
      $('ul.chapterList li').bind('mousedown mouseup', function (e) {
        if (e.type == "mousedown") {
          lastClick = e.timeStamp; // get mousedown time
        } else {
          diffClick = e.timeStamp - lastClick;
          if (diffClick < 600) {
            // add selected class to group draggable objects
            $(this).toggleClass('selected');
          }
        }
      });
      $('#chapterListSource').sortable({
        connectWith: '#chapterListTarget',
        revert: 50,
        cursor: 'copy',
        start: function (e, ui) {
          ui.item.siblings(".selected").appendTo(ui.item);
        },
        stop: function (e, ui) {
          ui.item.after(ui.item.find('li').clone());
          var idx = $('#chapterListTarget').children().index($(ui.item[0])) - 1;
          var elm = $(ui.item[0]).clone(true).removeClass('box ui-draggable ui-draggable-dragging').addClass('box-clone');
          replace(elm.find('.miniReader'), $('<a style="float:right" class="removeItem">[Remove]</a>'));
          $('#chapterListTarget').children(':eq(' + idx + ')').after(elm);
          $(this).sortable('cancel');
        },
        receive: function (e, ui) { sortableIn = 1; },
        over: function (e, ui) { sortableIn = 1; },
        out: function (e, ui) { sortableIn = 0; },
        beforeStop: function (e, ui) {
          if (sortableIn == 0) {
            ui.item.remove();
          }
        },
        remove: function (e, ui) {
        }
      });
      $('#chapterListSource li').disableSelection();
    }

    function replace(s, t) { return t.after(s).remove(); }

    function attachDragDropTarget() {
      $('#chapterListTarget').sortable({
        revert: 50,
        placeholder: 'sortable-placeholder',
        cursor: 'move'
      });
      $('#chapterListTarget li').disableSelection();
    }

    function loadSourceAndTarget(target, source) {
      $.ajax({
        url: lib.actionUrls.createChange,
        data: {
          id: source || 0,
          target: target
        },
        type: 'GET',
        cache: false,
        dataType: "html"
      }).then(function (data) {
        loadTargetCollection(data);
      }).done(function () {
        if (source || 0) {
          loadSourceCollection(source);
        }
      });
    }

    return {
      loadAll: loadSourceAndTarget,
      loadSource: loadSourceCollection
    }

  })(),

  changeFormSubmit: function () {
    var $this = this;
    var data = [];
    $('#chapterListTarget li[data-item]').map(function () {
      data.push($(this).data('item'));
    });
    // Add, Remove Sort --> we just send the desired result
    $.ajax({
      url: $this.actionUrls.assignFragment,
      cache: false,
      data: {
        id: $('#chapterListTarget').data('item'),
        fragmentIds: data
      },
      type: 'POST',
      traditional: true, //MVC needs this
      success: function (data) {
        toastr.success(data.msg);
        $("#changeCollection").fadeOut('fast');
        $('#btnCreate').show();
      }
    }); // ajax
  }

}