function ImageColorsDlg(data) {
  this.options = data.options;
}

ImageColorsDlg.prototype = new BaseDlg();
ImageColorsDlg.prototype.constructor = ImageColorsDlg;
ImageColorsDlg.prototype.options = {
  transparent: "",
  brightness: 0,
  contrast: 0,
  hue: 0,
  saturation: 0
};
ImageColorsDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return this.localize('' +
    '<div title="">' +
    '<p data-lc="Widgets" data-p="Image_Tools_Image_Colors"></p>' +
    '<div class="image-opts-container">' +
    '<div class="opt-titles">' +
    '<div class="opts-block">' +
    '<h4 data-lc="Widgets" data-p="Image_Tools_Transparent_Colors"></h4>' +
    '<input type="text" value="FF0000" id="t-color" /> <div class="t-color-picker"><div></div>' +
    '</div>' +
    '</div>' +
    '<div class="opts-block">' +
    '<h4 data-lc="Widgets" data-p="Image_Tools_Image_Brightness"></h4>' +
    '<div class="s-value"></div><div class="slider" id="b-slider"></div>' +
    '<h4 data-lc="Widgets" data-p="Image_Tools_Image_Contrast"></h4>' +
    '<div class="s-value"></div><div class="slider" id="c-slider"></div>' +
    '<h4 data-lc="Widgets" data-p="Image_Tools_Image_Hue"></h4>' +
    '<div class="s-value"></div><div class="slider" id="h-slider"></div>' +
    '<h4 data-lc="Widgets" data-p="Image_Tools_Image_Saturation"></h4>' +
    '<div class="s-value"></div><div class="slider" id="s-slider"></div>' +
    '</div>' +
    '</div>' +
    '</div><div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Reset" value="Reset" id="resetColors" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Preview" value="Preview" id="showColors" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Ok" value="Save" id="saveColors" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Cancel" value="Cancel" id="cancelColors" />' +
    '</div></div>');
};
