// Dialogs: Joerg Krause, joerg@augmentedcontent.de

// handles all embedded dialogs
// assume that the "addForm" is always on the page, hidden by default
// assume that the "editForm" is pulled from a controller using an ajax request
// assume that the "delForm" is just a confirm box always present but hidden
// assume that the "updateTbl" is refreshing a table using an ajax call

function Dialog(addForm, editForm, delForm, updateTbl) {
  this.addForm = addForm;
  this.editForm = editForm;
  this.delForm = delForm;
  this.updateTbl = updateTbl;
}

Dialog.prototype = {
  addForm: { formId: null, formDivId: null, formAction: null, sendBtnId: null, cancelBtnId: null, onShow: null, onClose: null },
  editForm: { formId: null, formDivId: null, formAction: null, sendBtnId: null, cancelBtnId: null, onShow: null, onClose: null },
  delForm: { formId: null, formDivId: null, formAction: null, sendBtnId: null, cancelBtnId: null, onShow: null, onClose: null },
  updateTbl: { formId: null, formDivId: null, formAction: null, onShow: null },
  // set a status information (optionally)
  setStatusBar: function (text, message) {
  },
  // refresh the table
  updateTable: function (page, order, dir, pagesize, filterValue, filterName) {
    var $this = this;
    page = (page == undefined) ? 0 : page;
    order = (order == undefined) ? "" : order;
    dir = (dir == undefined) ? false : dir;
    pagesize = (pagesize == undefined) ? 5 : pagesize;
    $.ajax({
      url: $this.updateTbl.formAction,
      data: {
        Page: page,
        Order: order,
        Dir: dir,
        PageSize: pagesize,
        FilterValue: filterValue,
        FilterName: filterName
      },
      cache: false,
      dataType: "html",
      success: function (data) {
        $($this.updateTbl.formDivId).html(data);
        // assume we have a regular table we activate the behavior
        $this.setTableBehavior();
        if ($this.updateTbl.onShow instanceof Function) {
          $this.updateTbl.onShow();
        }
        // filter is part of HTML
        $('.filter-control').on('keyup click', function (e) {
          if (e.which == 13 || (e.type == 'click' && e.offsetX > 70 && e.offsetX < 95)) {
            filterValue = $(this).val();
            filterName = $(this).data('name');
            $this.updateTable(page, order, dir, pagesize, filterValue, filterName);
          }
          if (e.which == 27) {
            $(this).val('');
            $this.updateTable(page, order, dir);
          }
        });
        $('.filter-control ~ i.icon-remove').on('click', function (e) {
          $(this).val('');
          $this.updateTable(page, order, dir);
        });
        $('a.counter').on('click', function () {
          $('ul.pagination-drop').toggle();
          $('ul.pagination-drop').css('left', $(this).offset().left);
        });
        $('.pagination-drop').on('mouseleave', function () {
          $('ul.pagination-drop').hide();
        });
        $('.pagination-digg input').on('mousemove', function (e) {
          if (e.offsetX >= 100 && e.offsetX <= 120) {
            $(this).css('cursor', 'pointer');
          } else {
            $(this).css('cursor', 'text');
          }
        });
        $('.pagination-digg input').on('click', function (e) {
          if (e.offsetX >= 100 && e.offsetX <= 120) {
            $this.updateTable(page, order, dir, pagesize, $(this).val(), $(this).data('name'));
          }
        });
        $('.pagination-layout').on('click', function () {
          var reset = function () {
            $('.multilist').gridster().data('gridster').destroy(true);
            $('.optionFields').off('mouseleave');
            $('.optionFields span').remove();
            $('.optionFields > a').css({
              'display': 'inline'
            });
            $('.multilist, .listitem').removeClass('col2').removeClass('col3');
          };
          switch ($(this).data('layout')) {
            default:
            case 0:
              $(this).data('layout', 1);
              reset();
              break;
            case 1:
              $(this).data('layout', 2);
              $('.multilist, .listitem').addClass('col2');
              break;
            case 2:
              $(this).data('layout', 0);
              // drop grid
              var row = 1;
              var col = 1;
              var positions;
              if ($('.multilist').data('gridster')) {
                positions = $('.multilist').data('gridster');
              } else {
                // prepare all items for gridster
                $('.listitem').each(function (i, e) {
                  $(e).attr('data-row', row);
                  $(e).attr('data-col', col);
                  $(e).attr('data-sizex', 2);
                  $(e).attr('data-sizey', 1);
                  col++;
                  if (col == 3) {
                    col = 1;
                    row++;
                  }
                });
              }
              $('.multilist').gridster({
                widget_margins: [10, 10],
                widget_base_dimensions: [140, 140],
                widget_selector: '.listitem',
                avoid_overlapped_widgets: true,
                serialize_changed: function () {
                  $('.multilist').data('gridster', $('.multilist').gridster().data('gridster').serialize());
                }
              });
              $('.optionFields > a').hide(); // TODO: Make drop down
              $('.optionFields').prepend($('<span/>').html('&raquo;&raquo;&raquo;')
                .on('click', function () {
                  $(this).parents('.optionFields').find('a').css({
                    'display': 'block'
                  });
                }));
              $('.optionFields').on('mouseleave', function () {
                $(this).find('a').hide();
              });
              break;
              // after gridster remove and fall through to default
          }
        });
        // register drop event for favorites
        if (!($('#favorites-droppable').is('.ui-droppable'))) {
          $('#favorites-droppable').droppable({
            hoverClass: 'drop-hover',
            accept: 'div[data-drop-item]',
            tolerance: 'pointer',
            drop: function (event, ui) {
              ui.draggable.draggable("option", "revert", false);
              if ($(ui.draggable).data('drop-item')) {
                var id = $(ui.draggable).data('drop-item');
                var model = $(ui.draggable).data('model');
                var title = $(ui.draggable).data('title');
                addFavorite(id, title, model); // defined in home.js
              }
            }
          });
        }
        // drag any item to favorites
        $('.draggable')
          //.prepend($('<i class="icon icon-list-alt icon-2x pull-right" style="position: relative; right: 0px; cursor: crosshair; color: green;" />'))
          .draggable({
          helper: function (e) {
            var elm = $(e.srcElement).hasClass('listitem') ? $(e.srcElement) : $(e.srcElement).parents('.listitem');
            var id = $(elm).data('drop-item');
            var model = $(elm).data('model');
            var title = $(elm).data('title');
            return $("<div></div>").append($('<div data.model=' + model + ' data-title=' + title + ' data-drop-item=' + id + ' style="border:2px solid green; padding:10px; background-color:#fff; font-size: 1.2em;">' + title + '</div>'));
          },
          zIndex: 5100,
          cursorAt: { top: 10, left: 10 },
          revert: 'invalid',
          start: function (e, ui) {
            $('#favorites-droppable').fadeOut(function() { $(this).fadeIn(); });            
          }
        });
        
      }
    });
  },
  // reset the form, show a popup message, and hide the form
  endRequest: function (message, formId) {
    $(formId).resetForm();
    toastr.info(message);
  },
  endWithError: function (message, formId) {
    $(formId).resetForm();
    toastr.error(message);
  },
  parseResult: function (message) {
    if (typeof(message) == 'string' && message.indexOf("{") >= 0 && message.indexOf("}") >= 0) {
      message = $.parseJSON(message.substring(message.indexOf("{"), message.lastIndexOf("}")+1));
    }
    return message;
  },
  // show add form, prepare for ajax call
  addElement: function () {
    var $this = this;
    var handleAddForm = function () {
      if (!$this._addIsAttached) {
        $this._addIsAttached = true;
        // attach events
        $(document).on('keyup.addTemplate', function (e) {
          if (e.keyCode == 13) {
            $($this.addForm.formDivId + " " + $this.addForm.sendBtnId).click();
          } // enter
          if (e.keyCode == 27) {
            $($this.addForm.formDivId + " " + $this.addForm.cancelBtnId).click();
          } // esc
        });
        $(document).on('click', $this.addForm.cancelBtnId, function (evt) {
          $this.hideForm($this.addForm.formDivId);
          $(document).off('keyup.addTemplate');
          $($this.updateTbl.formDivId).show();
          if ($.isFunction($this.addForm.onClose)) {
            $this.addForm.onClose();
          }
          $($this.addForm.sendBtnId).removeAttr('disabled');
          evt.preventDefault();
          return false;
        });
        $(document).on('click', $this.addForm.sendBtnId, function (evt) {          
          $this._checkWatermarks($this.addForm.formId);
          $.validator.unobtrusive.parse($($this.addForm.formId));
          $($this.addForm.formId).ajaxSubmit({
            beforeSubmit: function (arr, form, options) {
              if (form.valid()) {
                $($this.addForm.sendBtnId).attr('disabled', 'disabled');
                return true;
              }
              return false;
            },
            success: function (result) {
              result = $this.parseResult(result);
              $this.endRequest(result.msg, $this.addForm.formDivId);
              $this.hideForm($this.addForm.formDivId);
              $(document).off('keyup.addTemplate');
              $($this.updateTbl.formDivId).show();
              $this.updateTable(0);
              if ($.isFunction($this.addForm.onClose)) {
                $this.addForm.onClose(result);
              }
              $($this.addForm.sendBtnId).removeAttr('disabled');
            },
            error: function (result) {
              result = $this.parseResult(result);
              $this.endWithError(result.msg || result.statusText);
              $($this.addForm.sendBtnId).removeAttr('disabled');
            }
          });
          evt.preventDefault();
          return false;
        });
      }
      if ($this.addForm.onShow instanceof Function) {
        $this.addForm.onShow();
      }
      $($this.addForm.formDivId).show();
    };
    $($this.editForm.formDivId + " .box-content").empty();
    $this.hideForm($this.editForm.formDivId);
    $($this.updateTbl.formDivId).hide();
    $($this.delForm.formDivId).hide();
    if (!$.trim($($this.addForm.formDivId + " .box-content").html())) {
      // no form pre-loaded, hence we load on the fly
      $.ajax({
        url: $this.addForm.formAction,
        type: 'GET',
        cache: false,
        dataType: "html",
        success: function (data) {
          // load data into form container
          $($this.addForm.formDivId + " .box-content").html(data);
          // parse for validation
          $.validator.unobtrusive.parse($($this.addForm.formDivId));
          // show form
          handleAddForm();
          // bootstrap behavior
          setMaxLengthBehavior();
        }
      });
    } else {
      handleAddForm();
    }
    return false;
  },
  _addIsAttached: false,
  // request the controller to deliver the edit form filled with data for given id
  editElement: function (id) {
    var $this = this;
    $($this.addForm.formDivId).hide();
    $($this.delForm.formDivId).hide();
    $.ajax({
      url: $this.editForm.formAction,
      data: { id: id },
      type: 'GET',
      cache: false,
      dataType: "html",
      success: function (data) {
        // load data into form container + " .box-content"
        $($this.editForm.formDivId).html(data);
        // attach events
        $(document).on('keyup.editTemplate', function (e) {
          if (e.keyCode == 13) {
            $($this.editForm.formDivId + " " + $this.editForm.sendBtnId).click();
          } // enter
          if (e.keyCode == 27) {
            $($this.editForm.formDivId + " " + $this.editForm.cancelBtnId).click();
          } // esc
        });
        // parse for validation
        $.validator.unobtrusive.parse($($this.editForm.formDivId));
        // show form
        $($this.editForm.formDivId).show();
        if ($this.editForm.onShow instanceof Function) {
          $this.editForm.onShow(id);
        }
        // prepare buttons
        $($this.editForm.cancelBtnId).click(function () {
          $this.hideForm($this.editForm.formDivId);
          $(document).off('keyup.editTemplate');
          if ($.isFunction($this.editForm.onClose)) {
            $this.editForm.onClose();
          }
        });
        // bootstrap behavior
        setMaxLengthBehavior();
        // send button behavior
        $($this.editForm.sendBtnId).click(function () {
          $this._checkWatermarks($this.editForm.formId);
          $($this.editForm.formId).ajaxSubmit({
            dataType: "json",
            beforeSubmit: function (arr, form, options) {
              return form.valid();
            },
            success: function (result) {
              result = $this.parseResult(result);
              $this.endRequest(result.msg, $this.editForm.formDivId);
              $this.hideForm($this.editForm.formDivId);
              $(document).off('keyup.editTemplate');
              $this.updateTable(0);
              if ($.isFunction($this.editForm.onClose)) {
                $this.editForm.onClose(result);
              }
            },
            error: function (result) {
              result = $this.parseResult(result);
              $this.endWithError(result.msg || result.statusText);
            }
          });
        });
      }
    });
    return false;
  },
  // delete Element and refresh
  deleteElement: function (id, page) {
    var $this = this;
    // restore previously opened dialogs
    if ($('#delTemplate').data('html')) {
      var oldId = $('#delTemplate').data('htmlid');
      $('[data-item=' + oldId + ']').html($('#delTemplate').data('html'));
      $('#delTemplate').removeAttr('data-html');
      $('#delTemplate').removeAttr('data-htmlid');
    }
    // save the current option fields area
    var td = $('.optionFields[data-item=' + id + ']');
    $('#delTemplate').data('html', td.html());
    $('#delTemplate').data('htmlid', id);
    var template = $('#delTemplate').clone().removeClass('hidden-to-show').attr('id', $this.delForm.formDivId.substring(1));
    // copy the template
    td.html($(template));
    $(document).on('keyup.delTemplate', function (e) {
      if (e.keyCode == 13) {
        $($this.delForm.formDivId + " " + $this.delForm.sendBtnId).click();
      } // enter
      if (e.keyCode == 27) {
        $($this.delForm.formDivId + " " + $this.delForm.cancelBtnId).click();
      } // esc
    });
    var retTemplate = function () {
      // hide copy
      td.html($('#delTemplate').data('html'));
      $(document).off('keyup.delTemplate');
    };
    // hide others
    $this.hideForm($this.editForm.formDivId);
    $this.hideForm($this.addForm.formDivId);
    // delform must have a confirm and cancel button, attach the clone
    $($this.delForm.formDivId + " " + $this.delForm.cancelBtnId).off('click');
    $($this.delForm.formDivId + " " + $this.delForm.cancelBtnId).on('click', function (e) {
      retTemplate();
      if ($this.delForm.onClose instanceof Function) {
        $this.delForm.onClose();
      }
    });
    $($this.delForm.formDivId + " " + $this.delForm.sendBtnId).off('click');
    $($this.delForm.formDivId + " " + $this.delForm.sendBtnId).on('click', function () {
      if ($this.delForm.onShow instanceof Function) {
        $this.delForm.onShow();
      }
      $.ajax({
        type: 'POST',
        url: $this.delForm.formAction,
        data: { id: id },
        cache: false,
        dataType: "json",
        success: function (result) {
          retTemplate();
          $this.endRequest(result.msg, $this.delForm.formDivId);
          $this.updateTable(page);
        },
        error: function (xhr, result, errorThrown) {
          retTemplate();
          $this.endRequest(result.msg || result.statusText, $this.delForm.formDivId);
        }
      }).done(function () {
        td.empty().append($('#delTemplate').data('html'));
        if ($this.delForm.onClose instanceof Function) {
          $this.delForm.onClose();
        }
      });
    });
    return false;
  },
  _checkWatermarks: function (formId) {
    $(formId).find('input[type=text]').each(function () {
      if ($(this).val() == $(this).data('watermark')) {
        $(this).val('');
      }
    });
    $(formId).find('textarea').each(function () {
      if ($(this).text() == $(this).data('watermark')) {
        $(this).text('');
      }
    });
  },
  // activate the table behavior used to hide and show buttons on a per row base
  validateAndSend: function (formId, callBack) {
    var $this = this;
    $this._checkWatermarks(formId);
    $.validator.unobtrusive.parse($(formId));
    if (callBack) {
      $(formId).validate();
      if ($(formId).valid()) {
        callBack();
      }
    } else {
      $(formId).validate({
        submitHandler: function (form) {
          form.submit();
        }
      });
      $(formId).submit();
    }
  },
  setTableBehavior: function () {
    // this fixes the dropdown behavior in IE 
    $("select").mouseleave(function (event) {
      event.stopPropagation();
    });
    $('tr').each(function (i, e) {
      var options = $(e).nextAll(':first').find('div.optionFields');
      $(e).next().hover(
        function (evt) {
          options.addClass('multilistSelected');
        },
        function (evt) {
          options.removeClass('multilistSelected');
        });
      $(e).hover(
        function (evt) {
          options.addClass('multilistSelected');
        },
        function (evt) {
          options.removeClass('multilistSelected');
        });
    });
    $('div.thirdRow').each(function (i, e) {
      var options = $(e).find('div.optionFields');
      $(e).next().hover(
        function (evt) {
          options.addClass('multilistSelected');
        },
        function (evt) {
          options.removeClass('multilistSelected');
        });
      $(e).hover(
        function (evt) {
          options.addClass('multilistSelected');
        },
        function (evt) {
          options.removeClass('multilistSelected');
        });
    });
  },
  hideForm: function (id) {
    $(id).hide();
  },
  showForm: function (id) {
    $(id).show();
  }
};

/*
        // per page droppable zone, drop UI concept, drop an item to invoke action

        $('.dropzone-droppable').droppable({
          hoverClass: 'dropzone-hover',
          drop: function (event, ui) {
            ui.draggable.draggable("option", "revert", false);
            // expected key
            var key = $(this).data('command-key');
            // if key provided or not needed
            if (!key || $(ui.draggable).data(key)) {
              var id = $(ui.draggable).data(key);
              var action = $(this).data('command-action');
              var controller = $(this).data('command-controller');
              var type = $(this).data('command-type');
              var update = Boolean($(this).data('command-update'));
              var url = "/" + controller + "/" + action + "/" + id;
              switch (type) {
                case "Json":
                  $.get({
                    url: url,
                    dataType: 'json'
                  }).done(function (d) {
                    toastr.success(d);
                    if (update) {
                      $this.updateTable(page, order, dir, pagesize, filterValue, filterName);
                    }
                  })
                    .fail(function(d) { toastr.error(d); });
                  break;
                case "Href":
                  window.location.href = url;
                  break;
              }              
            }
          }
        });
*/