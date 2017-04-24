function _ImageWidget() {
  this.documentId = 0;
  this.readonly = false;
  this.content = "";
  this.imageWidth = 0;
  this.imageHeight = 0;
}

_ImageWidget.prototype = {
  getWidgetHtml: function () {
    var isReadOnly = this.snippetObj.isReadOnly ? "readonly='readonly'" : "";
    var id = this.snippetObj.snippetId;
    var mimetype = this.snippetObj.mimetype.replace('+', '%2B');
    this.snippetObj.title = !this.snippetObj.title ? "" : this.snippetObj.title;
    this.calculateWithByAspectRatio(this.snippetObj.width, this.snippetObj.height, "900", "900");
    // in src URL "m" is the mimetype to manage SVG, c is to convert properties (SVG ignores currently)
    return '' +
            '<div class="editableByTexxtoor clearfix editableImage saveableByTexxtoor" data-item="' + id + '" data-editor="ImageEditor">' +
            '<div class="img" style="width:' + this.snippetObj.width + 'px;height:' + this.snippetObj.height + 'px; overflow:hidden; ">' +
            '<img src="' + this.snippetObj.serviceUrl + '?id=' + id + '&m=' + mimetype + '&c=true" style="width:100%; height:100%;" ></div>' +
            '<div class="snippetTitle imageEditor">' +
            '<span>' + this.snippetObj.imageLocalization + ' ' + this.snippetObj.genericChapterNumber + '.' + '<span class="snippetCounter">' + this.snippetObj.snippetCounter + '</span></span>' +
            '<input type="text" name="caption" value="' + this.snippetObj.title + '" data-item="' + id + '" class="saveableCaption editableCaption" ' + isReadOnly + ' />' +
            '<input type="hidden" name="width" value="' + this.snippetObj.width + '" data-item="' + id + '" />' +
            '<input type="hidden" name="height" value="' + this.snippetObj.height + '" data-item="' + id + '" />' +
            // this is for crop dialog 
            '<input type="hidden" name="originalwidth" value="' + JSON.parse(this.snippetObj.properties).OriginalWidth + '" data-item="' + id + '" />' +
            '<input type="hidden" name="originalheight" value="' + JSON.parse(this.snippetObj.properties).OriginalHeight + '" data-item="' + id + '" />' +
            '<input type="hidden" name="properties" value=\'' + this.snippetObj.properties + '\' data-item="' + id + '" />' +
            '</div></div>';
  },
  calculateWithByAspectRatio: function (originalWidth, originalHeight, maxWidth, maxHeight) {
    var ratio = 0;  // Used for aspect ratio
    var width = originalWidth;    // Current image width
    var height = originalHeight;  // Current image height

    // Check if the current width is larger than the max
    if (width > maxWidth) {
      ratio = maxWidth / width;   // get ratio for scaling image
      this.snippetObj.height = height * ratio;    // Reset height to match scaled image
      this.snippetObj.width = maxWidth;    // Reset width to match scaled image
    }
    else {
      // Check if current height is larger than max
      if (height > maxHeight) {
        ratio = maxHeight / height; // get ratio for scaling image
        this.snippetObj.height = maxHeight;
        this.snippetObj.width = width * ratio;    // Reset width to match scaled image
      }
    }
  }
};