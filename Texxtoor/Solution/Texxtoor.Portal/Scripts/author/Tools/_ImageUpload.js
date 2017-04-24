function ImageUploadDlg(data) {
  this.options = data.options;
}

ImageUploadDlg.prototype = new BaseDlg();
ImageUploadDlg.prototype.constructor = ImageUploadDlg;
ImageUploadDlg.prototype.options = {};
ImageUploadDlg.prototype.getDialogHtml = function (serviceUrl) {
  var $this = this;
  return $this.localize('' +
    '<div>' +
    '<p data-lc="Widgets" data-p="Upload_Title_Text" ></p>' +
    '<form id="fileForm">' +
    '<div style="border: 2px solid black; background-color:#8db2e3; padding-top: 15px; width: 200px;" class="btn btn-file">' +
    '<span data-lc="Widgets" data-p="Upload_Legend_Upload"></span>' +
    '<input type="file" name="file" data-action="' + serviceUrl + '" data-form="#fileForm" data-filename="#fileName" data-progress="#progress">' +
    '</div>' +
    '<span id="fileName" class="badge badge-info" style="float:right; position: relative; top:-50px;"></span>' +
    '<br/>Caption (optional): <input type="text" name="caption" style="width: 200px" />&nbsp;' +
    '<br />' +
    '<span id="progress" class="badge badge-info hidden-to-show"></span>' +
    '<img id="upload-preview" />' +
    '</form>' +
    '</div>');
};

ImageUploadDlg.prototype.boundCallback = null;

ImageUploadDlg.prototype.bindEvents = function (callback) {
  var $this = this;
  $this.boundCallback = callback;
  $(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    var size = 0;
    for (var i = 0; i < input.get(0).files.length; i++) {
      size += input.get(0).files[i].size;
    }
    var fr = new FileReader();
    fr.onload = function (e) {
      $('#upload-preview').attr('src', e.target.result);
    }
    fr.readAsDataURL(input.get(0).files[0]);
    if (navigator.userAgent.indexOf('Chrome')) {
      label = label.replace(/C:\\fakepath\\/i, '');
    }
    input.trigger('fileselect', [numFiles, label, size]);
  });
  $(document).on('fileselect', '.btn-file :file', function (event, numFiles, label, size) {
    $($(this).data('filename')).text(label + " (" + size + " Bytes)");
  });
};

ImageUploadDlg.prototype.uploadInsert = function (e) {
  var $this = this;
  var progress = $($(':file').data('progress'));
  var form = $(':file').data('form');
  var action = $(':file').data('action');
  if (progress)
    progress.show();
  $this.uploadFile(form, action,
    function (progressvalue) {
      if (progress)
        progress.text(progress.text() + ".");
    },
    function (data) {
      if (progress)
        progress.hide();
      if ($this.boundCallback != null) {
        $this.boundCallback(data);
      }
    },
    function (data) {
      alert(data.responseText);
    });
  return false;
};


ImageUploadDlg.prototype.uploadFile = function (formSel, url, progress, success, error) {
  var formData = new FormData($(formSel)[0]);
  $.ajax({
    url: url, //server script to process data
    type: 'POST',
    xhr: function () { // custom xhr
      var myXhr = $.ajaxSettings.xhr();
      if (myXhr.upload) { // if upload property exists
        myXhr.upload.addEventListener('progress', progress, false); // progressbar
      }
      return myXhr;
    },
    //Ajax events
    success: function (data) {
      success(data);
    },
    error: function (data) {
      error(data);
    },
    // Form data
    data: formData,
    //Options to tell JQuery not to process data or worry about content-type
    cache: false,
    contentType: false,
    processData: false
  }, 'json');
};