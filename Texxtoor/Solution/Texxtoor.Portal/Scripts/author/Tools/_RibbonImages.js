function _RibbonImages() {
  this.resourceObj = {};
  this.title = "";
}

_RibbonImages.prototype = {
  getWidgetHtml: function () {
    var outputHtml = '';

    if (this.resourceObj.length > 0) {
      outputHtml += '<h2>' + this.title + '</h2>';
    } else {
      return outputHtml;
    }
    var counter;
    if (this.resourceObj.length > 7) {
      for (counter = 0; counter < 5; counter++) {
        outputHtml += '<div class="ribbon-button" data-command="insert" data-action="img" data-item="' + this.resourceObj[counter].id + '">' +
                      '<img src="' + this.resourceObj[counter].imageUrl + '"' +
                      'style="width:32px; height:32px; vertical-align:top; margin:2px;border:0" title="' + this.resourceObj[counter].name + '"  alt="' + this.resourceObj[counter].folder + '" />' +
                      '</div>';
      }
      outputHtml += '<div class="popContainer"><span>v</span>' +
                    '<section class="popContainerPopup" style="width: 500px; overflow: auto; max-height: 500px;" data-item="images">';
      for (counter = 5; counter < this.resourceObj.length; counter++) {
        outputHtml += '<div class="ribbon-button" data-command="insert" data-action="img" data-item="' + this.resourceObj[counter].id + '">' +
                      '<img src="' + this.resourceObj[counter].imageUrl + '"' +
                      'style="width:32px; height:32px; vertical-align:top; margin:2px;border:0" title="' + this.resourceObj[counter].name + '"  alt="' + this.resourceObj[counter].folder + '" />' +
                      '</div>';
      }
      outputHtml += '</section></div>';
    }
    else {
      for (counter = 0; counter < this.resourceObj.length; counter++) {
        outputHtml += '<div class="ribbon-button" data-command="insert" data-action="img" data-item="' + this.resourceObj[counter].id + '"><span>' +
                      '<img src="' + this.resourceObj[counter].imageUrl + '"' +
                      'style="width:32px; height:32px; vertical-align:top; margin:2px;border:0" title="' + this.resourceObj[counter].name + '"  alt="' + this.resourceObj[counter].folder + '" /></span>' +
                      '</div>';
      }
    }
    return outputHtml;
  }
};