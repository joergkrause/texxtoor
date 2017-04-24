var PUBL = function (my) {

  my.modelId = 0;
  my.url = {
    marketingUrl: '', // @Url.Action("GetBasePrice", "Marketing")
    contractUrl: '', // @Url.Action("GetContract", "Publishing")
    getCoverUrl: '', // @Url.Action("GetImg", "Tools", new { id = Model.Id, res = "150x217", nc = true, c = "ProjectCover", area = "" })
    getCoverUrlBig: '',
    removeCoverUrl: '',
    setCoverUrl: '', // @Url.Action("SetCover", new { id = Model.Id })
    filterUrl: '' // @Url.Action("Catalog")
  };
  my.resources = {
    Dialog_View_Saving: '@ViewResources.Dialog_View_Saving',
    Action_UploadImportFile: '@ViewResources.Action_UploadImportFile',
    Button_Cancel: '@ViewResources.Button_Cancel',
    textUpload: '@Html.Loc("textUpload", @"Click Image to upload custom cover.")',
    Publishing_Catalogie_NoStoreFound: '@ViewResources.Publishing_Catalogie_NoStoreFound'
  };

  my.init = function (modelId, url, resources, wizardId) {
    my.modelId = modelId;
    my.url = url;
    my.resources = resources;
    my.wizard = $(wizardId); // fuelEx wizard
    my.wizard.on('change', function (e, data) {
      if (!(data.direction == 'previous' && data.step == 7)) {
        //saveSettings(data.step);
      }
    });
    $('.confirmformbutton').attr('disabled', 'disabled');
    $("#btnPrintContract").click(function () {
      var w = window.open();
      w.document.write($('#authorcontractcontainer').html());
      w.print();
    });
    $("#btnConfirmContract").click(function () {
      $('.confirmformbutton').removeAttr('disabled');
      $("#modal-publish").modal('hide');
    });
    $('#readContract').click(function (e) {
      $.ajax({
        url: my.url.contractUrl,
        data: { id: my.modelId },
        dataType: 'html',
        type: 'GET',
        cache: false
      }).done(function (data) {
        $('#authorcontractcontainer').html(data);
      });
      e.preventDefault();
    });
    $('input[name="BasePrice"]').on('change blur keyup', function () {
      setEstimates($(this).val());
    });
    setEstimates($('input[name="BasePrice"]').val());
    $('#publishingTab').tabs();
    $('.btnSave').click(function (e) {
      var tab = $(this).parents('form').data('item');
      saveSettings(tab);
      e.preventDefault();
      return false;
    });
    $('.btnNext').click(function (e) {
      var tab = $(this).parents('form').data('item');
      if (saveSettings(tab)) {
        my.wizard.wizard('next');
      }
      e.preventDefault();
      return false;
    });
    $('#btnPublish').click(function () {
      var tab = $(this).parents('form').data('item');
      var form = $('form[data-item=' + tab + ']');
      form.submit();
    });
    $('#Catalogs').click(function () {
      $(this).children(':selected').remove();
      $('#Catalogs option').each(function () {
        $(this).attr('selected', 'selected');
      });
    });
    $('#genCoverLnk').click(function (e) {
      saveSettings(5);
      e.preventDefault();
      return false;
    });
    $('#useBackTemplate').change(function(e) {
      var on = $(this).is(':checked');
      if (on) {
        $('#backTemplate').show();
        $('#backColorZone').hide();
      } else {
        $('#backTemplate').hide();
        $('#backColorZone').show();
      }
    });
    $('#remCoverLnk').click(function (e) {
      $.ajax({
        url: my.url.removeCoverUrl,
        type: 'POST',
        success: function () {
          $('img#coverImg').attr('src', my.url.getCoverUrl.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
          $('img#coverImgBig').attr('src', my.url.getCoverUrlBig.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
        }
      });
      e.preventDefault();
      return false;
    });
    $('#coverImg').click(function() {
      $("#coverShow").modal('show');
    });
    $('div.editable').editable(my.url.setCoverUrl, {
      type: 'ajaxupload',
      indicator: my.resources.Dialog_View_Saving,
      submit: my.resources.Action_UploadImportFile,
      cancel: my.resources.Button_Cancel,
      tooltip: my.resources.textUpload,
      name: 'file',
      intercept: function (s) {
        $('img#coverImg').attr('src', my.url.getCoverUrl.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
        $('img#coverImgBig').attr('src', my.url.getCoverUrlBig.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
      }
    });
    createCatTree('');
    $('#cat_filter').keyup(function (evt) {
      if (evt.keyCode == 13) {
        evt.preventDefault();
        return false;
      }
      filterCatTree();
    });
    $('input[name="Marketing.AssignIsbn"]').click(function () {
      if ($('input[name="Marketing.AssignIsbn"]').is(':checked')) {
        $.ajax({
          url: '',
          data: {
            id: modelId,
            assignIsbn: true
          },
          cache: false,
          type: 'GET',
          success: function (data) {
            $('#Isbn input').val(data);
            $('#Isbn img').attr('src', "/Tools/GetImg/" + my.modelId + "?c=BarCode&res=150x80&href=" + data + "&t=" + new Date());
          }
        });
        $('#Isbn').show();
      } else {
        $('#Isbn input').val('');
        $('#Isbn').hide();
      }
    });
    $('input[name=backColor]').simpleColor();
    $('input[name=foreColor]').simpleColor();
    $('#backColorOff').click(function () {
      $('input[name=backColor]').val('');
      $('input[name=backColor]').css('background-color', '');
    });
    $('select[name=fontFamily]').fontSelector();
    $("#bioShow").dialog({ autoOpen: false });
    $('.bioBox').on('click', function () {
      var html = $(this).data('value');
      $('#bioShow').html(html);
      $('#bioShow').dialog("open");
    });
    $('[name="ExternalPublisher.KindleLanguage"]').on('change', function () {
      createCatTree('');
    });
    $(window).keydown(function (event) {
      if (event.keyCode == 13) {
        event.preventDefault();
        return false;
      }
    });
    // 6.
    $('.chapterList').sortable({
      connectWith: 'ul.chapterList',
      cancel: '[data-type="exclude"]',
      start: function (e, i) {
        i.item.siblings(".selected").appendTo(i.item);
      },
      stop: function (e, i) {
        i.item.after(i.item.find('li'));
      }
    });
  };

  var saveSettings = function (formId) {
    var form = $('form[data-item=' + formId + ']');
    var url = $(form).attr('action');
    $(form).resetValidation();
    $.validator.unobtrusive.parse($(form));
    var makeJson = function (data) {
      var js = {};
      $.each(data, function () {
        if (js[this.name] !== undefined) {
          if (!js[this.name].push) {
            js[this.name] = [js[this.name]];
          }
          js[this.name].push(this.value || '');
        } else {
          js[this.name] = this.value || '';
        }
      });
      return JSON.stringify(js);
    };
    if ($(form).valid()) {
      var data = $(form).serializeArray();
      var json = {};
      switch (formId) {
        // special treatment for start tab
        case 1:
          if ($('[name=globalValid]').is(':checked')) {
            $('[data-globalonly]').show();
          } else {
            $('[data-globalonly]').hide();
          }
          //if (!$('[name=publisher]').is(':checked')) {
          //  data.push({ name: "publisher", value: "false" });
          //}
          json = makeJson(data);
          break;
          // special treatment for catalogue HACK:(empty is allowed in DB for quick publish, hence we make the business rule here)
        case 3:
          if ($('#Catalogs option').length == 0) {
            $('#catalogError').show();
            return false;
          } else {
            $('#catalogError').hide();
          }
          json = makeJson(data);
          break;
          // special treatment for resources
        case 6:
          var li = [];
          $('#targetList li[data-item]').each(function (i, e) {
            li.push($(e).data('item'));
          });
          json = JSON.stringify({ targetId: li });
          break;
        case 4:
          json = data;
        default:
          json = makeJson(data);
          break;
      }

      $.ajax({
        url: url,
        data: json,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        type: 'POST',
        traditional: true,
        success: function (data) {
          toastr.success(data.msg);
          if (formId == 5) {
            $('img#coverImg').attr('src', my.url.getCoverUrl.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
            $('img#coverImgBig').attr('src', my.url.getCoverUrlBig.replace(/&amp;/g, '&') + '&t=' + new Date().getTime());
          }
        },
        error: function (data) {
          toastr.error(data.result);
        }
      });
      return true;
    }
    return false;
  };

  var filterCatTree = function () {
    var vtree = $("#res_catBar").dynatree("destroy");
    $("#res_catBar").empty();
    createCatTree($('#cat_filter').val());
    //createCatTree($('#cat_filter').val());
  };

  var createCatTree = function (filter) {
    var lang = $('[name="ExternalPublisher.KindleLanguage"]').val();
    $("#res_catBar").dynatree({
      autoFocus: false,
      initAjax:
      {
        type: "GET",
        cache: false,
        url: my.url.filterUrl,
        data: {
          filter: filter,
          lang: lang,
          id: 0
        },
      },
      onActivate: function (node) {
        var id = node.data.attr.id;
        var tx = node.data.title;
        event.preventDefault();
        $('#Catalogs option[value=' + id + ']').each(function () {
          $(this).remove();
        });
        var option = $('<option>');
        option.attr('value', id);
        option.attr('selected', 'selected');
        option.html(tx);
        $('#Catalogs').append(option);
        return false;
      }
    });

  };


 var setEstimates = function (val) {
    if (parseFloat(val) == 0) {
      $('input[name="Marketing.RegisterForLibraries"]').attr('disabled', 'disabled');
      $('input[name="Marketing.RegisterForLibraries"]').removeAttr('checked');
    } else {
      $('input[name="Marketing.RegisterForLibraries"]').removeAttr('disabled');
    }
    $.ajax({
      url: my.url.marketingUrl,
      data: {
        id: my.modelId,
        val: val
      },
      cache: false,
      type: 'POST',
      success: function (data) {
        $('#estimatePricePrint').html(data.Print);
        $('#estimatePriceEPub').html(data.Epub);
        $('#estimatePriceiBook').html(data.Ibook);
      }
    });
  };

  return {
    Init: my.init
  };

}(PUBL || {});
