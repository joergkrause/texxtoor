function PropertiesDlg(data) {
  this.options = data.options;
}

PropertiesDlg.prototype = new BaseDlg();
PropertiesDlg.prototype.constructor = PropertiesDlg;
PropertiesDlg.prototype.options = {
  allowChapters: true,
  allowMetaData: true,
  chapterDefault: "Chapter",
  sectionDefault: "Section",
  textSnippetDefault: "<p>&nbsp;</p>",
  listingSnippetDefault: "public class Default { \n // Demo \n}",
  showNaviPane: true,
  showFlowPane: true,
  showNumberChain: true
};
PropertiesDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return this.localize('' +
    '<div style="width:650px !important">' +
    '<p data-lc="Widgets" data-p="Properties_Text"></p>' +
    '<fieldset>' +
    '<legend data-lc="Widgets" data-p="Properties_Legend"></legend>' +
    '<label for="AllowChapters" class="dialog" data-lc="Widgets" data-p="Properties_AllowChapters_Label"></label>' +
    '<input type="checkbox" name="allowChapters" value="true" ' + ($this.options.allowChapters ? "checked=checked" : "") + "/>" +
    '<br/>' +
    '<label for="AllowMetaData" class="dialog" data-lc="Widgets" data-p="Properties_AllowMetadata_Label"></label>' +
    '<input type="checkbox" name="allowMetaData" value="true" ' + ($this.options.allowMetaData ? "checked=checked" : "") + "/>" +
    '</fieldset>' +
    '<br/>' +
    '<fieldset>' +
    '<legend data-lc="Widgets" data-p="Properties_Defaults"></legend>' +
    '<label for="chapterDefault" class="dialog" data-lc="Widgets" data-p="Properties_Defaults_ChapterTitle"></label>' +
    '<input type="text" name="chapterDefault" value="' + $this.options.chapterDefault + '" />' +
    '<label for="sectionDefault" class="dialog" data-lc="Widgets" data-p="Properties_Defaults_SectionTitle"></label>' +
    '<input type="text" name="sectionDefault" value="' + $this.options.sectionDefault + '" />' +
    '<label for="textSnippetDefault" class="dialog" data-lc="Widgets" data-p="Properties_Defaults_Text"></label>' +
    '<input type="text" name="textSnippetDefault" value="' + $this.options.textSnippetDefault + '" />' +
    '<label for="listingSnippetDefault" class="dialog" data-lc="Widgets" data-p="Properties_Defaults_Listing"></label>' +
    '<input type="text" name="listingSnippetDefault" value="' + $this.options.listingSnippetDefault + '" />' +
    '</fieldset>' +
    '<br/>' +
    '<fieldset>' +
    '<legend data-lc="Widgets" data-p="Propertires_Behavior_Legend"></legend>' +
    '<label for="ShowFlowPane" class="dialog" data-lc="Widgets" data-p="Properties_ShowFunctions_FlowPane"></label>' +
    '<input type="checkbox" name="showNaviPane" value="true" ' + ($this.options.showNaviPane ? "checked=checked" : "") + "/>" +
    '<br/>' +
    '<label for="ShowNaviPane" class="dialog" data-lc="Widgets" data-p="Properties_ShowFunctions_NaviPane"></label>' +
    '<input type="checkbox" name="showFlowPane" value="true" ' + ($this.options.showFlowPane ? "checked=checked" : "") + "/>" +
    '</fieldset>' +
    '<br/>' +
    '<fieldset>' +
    '<legend data-lc="Widgets" data-p="Propertires_Behavior_Legend"></legend>' +
    '<label for="ShowNumberChain" class="dialog" data-lc="Widgets" data-p="Propertires_Behavior_ShowNumberChain"></label>' +
    '<input type="checkbox" name="showNumberChain" value="true" ' + ($this.options.showNumberChain ? "checked=checked" : "") + "/>" +
    '</fieldset>' +
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Ok" value="Ok" name="btnSaveProperties" id="btnSaveProperties" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Cancel" value="Cancel" id="closeProperties" />' +
    '</div></div>');
};