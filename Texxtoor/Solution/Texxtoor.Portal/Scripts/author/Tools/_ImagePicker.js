function ImagePickerDlg(data) {
  this.options = data.options;
}

ImagePickerDlg.prototype = new BaseDlg();
ImagePickerDlg.prototype.constructor = ImagePickerDlg;
ImagePickerDlg.prototype.options = { };
ImagePickerDlg.prototype.getDialogHtml = function (serviceUrl) {
  var $this = this;
  var items = {};
  for (var i = 0; i < $this.options.images.length; i++) {
    var id = $this.options.images[i].id;
    var name = $this.options.images[i].name;
    var folder = $this.options.images[i].folder;
    //  GetImg(int id, string c, string res, bool nc = false, string href = "")
    var img = '<li>' +
      '<input type="radio" value="' + id + '" name="figureId" />' +
      '<a class="thumb" data-href="' + serviceUrl.getThumbnail + '/' + id + '?c=EditorResource&res=600x600" title="' + folder + '" >' +
      '<img src="' + serviceUrl.getThumbnail + '/' + +id + '?c=EditorResource&res=250x200&nc=true" alt="' + name + '" /></a>' +
      '<div class="caption">' + name + '</div>';
    if (!items[folder]) {
      items[folder] = [];
    }
    items[folder].push({
      folder: folder,
      item: img
    });
  }
  var pre = '' +
    '<div style="width:600px">' +
    '<p data-lc="Widgets" data-p="Image_Tools_Picker_Text"></p>' +
    '<div class="image-container" id="imageContainer">' +
    '<ul class="thumbs noscript" style="list-style: none; max-height: 350px; overflow-y:scroll; overflow-x:hidden;">';
  var after = '</ul>' + '</div>' +
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Ok" value="Save" id="savePickerDialog"  />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Cancel" value="Cancel" id="closePickerDialog"  />' +
    '</div></div>';
  var list = '';
  $.each(items, function(i, e) {
    list += '<h3>' + i + '</h3>';
    for (j = 0; j < items[i].length; j++) {
      list += items[i][j].item;
    }    
  });
  return this.localize(pre + list + after);
};