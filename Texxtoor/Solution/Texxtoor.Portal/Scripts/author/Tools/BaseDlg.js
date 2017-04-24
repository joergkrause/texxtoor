function BaseDlg() {

}

BaseDlg.prototype.localize = function (html) {
  var mh = $(html);
  mh.find('span,p,h2,h3,h4,button,label,a,legend').filter('[data-lc]').each(function (i, e) {
    $(e).html(window.localize[$(e).data('lc')][$(e).data('p')]);
  });
  mh.find('input[type=button]').filter('[data-lc]').each(function (i, e) {
    $(e).val(window.localize[$(e).data('lc')][$(e).data('p')]);
  });
  return mh.html();
};

BaseDlg.prototype.getDialogHtml = function () {
  return "";
};