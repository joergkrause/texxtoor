

$.editable.addInputType('ajaxupload', {
  /* create input element */
  element: function (settings) {
    settings.onblur = 'ignore';
    //var input = $('<input type="file" name="file" />');
    var input = $(' <span class="btn btn-success fileinput-button"><i class="icon-plus icon-white"></i><span>Add files...</span><input name="file" type="file"></span>');
    $(this).append(input);

    return (input);
  },
  content: function (string, settings, original) {
    /* do nothing */
  },
  plugin: function (settings, original) {
    var form = this;
    form.attr("enctype", "multipart/form-data");
    form.attr("method", "post");
    form.attr("action", settings.target);

   
    $("button:submit", form).bind('click', function () {
       form.ajaxSubmit({
         iframe: true,
         dataType: 'html',
         success: function (data, status) {
           $(original).html(data);
           original.editing = false;
           if (settings.intercept !== undefined) {
             settings.intercept();
           }
         },
         error: function (data, status, e) {
           alert(e);
         }
      });    
      return (false);
    });
   
  }
});
