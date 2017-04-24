var AUTHOR = (function (my) {
  my.showReorganize = function () {
    var $this = this;
    $this.showLoader('Loading Reorganization Dialog');
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'reorganize'
      },
      dataType: 'json',
      contentType: 'application/json',
      cache: false,
      success: function (data) {
        var dlg = new ReorganizeDlg(JSON.parse(data));
        dlg.options.serviceUrl = my.serviceUrl.saveReorganizedTree;
        $('#reorganizeDialog').html(dlg.getDialogHtml());
        $('#closeReorganize').click(function () {
          $('#reorganizeDialog').dialog('close');
        });
        dlg.initializeData(my.chapterId);
        $this.hideLoader();
        $('#reorganizeDialog').dialog('open');
      }
    });
  }; 
  return my;
}(AUTHOR || {}));