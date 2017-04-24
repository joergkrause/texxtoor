(function ($) {

  //re-set all client validation given a jQuery selected form or child
  $.fn.resetValidation = function () {

    var $form = this;

    //reset jQuery Validate's internals
    //$form.validate().resetForm();

    //reset unobtrusive validation summary, if it exists
    $form.find("[data-valmsg-summary=true]")
        .removeClass("validation-summary-errors")
        .addClass("validation-summary-valid")
        .find("ul").empty();

    //reset unobtrusive field level, if it exists
    $form.find("[data-valmsg-replace]")
        .removeClass("field-validation-error")
        .addClass("field-validation-valid")
        .empty();

    return $form;
  };
})(jQuery);