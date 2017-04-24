var AUTHOR = (function (my) {

  my.showlinkDialog = function (action) {

    function errorCondition() {
      $('#internalLinkForm').hide();
      $('#btnAddlink').hide();
      $('[data-error=noInsert]').css('display', 'block');
    }

    function addCloseEvent() {
      $('#btnCloselink').click(function () {
        $('#internalLinkDialog').dialog('close');
      });
    }

    var $this = this;
    $this.showLoader('Loading Links');
    // get and keep the range before we open the dialog, as we'll loose the focus
    var editable = $("#sn_block-" + my.snippetId).find(".editableByTexxtoor");
    var r = $(editable).htmlarea('getRange');
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: action,
        additionalData: $this.snippetId
      },
      cache: false,
      dataType: 'json',
      contentType: 'application/json',
      success: function (data) {
        var dlg = new InternalLinkDlg(JSON.parse(data));
        $('#internalLinkDialog').html(dlg.getDialogHtml());
        dlg.selectNode = function (id, text) {
          $('input[name=captionvalue]').val(text);
          $('input[name=captionvalue]').data('item', id);
        };
        addCloseEvent();
        dlg.initializeData();
        $('#internalLinkDialog').dialog('open');
        $this.hideLoader();
        $('input[name=searchvalue]').on('blur keypress', function() {
          $('#il-tree').jstree('search', $(this).val());
        }).on('keyup', function (e) {
          // forward backspace key
          if (e.keyCode == 8) {
            $(this).trigger('keypress');
          }
        });
        $('#btnAddlink').click(function () {
          var text = $('input[name=captionvalue]').val() + " ";
          var id = $('input[name=captionvalue]').data('item');
          var type = $('input[name=captionvalue]').data('type');          
          $('#internalLinkDialog').dialog('close');
          $this._addLink(id, type, text, r);
        });
      }
    });
  };
  my._addLink = function (id, type, text, r) {
    var $this = this;
    var html = '<a class="innerLink ' + type + '" href="#' + type + '-' + id + '" data-type="' + type + '" data-snippet="' + id + '" >' + text + '</a>';
    my.pasteHtmlAtCaret(html, r);
    $this.saveSnippet();
    $('#internalLinkDialog').dialog('close');
  };
  return my;
}(AUTHOR || {}));