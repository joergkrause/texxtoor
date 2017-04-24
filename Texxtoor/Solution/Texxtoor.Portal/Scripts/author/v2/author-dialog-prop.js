var AUTHOR = (function (my) {
  my.showProperties = function() {
    var $this = this;
    $this.showLoader('Loading Properties');
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'properties',
        additionalData: $this.snippetId
      },
      dataType: 'json',
      contentType: 'application/json',
      cache: false,
      success: function(data) {
        var dlg = new PropertiesDlg(JSON.parse(data));
        $('#propertiesDialog').html(dlg.getDialogHtml());
        $('#btnSaveProperties').click(function() {
          $this.saveProperties();
        });
        $('#closeProperties').click(function() {
          $('#propertiesDialog').dialog('close');
        });
        $this.hideLoader();
        $('#propertiesDialog').dialog('open');
      }
    });
  };
  my.saveProperties = function() {
    var $this = this;
    var d = $('#propertiesDialog :input').serializeArray();
    d.push({ name: 'id', value: $this.documentId });
    var json = [];
    $.each(d, function() {
      var obj = {};
      obj["Key"] = this.name;
      obj["Value"] = this.value;
      json.push(obj);
    });
    $.ajax({
      url: $this.serviceUrl.saveDialog,
      type: 'POST',
      data: JSON.stringify({
        id: $this.documentId,
        dialog: 'properties',
        form: json
      }),
      dataType: 'json',
      contentType: 'application/json',
      success: function(data) {
        $('#propertiesDialog').dialog('close');
        window.location.reload(true);
      }
    });
  };
  return my;
}(AUTHOR || {}));