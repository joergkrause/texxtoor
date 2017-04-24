function _SingleWidget() {
  this.snippetObj = {};
  this.outputHtml = "";
  this.serviceUrl = "";
}

_SingleWidget.prototype = {
  getWidgetHtml: function () {
    var id = this.snippetObj.snippetId;
    this.outputHtml =
          '<div class="snippet-block" id="sn_block-' + id + '" data-item="' + id + '" data-parentid="' + this.snippetObj.parentId + '" data-levelid="' + this.snippetObj.levelId + '">' +
              '<div class="debugDisplay"></div>' +
              '<div class="editable_highlight" style="display:none" data-item="' + id + '"></div>' +
              '<div class="editable_highlight_haschanged" style="display:none" data-item="' + id + '"></div>';
    if (this.snippetObj.widgetName.toLowerCase() != "section") {
      this.outputHtml += '<div class="up-down" data-item="' + id + '">' +
                        '<a class="leftButton nav-up" data-item="' + id + '" data-init-renderactive="false">' +
                            '<img class="Editor_navigate_up_16_png upButton naviButton" src="/App_Sprites/blank.gif" />' +
                        '</a>' +
                        '<a class="leftButton nav-down" data-item="' + id + '" data-init-renderactive="false">' +
                            '<img class="Editor_navigate_down_16_png downButton naviButton" src="/App_Sprites/blank.gif" />' +
                       '</a>' +
                    '</div>';
    }
    switch (this.snippetObj.widgetName.toLowerCase()) {
      case 'section':
        this.outputHtml += '<img class="Editor_newspaper_16_png hintIcon metaDataText flowButton" data-item="' + id + '" src="/App_Sprites/blank.gif" />' +
                      '<div class="right-left">' +
                          '<a class="leftButton nav-left" data-item="' + id + '" data-init-renderactive="true">' +
                              '<img class="Editor_sort_up_plus_16_png higherIcon naviButton" src="/App_Sprites/blank.gif" />' +
                          '</a>' +
                          '<a class="leftButton nav-right"  data-item="' + id + '" data-init-renderactive="true">' +
                              '<img class="Editor_sort_up_minus_16_png deeperIcon naviButton" src="/App_Sprites/blank.gif" />' +
                          '</a>' +
                      '</div>';
        var sectionWidget = new _SectionWidget();
        sectionWidget.snippetObj = this.snippetObj;
        this.outputHtml += sectionWidget.getWidgetHtml();
        break;
      case 'text':
        this.outputHtml += '<img class="Editor_text_16_png hintIcon metaDataText flowButton" data-item="' + id + '" />';
        var textWidget = new _TextWidget();
        textWidget.snippetObj = this.snippetObj;
        this.outputHtml += textWidget.getWidgetHtml();
        break;
      case 'listing':
        this.outputHtml += '<img class="Editor_code_16_png hintIcon metaDataText flowButton" data-item="' + id + '" />';
        var listingWidget = new _ListingWidget();
        listingWidget.snippetObj = this.snippetObj;
        this.outputHtml += listingWidget.getWidgetHtml();
        break;
      case 'sidebar':
        this.outputHtml += '<img class="Editor_newspaper_16_png hintIcon metaDataText flowButton" data-item="' + id + '" />';
        var sidebarWidget = new _SidebarWidget();
        sidebarWidget.snippetObj = this.snippetObj;
        this.outputHtml += sidebarWidget.getWidgetHtml();
        break;
      case 'table':
        this.outputHtml += '<img class="Editor_table2_16_png hintIcon metaDataText flowButton" data-item="' + id + '" />';
        var tableWidget = new _TableWidget();
        tableWidget.snippetObj = this.snippetObj;
        this.outputHtml += tableWidget.getWidgetHtml();
        break;
      case 'image':
        this.outputHtml += '<img class="Editor_photo_landscape_16_png hintIcon metaDataText flowButton" data-item="' + id + '" />';
        var imageWidget = new _ImageWidget();
        this.snippetObj.serviceUrl = this.serviceUrl;
        imageWidget.snippetObj = this.snippetObj;
        this.outputHtml += imageWidget.getWidgetHtml();
        break;
    }
    return this.outputHtml;
  }  
};