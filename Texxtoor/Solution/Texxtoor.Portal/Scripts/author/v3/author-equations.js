var AUTHOR = (function (my) {

  my.doMathSrc = function (paste) {
    var $this = this;
    var ents;
    $('#mathSrc').focus();
    $('#mathSrc').val().length > 0 ? $('#previewMath').show() : $('#previewMath').hide();
    var srcE = $('#mathSrc')[0];
    var ms = srcE.value.replace(/&([-#.\w]+);|\\([a-z]+)(?: |(?=[^a-z]))/ig,
          function (s, e, m) {
            if (m && (M.macros_[m] || M.macro1s_[m])) return s; // e.g. \it or \sc
            var t = '&' + (e || m) + ';', res = $('<span>' + t + '</span>').text();
            return res != t ? res : ents[e || m] || s;
          }),
        h = ms.replace(/</g, '&lt;');
    if (srcE.value != h) srcE.value = h; // assignment may clear insertion point
    var t;
    try {
      t = M.sToMathE(ms, true);
    } catch (exc) {
      t = String(exc);
    }
    if (paste == undefined) { // preview
      $('#previewMath').empty().append(t);
    }
    //
    if ($('#mathSrc').val().length > 0)
      return '<div contenteditable="false" class="equationContainer">' +
        '<img class="Editor_delete2_16_png" alt="" onclick="var e = $(this).closest(' + "'.editor'" + '); $(this).parent().remove(); e.focus().click(); AUTHOR.saveSnippet();" />' + $(t).get(0).outerHTML + '</div>';
    return '';
  };
  return my;
}(AUTHOR || {}));