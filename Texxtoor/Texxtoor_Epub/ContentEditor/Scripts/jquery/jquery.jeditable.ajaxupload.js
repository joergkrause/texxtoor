

$.editable.addInputType('ajaxupload', {
  /* create input element */
  element: function (settings) {
    settings.onblur = 'ignore';
    var input = $('<input type="file" name="file" />');
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

    //    from.bind('submit', function(e) {
    //      e.preventDefault(); // <-- important
    //      $(this).ajaxSubmit({
    //          target: '#output'
    //        });
    //    });

    //    form.ajaxForm({
    //      iframe: true,
    //      dataType: 'html',
    //      success: function (data, status) {
    //        $(original).html(data);
    //        original.editing = false;
    //      },
    //      error: function (data, status, e) {
    //        alert(e);
    //      }
    //    });
    
    $("button:submit", form).bind('click', function () {
       form.ajaxSubmit({
         iframe: true,
         dataType: 'html',
         success: function (data, status) {
           $(original).html(data);
           original.editing = false;
         },
         error: function (data, status, e) {
           alert(e);
         }
      });    
      return (false);
    });
   
  }
});
