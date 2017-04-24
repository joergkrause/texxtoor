function SemanticListDlg(data) {
  this.options = data.options;
}

SemanticListDlg.prototype = new BaseDlg();
SemanticListDlg.prototype.constructor = SemanticListDlg;
SemanticListDlg.prototype.options = {};
SemanticListDlg.prototype.getDialogHtml = function () {
  var $this = this;
  // TODO: Make a dialog that handles huge amount of semantic data
};