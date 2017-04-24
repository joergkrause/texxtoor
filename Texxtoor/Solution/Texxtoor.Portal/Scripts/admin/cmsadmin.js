function CmsAdmin(culture, savePath) {
  this.culture = culture;
  this.savePath = savePath;
}

CmsAdmin.prototype = {
  culture: '',
  savePath: '',
  language: '',
  key: '',
  EditRes: function (uniqueid, args, min) {
    var curr = $(uniqueid);
    key = uniqueid.substr(1);
    $('#img_' + key).css('visibility', 'visible');
    if (min === undefined) {
      curr.attr('contenteditable', 'true');
      var format = "";
      $.each($(uniqueid + ' [data-args]'), function (idx, e) {
        var desc = $(e).attr('data-desc');
        format += $(e).attr('data-args') + "(" + desc + ") = " + $(e).html() + "<br />";
        $(e).attr('data-pattern', $(e).html());
        $(e).html('{' + $(e).attr('data-args') + '}');
      });
      toastr.success('Arguments:<br><br>' + format.toString());
    }
    if (min == 'a') {

    }
    $(uniqueid).focus();
  },
  SaveRes: function (path, uniqueid, reskey, args, min) {
    var $this = this;
    $(uniqueid).attr('contenteditable', 'false');
    $('#img_' + key).css('visibility', 'hidden');
    var d = $(uniqueid).html();
    $.ajax({
      async: true,
      type: 'POST',
      url: $this.savePath,
      data: {
        "path": path,
        "key": reskey,
        "culture": $this.culture,
        "data": d
      },
      success: function (r) {
        if (min === undefined) {
          $.each($('[data-args]'), function (idx, e) {
            $(e).html($(e).attr('data-pattern'));
          });
        }
        $(uniqueid).html(r.data); // allow the server to modify content and immediately write this back
        toastr.success('Save ' + r.msg);
      },
      error: function (r) {
        toastr.error(r.msg);
      }
    });
  }
}
