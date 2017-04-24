function ImageCropDlg(data) {
  this.options = data.options;
}

ImageCropDlg.prototype = new BaseDlg();
ImageCropDlg.prototype.constructor = ImageCropDlg;
ImageCropDlg.prototype.options = {
  noRatio: true,
  keepRatio: false,
  setRatio: false,
  cropWidth: 0,
  cropHeight: 0,
  ratioWidth: 0,
  ratioHeight: 0,
  cropX: 0,
  cropY: 0
};
ImageCropDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return this.localize('' +
    '<div style="width:600px">' +
    '<p data-lc="Widgets" data-p="Image_Tools_Crop_Text"></p>' +
    '<div class="image-container" id="imageContainer">' +
    '</div>' +
    '<div class="options-container" style="float:right;">' +
    '<h3 style="margin-top: 10px;" data-lc="Widgets" data-p="Image_Tools_Crop_Size"></h3>' +
    '<input type="radio" id="ratio-none" name="ratio" ' + ($this.options.noRatio ? "checked=checked" : "") + ' />' +
    '<label for="ratio-none" class="dialog" data-lc="Widgets" data-p="Image_Tools_Crop_None"></label><br />' +
    '<input type="radio" id="ratio-keep" name="ratio" ' + ($this.options.keepRatio ? "checked=checked" : "") + ' />' +
    '<label for="ratio-keep" class="dialog" data-lc="Widgets" data-p="Image_Tools_Crop_KeepRatio"></label><br />' +
    '<input type="radio" id="ratio-set" name="ratio" ' + ($this.options.setRatio ? "checked=checked" : "") + ' />' +
    '<label for="ratio-set" class="dialog" data-lc="Widgets" data-p="Image_Tools_Crop_Set_Ratio"></label><br />' +
    '<input type="text" class="ratio-input" value="' + $this.options.ratioWidth + '" />' + ' x ' +
    '<input type="text" class="ratio-input" value="' + $this.options.ratioHeight + '" />' +
    '<h3 style="margin-top: 10px;" data-lc="Widgets" data-p="Image_Tools_Crop_Position"></h3>' +
    '<table>' +
    '<tr><td><label>&times;</label>&nbsp;</td>' +
    '<td><input type="text" name="crop-x" class="cropvalues" value="' + $this.options.cropX + '" />' +
    '<td>x</td><td><label>Y</label>&nbsp;<input type="text" name="crop-y" class="cropvalues" value="' + $this.options.cropY + '" /></td></tr>' +
    '<tr><td><label>W</label>&nbsp;</td>' +
    '<td><input type="text" name="crop-width" class="cropvalues" value="' + $this.options.cropWidth + '" />' +
    '<td>x</td><td><label>H</label>&nbsp;<input type="text" name="crop-height" class="cropvalues" value="' + $this.options.cropHeight + '" /></td></tr>' +
    '</table></div>' +
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Preview" value="Preview" id="showCrop" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Ok" value="Save" id="saveCrop" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Cancel" value="Cancel" id="cancelCrop"  />' +
    '</div></div>');
};

ImageCropDlg.prototype.isNumber = function(o) {
  return !isNaN(o - 0) && o != null;
};
