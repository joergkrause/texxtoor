var AUTHOR = (function (my) {

  my.figureUpload = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'imageupload',
        additionalData: $this.snippetId
      },
      dataType: 'json',
      contentType: 'application/json',
      success: function (data) {
        var dlg = new ImageUploadDlg(JSON.parse(data));
        $('#imageUploadDialog').html(dlg.getDialogHtml($this.serviceUrl.imageUploadByService + "?docId=" + $this.documentId + "&chapId=" + $this.chapterId + "&SnipId=" + $this.snippetId));
        $('#imageUploadDialog').dialog('open');
        dlg.bindEvents();
        $this.bindImageUploadEvents();
      }
    });
  };

  my.figurePicker = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'figurepicker',
        additionalData: $this.snippetId
      },
      dataType: 'json',
      contentType: 'application/json',
      success: function (data) {
        var dlg = new ImagePickerDlg(JSON.parse(data));
        $('#imagePickerDialog').html(dlg.getDialogHtml());
        $('#closePickerDialog').click(function () {
          $('#propertiesDialog').dialog('close');
        });
        $this.hideLoader();
        $('#imagePickerDialog').dialog('open');
      }
    });
  };

  my.figureCommand = function (action, obj) {
    var $this = this;
    switch (action) {
      case "crop":
        if ($(".cmd-button").attr("disable")) {
          return false;
        }
        var prevCrop = JSON.stringify($this.jsonObj);
        $.ajax({
          url: $this.serviceUrl.loadDialog,
          data: {
            id: $this.documentId,
            dialog: 'imagecrop',
            additionalData: $this.snippetId
          },
          dataType: 'json',
          contentType: 'application/json',
          success: function (data) {
            var dlg = new ImageCropDlg(JSON.parse(data));
            $('#imageCropDialog').html(dlg.getDialogHtml($this.ImageUploadByService + "/" + $this.documentId + "?title=Image"));
            $('#imageCropDialog').dialog('open');
            $this.crop.setSnippetId($this.snippetId);
            $('#imageCropDialog .image-container').find("img.source-img").remove();
            $this.crop.loadImageContainer();
            var t = new Date().getTime();
            $this.crop.setImage($this.serviceUrl.imagePath + '/' + $this.snippetId + '/false?' + t);


            $('#saveCrop').bind('click', function () {
              $this.crop.saveCropProperties();
              $this.saveSnippet(function () {
                t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
                $this.crop.release();
                $('#imageCropDialog').dialog('close');
              });
            });

            $('#showCrop').bind('click', function () {
              $this.crop.saveCropProperties();
              $this.saveSnippet(function () {
                t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
              });
            });

            $('#cancelCrop').bind('click', function () {
              $("#sn_block-" + $this.snippetId + " input[name='properties']").val(prevCrop);
              $this.saveSnippet(function () {
                t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
                $this.crop.release();
                $('#imageCropDialog').dialog('close');
              });
            });

          }
        });
        break;
      case "keepsize":
        if ($("#keepsize").attr("disable")) {
          $("#keepsize").attr("checked", "checked");
          return false;
        }
        if ($("#keepsize").attr("checked")) {
          $(".imagePane input[type='text']").spinner('disable');
          $this.jsonObj.KeepSize = true;
          if ($this.jsonObj["Crop"] == null) {
            $this.jsonObj.ImageHeight = $this.jsonObj.OriginalHeight;
            $(".imagePane input[name='width']").val($this.jsonObj.OriginalWidth);
            $(".imagePane input[name='height']").val($this.jsonObj.OriginalHeight);
            $this.jsonObj.ImageWidth = $this.jsonObj.OriginalWidth;
          }
        } else {
          $(".imagePane input[type='text']").spinner('enable');
          $this.jsonObj.KeepSize = false;
        }
        $("#sn_block-" + $this.snippetId + " input[name='properties']").val(JSON.stringify($this.jsonObj));
        $this.saveSnippet(function () {
          var t = new Date().getTime();
          $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
        });
        break;
      case "setsize":
        var img = $('#sn_block-' + $this.snippetId + ' div.img');
        if ($this.keepImageRatio) {
          if (isNaN($this.jsonObj.ImageWidth)) {
            $this.jsonObj.ImageWidth = $this.jsonObj.OriginalWidth;
          }
          if (isNaN($this.jsonObj.ImageHeight)) {
            $this.jsonObj.ImageHeight = $this.jsonObj.OriginalHeight;
          }
          if (obj != null) {
            if (obj.attr('name') == 'width') {
              $(".imagePane input[name='height']").val(Math.round(obj.val() * ($this.jsonObj.ImageHeight / $this.jsonObj.ImageWidth)));
            } else if (obj.attr('name') == 'height') {
              $(".imagePane input[name='width']").val(Math.round(obj.val() * ($this.jsonObj.ImageWidth / $this.jsonObj.ImageHeight)));
            }
          } else {
            if ($(img).width() != $this.jsonObj.ImageWidth) {
              $(".imagePane input[name='height']").val(Math.round($(img).width() * ($this.jsonObj.ImageHeight / $this.jsonObj.ImageWidth)));
            } else if ($(img).height() != $this.jsonObj.ImageHeight) {
              $(".imagePane input[name='width']").val(Math.round($(img).height() * ($this.jsonObj.ImageWidth / $this.jsonObj.ImageHeight)));
            }
          }
          $this.jsonObj.ImageWidth = $(".imagePane input[name='width']").val();
          $this.jsonObj.ImageHeight = $(".imagePane input[name='height']").val();
        }
        $(img).width($(".imagePane input[name='width']").val());
        $(img).height($(".imagePane input[name='height']").val());
        $this.jsonObj.ImageWidth = $(img).width();
        $this.jsonObj.ImageHeight = $(img).height();
        $("#sn_block-" + $this.snippetId + " input[name='properties']").val(JSON.stringify($this.jsonObj));
        $this.saveSnippet(function () {
          var t = new Date().getTime();
          $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
        });
        break;
      case "setratio":
        if ($("#keepratio1").is(":visible")) {
          $this.keepImageRatio = false;
          $("#keepratio1").hide();
          $("#keepratio2").show();
        } else {
          $this.keepImageRatio = true;
          $("#keepratio2").hide();
          $("#keepratio1").show();
        }
        $this.figureCommand('setsize');
        break;
      case "colors":
        if ($(".cmd-button").attr("disable")) {
          return false;
        }
        if ($this.jsonObj.Colors == null) {
          return false;
        }
        var previousColors = $("#sn_block-" + $this.snippetId + " input[name='properties']").val();
        $.ajax({
          url: $this.serviceUrl.loadDialog,
          data: {
            id: $this.documentId,
            dialog: 'imagecolors',
            additionalData: $this.snippetId
          },
          dataType: 'json',
          success: function (data) {
            var dlg = new ImageColorsDlg(JSON.parse(data));
            $('#imageColorsDialog').html(dlg.getDialogHtml());
            $('#imageColorsDialog').dialog('open');
            var color = $this.jsonObj.Colors['TransparentColor'] != null ? '#' + $this.jsonObj.Colors['TransparentColor'] : '#FFFFFF';
            $("#t-color").val(color);

            $('.t-color-picker > div').css('backgroundColor', color);
            $("#t-color").ColorPicker({
              color: color,
              onShow: function (colpkr) {
                $(colpkr).fadeIn(500);
                return false;
              },
              onHide: function (colpkr) {
                $(colpkr).fadeOut(500);
                return false;
              },
              onChange: function (hsb, hex, rgb) {
                $('.t-color-picker > div').css('backgroundColor', '#' + hex);
                $("#t-color").val('#' + hex);
                $this.jsonObj.Colors['TransparentColor'] = hex;
              }
            });
            $(".s-value:eq(0)").html($this.jsonObj.Colors['Brightness']);
            $("#b-slider").slider({
              animate: true,
              value: $this.jsonObj.Colors['Brightness'],
              max: 256,
              min: -256,
              change: function (event, ui) {
                $this.jsonObj.Colors['Brightness'] = ui.value;
              },
              slide: function (event, ui) { $(".s-value:eq(0)").html(ui.value); }
            });
            $(".s-value:eq(1)").html($this.jsonObj.Colors['Contrast']);
            $("#c-slider").slider({
              animate: true,
              value: $this.jsonObj.Colors['Contrast'],
              max: 128,
              min: -128,
              change: function (event, ui) {
                $this.jsonObj.Colors['Contrast'] = ui.value;
              },
              slide: function (event, ui) { $(".s-value:eq(1)").html(ui.value); }
            });
            $(".s-value:eq(2)").html($this.jsonObj.Colors['Hue']);
            $("#h-slider").slider({
              animate: true,
              value: $this.jsonObj.Colors['Hue'],
              max: 180,
              min: -180,
              change: function (event, ui) {
                $this.jsonObj.Colors['Hue'] = ui.value;

              },
              slide: function (event, ui) { $(".s-value:eq(2)").html(ui.value); }
            });
            $(".s-value:eq(3)").html($this.jsonObj.Colors['Saturation']);
            $("#s-slider").slider({
              animate: true,
              value: $this.jsonObj.Colors['Saturation'],
              max: 100,
              min: -100,
              change: function (event, ui) {
                $this.jsonObj.Colors['Saturation'] = ui.value;

              },
              slide: function (event, ui) { $(".s-value:eq(3)").html(ui.value); }
            });
            if ($this.jsonObj["Effects"] != null) {
              var filters = $this.jsonObj["Effects"].split(";");
              $("#selected-effects").html('');
              for (var i = 0; i < filters.length - 1; i++) {
                $("#selected-effects").append($('<div class="ui-state-default">' + filters[i] + '</div>'));
              }
            }
            $(".connectedSortable").css({ minHeight: $(".connectedSortable:eq(0)").height() });
            $("#available-effects div").draggable({
              connectToSortable: "#selected-effects",
              helper: "clone"
            }).disableSelection();
            $("#selected-effects").sortable({
              connectWith: '#available-effects',
              forcePlaceholderSize: true,
              receive: function (e, ui) { $this.sortableIn = 1; },
              over: function (e, ui) { $this.sortableIn = 1; },
              out: function (e, ui) { $this.sortableIn = 0; },
              beforeStop: function (e, ui) {
                if ($this.sortableIn == 0) {
                  ui.item.remove();
                }
              }
            }).disableSelection();

            $('#saveColors').on('click', function () {
              $this.jsonObj["Effects"] = "";
              $("#selected-effects > div").each(function () {
                $this.jsonObj["Effects"] += $(this).html() + ";";
              });
              $("#sn_block-" + $this.snippetId + " input[name='properties']").val(JSON.stringify($this.jsonObj));
              $this.saveSnippet(function () {
                var t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
              });
              $('#imageColorsDialog').dialog('close');
            });

            $('#showColors').on('click', function () {
              $this.jsonObj["Effects"] = "";
              $("#selected-effects > div").each(function () {
                $this.jsonObj["Effects"] += $(this).html() + ";";
              });
              $("#sn_block-" + $this.snippetId + " input[name='properties']").val(JSON.stringify($this.jsonObj));
              $this.saveSnippet(function () {
                var t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
              });
            });
            $('#resetColors').on('click', function () {
              $("#h-slider, #b-slider, #c-slider, #s-slider").slider('value', 0);
            });
            $('#cancelColors').on('click', function () {
              $("#sn_block-" + $this.snippetId + " input[name='properties']").val(previousColors);
              $this.saveSnippet(function () {
                var t = new Date().getTime();
                $(".editableImage[data-item=" + $this.snippetId + "]").find('img').attr('src', $this.serviceUrl.imagePath + '/' + $this.snippetId + '/true?' + t);
              });
              $('#imageColorsDialog').dialog('close');
            });

          }
        });
        break;
      default:
        break;
    }
  };
  my.setImageSize = function () {
    var $this = this;
    var width = $(".imagePane input[name='width']");
    var height = $(".imagePane input[name='height']");
    $(".editableImage[data-item=" + $this.snippetId + "]").find(".imageEditor input:eq(1)").val(width);
    $(".editableImage[data-item=" + $this.snippetId + "]").find(".imageEditor input:eq(2)").val(height);
  };
  my.bindImageUploadEvents = function () {
    var $this = this;
    var widget = new _SingleWidget();
    $('#fileupload').bind('fileuploaddone', function (e, data) {
      if (data.error) {
        alert(data.error);
      }
      if (data.jqXHR.responseText || data.result) {
        var fu = $('#fileupload').data('fileupload');
        var json = (data.jqXHR.responseText) ? jQuery.parseJSON(data.jqXHR.responseText) : data.result;
        //var files = jQuery.parseJSON(json.files)["files"];
        ////console.log(jQuery.parseJSON(json.snippet.properties));
        //fu._adjustMaxNumberOfFiles(files["0"].Length);
        //fu._renderDownload(files)
        //    .appendTo($('#fileupload .files'))
        //    .fadeIn(function () {
        //      // Fix for IE7 and lower:
        //      $(this).show();
        //    });
        var id = $this.snippetId;
        //console.log(json.snippet);
        if (json.snippet) {
          widget.snippetObj = json.snippet;
          $this.snippetId = json.snippet.id;
          widget.serviceUrl = $this.serviceUrl.imagePath;
          $this._appendElement(widget.getWidgetHtml(), $("#sn_block-" + id)); // id is current element before inserting 
          $this.resize();
          $this.scrollManager.setScrollPosition($this.scrollPosition);

          $this.AutoSetSnippetEditorButtons();
          $this.hideLoader();
          $this.updateWidgetTools();
          $("#imageUploadDialog").dialog('close');
        }
        $this.loadRibbonImages();
      }
    });
  };

  return my;
}(AUTHOR || {}));