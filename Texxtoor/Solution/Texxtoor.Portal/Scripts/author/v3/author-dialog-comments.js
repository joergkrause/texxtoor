var AUTHOR = (function (my) {

  /*********************** Comments ***************************/
  my.activateCommentBtn = function (txt) {
    if ($(txt).val().length > 2) {
      $(txt).next('input').removeAttr('disabled');
    } else {
      $(txt).next('input').attr('disabled', 'disabled');
    }
  };
  my.saveComments = function (type, id) {
    var $this = this;
    $this.setSnippetId(id);
    var txt = $('#' + type + 'comment' + '-' + $this.snippetId).val();
    $.ajax({
      url: $this.serviceUrl.saveComments,
      data: {
        id: $this.snippetId,
        comment: txt,
        type: type
      },
      type: "POST",
      cache: false,
      success: function (data) {
        $this.loadComments();
      },
      error: function (data) {
        $this.setStatusBar(window.localize["Authoring"]["Status_NotInserting"]);
      }
    });
  };
  my.loadComments = function (id) {
    
    /* private functions for comments */
    function loadComments(docId, snippetId, target) {
      $.ajax({
        url: $this.serviceUrl.loadComments,
        data: {
          id: $this.documentId,
          snippetId: snippetId,
          target: target
        },
        type: "GET",
        cache: false,
        dataType: "json",
        success: function (idata) {
          var data = JSON.parse(idata.LoadCommentsResult);
          showComments(data);
        }
      });
    }
    
    function saveComment(docId, snippetId, target, subject, comment, closed) {
      $.ajax({
        url: $this.serviceUrl.saveComment,
        data: JSON.stringify({
          id: docId,
          snippetId: snippetId,
          target: target,
          subject: subject,
          comment: comment,
          closed: closed
        }),
        type: "POST",        
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (idata) {
          var data = JSON.parse(idata.SaveCommentResult);
          showComments(data);
        }
      });
    }
    
    function showComments(items) {
      var t = $('p[data-item=' + items.Target + ']');
      if (items.Comments.length == 0) {
        t.text(window.localize["Authoring"]["Comments_NoComments"]);
        t.css('color', 'red');
      } else {
        t.empty();
        t.css('color', '');
        var ul = $('<ul>');
        $.each(items.Comments, function(i, e) {
          $('<li>').html('<b>' + e.Subject + '</b> (' + e.Date + ', User ' + e.UserName + ')<br />' + e.Text).appendTo(ul);
        });
        ul.appendTo(t);
      }
    }

    var $this = this;
    if (!id) id = $this.snippetId;
    $this.showLoader(window.localize["Authoring"]["Loader_Load_Data"]);
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'comments'
      },
      type: "GET",
      cache: false,
      dataType: "json",
      success: function (data) {        
        var dlg = new CommentsDlg(JSON.parse(data));
        $('#commentDialog').html(dlg.getDialogHtml());
        $('#commentAccordion').tabs();
        $('#btnCloseComments').bind('click', function () {
          $('#commentDialog').dialog('close');
        });
        $('.btnAddComment').click(function() {
          var target = $(this).data('item');
          var subject = $('input[type=text][data-item=' + target + ']').val();
          var closed = $('input[type=checkbox][data-item=' + target + ']').is(':checked').length > 0 ? true : false;
          var comment = $('textarea[data-item=' + target + ']').val();
          saveComment($this.documentId, id, target, subject, comment, closed);
        });
        $('p.existingComments').each(function(i, e) {
          var target = $(this).data('item');
          loadComments($this.documentId, id, target);
        });
        $this.hideLoader();
        $('#commentDialog').dialog('open');        
      },
      error: function (data) {
        $this.hideLoader();
        $this.setStatusBar(window.localize["Authoring"]["Loader_Comment_Error"]);
      }
    });
  };
  return my;
}(AUTHOR || {}));