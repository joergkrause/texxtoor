// summarize all global functions
$(function () {
  // close main menu on load
  $('#nav-menu').trigger('click');
  var timerId;
  var delayShow = true;
  // provide a global loader handler
  $(document).ajaxStart(function () {
    $('#loadIndicator').text($('#loadIndicator').data('text')).show();
    timerId = setInterval(function () { $('#loadIndicator').append("."); }, 500);
    $('#delayIndicator').hide();
    delayShow = true;
    setTimeout(function () {
      clearInterval(timerId);
      if (delayShow) {
        $('#loadIndicator').hide();
        $('#delayIndicator').show();
      }
    }, 5000);
  }).ajaxSuccess(function () {
  }).ajaxError(function (o, x, h, e) {
    $('#errorIndicator').empty().append('Error occured: ' + e).show();
    setTimeout(function () { $('#errorIndicator').hide(); }, 5000);
  }).ajaxComplete(function () {
    delayShow = false;
    $('#loadIndicator').hide();
    $('#delayIndicator').hide();
    clearInterval(timerId);
  });
  // bootstrap globals
  setMaxLengthBehavior();

  // task button  
  var replaceTaskButton = function (element) {
    $(element).removeClass('task');
    var r = template.replace('{{Title}}', $(element).data('text'));
    r = r.replace('{{Description}}', $(element).data('text'));
    if ($(element).data('disabled') != 'true') {
      r = r.replace('{{NavigateUrl}}', $(element).data('url'));
    } else {
      r = r.replace('{{NavigateUrl}}', '');
    }
    return r;
  };

  // Copy Task buttons to menu
  if ($('.task').length > 1) {
    var taskContainer = $('[data-template="topmenu-task-container"]');
    var template = taskContainer.get(0).outerHTML;
    var showDivider = ($('span.task[data-belowdivider="False"]').length > 0 && $('span.task[data-belowdivider="True"]').length > 0);
    $('span.task[data-belowdivider="False"]').each(function () {
      var r = replaceTaskButton($(this));
      $(taskContainer)
        .parents('ul')
        .css({ position: 'absolute', 'z-index': 5000 })
        .append(r);
    });
    if (showDivider) {
      $(taskContainer)
        .parents('ul')
        .css({ position: 'absolute', 'z-index': 5000 })
        .append($('<li class="divider">'));
    }
    $('span.task[data-belowdivider="True"]').each(function () {
      var r = replaceTaskButton($(this));
      $(taskContainer)
        .parents('ul')
        .css({ position: 'absolute', 'z-index': 5000 })
        .append(r);
    });
    $(taskContainer).parents('ul').find(':first').remove();
  } else {
    $('[data-taskdropdown]').hide();
  }
  // place on open
  $(document).on('shown.bs.dropdown', '[data-taskdropdown]', function () {
    var btn = $(this);
    var ul = $('ul[data-target="' + $(this).find('[data-target]').data('target') + '"');
    if ($(ul).find('li').length > 0) {
      var maxWidth = 0;
      $(ul).find('li').each(function (i, e) {
        var width = $(e).width();
        if (width > maxWidth) maxWidth = width;
      });
      ul.attr('style', 'left: ' + Math.ceil(-(maxWidth - btn.width())) + 'px !important'); // css does not understand !important  
    } else {
      ul.hide();
    }
    return false;
  });
  //$(document).on('hide.bs.dropdown', 'a', function () {
  //  return false;
  //});
  // globalization
  // Tell the validator what we want (in case we have one loaded)
  if ($.validator) {
    $.validator.methods.number = function (value) {
      if (Globalize.parseFloat(value, 10, GLOBAL_CULTURE) != NaN) {
        return true;
      }
      return false;
    };
    $.validator.methods.date = function (value) {
      if (Globalize.parseDate(value, null, GLOBAL_CULTURE)) {
        return true;
      }
      return false;
    };
    jQuery.extend(jQuery.validator.methods, {
      // Use the Globalization plugin to parse the value for [Range] annotation
      range: function (value, element, param) {
        var val = Globalize.parseFloat(value);
        return this.optional(element) || (val >= param[0] && val <= param[1]);
      }
    });
  }
  // help
  $(document).on('mouseover', '.fieldHelpIcon', function () {
    $().jguide().showBubble(this, {
      offset: {
        x: -15,
        y: 0
      },
      noScroll: true,
      header: $(this).data('header'),
      content: $(this).data('content'),
      arrow: ''
    });
  });
  $(document).on('mouseout', '.fieldHelpIcon', function () {
    $('.jguidebubble').fadeOut();
  });
  // special treatment for pages
  $('.page-nocolumns').each(function () {
    $(this).parent().removeClass('page-columns');
  });
  // relocate the page's menu item to the placeholder
  if ($('#menu-placeholder').length == 1) {
    if ($('menu[type=toolbar]').length == 1) {
      $('#menu-placeholder').append($('menu[type=toolbar]'));
    }
  }
  // special treatment for static footer
  $('.wrapper').css('min-height', 'auto');
  $('.wrapper').css('min-height', $('div.container > header').height() + $('div.container > div.wrapper').height() + $('footer').height());
  $('.culture-selection-logoff').click(function () {
    $(this).nextAll('.drop').show();
  });
  $('.culture-selection-logoff ~ .drop').on('mouseleave', function () {
    $(this).hide();
  });
  $('div#topmenu-task-action').on('click', function () {
    $('div#topmenu-task-container').show();
  });
  $('div#topmenu-task-container').on('mouseleave', function () {
    $(this).fadeOut();
  });
  // fill help section with content from view
  $('.helpSection').html($('script#helpSection').html());
  // start screen
  $('.lpcontent').hide();
  $('.lpcontent[data-item=beta]').show();
  $('td.mainlink').on('click', function (e) {
    $('.lpcontent').hide();
    var target = $(this).data('item');
    $('div.lpcontent[data-item=' + target + ']').fadeIn();
    e.preventDefault();
    return false;
  });
  $('.scroller').click(function () {
    var id = $(this).data('item');
    $('html, body').animate({ scrollTop: $("#" + id).offset().top - 150 }, 1000, 'easeOutCirc');
  });
  // Watermark
  //if blur and no value inside, set watermark text and class again.
  $(document).on('blur', 'input[data-watermark]', function () {
    if ($(this).val().length == 0) {
      $(this).val($(this).data('watermark')).addClass('watermark');
    }
  });
  //if focus and text is watermrk, set it to empty and remove the watermark class
  $(document).on('focus', 'input[data-watermark]', function () {
    if ($(this).val() == $(this).data('watermark')) {
      $(this).val('').removeClass('watermark');
    }
  });
  $('input[data-watermark]').each(function () {
    if ($(this).val() == '') {
      $(this).val($(this).data('watermark')).addClass('watermark');
    }
  });
  $(document).on('blur', 'textarea[data-watermark]', function () {
    if ($(this).text().length == 0) {
      $(this).text($(this).data('watermark')).addClass('watermark');
    }
  });
  //if focus and text is watermrk, set it to empty and remove the watermark class
  $(document).on('focus', 'textarea[data-watermark]', function () {
    if ($(this).text() == $(this).data('watermark')) {
      $(this).text('').removeClass('watermark');
    }
  });
  $('textarea').each(function () {
    if ($(this).val() == '') {
      $(this).text($(this).data('watermark')).addClass('watermark');
    }
  });
  if ($('#backButtonTemplate').html() != '') {
    $('h1').append($('#backButtonTemplate').html());
  }
  // show minimum reputation score hint for links in datagrids on request
  $(document).on('click mouseover', 'a.minRepDisabled', function () {
    $(this).next('div.minRepDisabled').show();
  });
  $(document).on('mouseout', 'a.minRepDisabled', function () {
    $(this).next('div.minRepDisabled').hide();
  });
  // handle the global hub functions, powered by SignalR
  $('#users_settings').dialog({
    autoOpen: false, resizable: false, width: '450', height: 'auto', modal: true, title: 'Chat Options',
    buttons: [
      {
        text: 'Save', click: function () {
          $(this).dialog("close");
        }
      },
      {
        text: 'Close', click: function () {
          $(this).dialog("close");
        }
      }
    ]
  });
  // spinner
  if ($("[data-spinner]").length > 0) {
    $("[data-spinner]").TouchSpin({
      min: 0,
      max: 100,
      step: 1,
      boostat: 5,
      maxboostedstep: 10,
      buttondown_class: 'btn btn-default',
      buttonup_class: 'btn btn-default'
    });
  }
  // token input
  if ($("[data-tokeninput]").length > 0) {
    prepareTokeInput();
  }
  
  // only if a file button has been found
  // file upload, trigger event to tweak UI
  $(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    var size = 0;
    for (var i = 0; i < input.get(0).files.length; i++) {
      size += input.get(0).files[i].size;
    }
    if (navigator.userAgent.indexOf('Chrome')) {
      label = label.replace(/C:\\fakepath\\/i, '');
    }
    input.trigger('fileselect', [numFiles, label, size]);
  });
  $(document).on('fileselect', '.btn-file :file', function (event, numFiles, label, size) {
    $($(this).data('filename')).text(label + " (" + size + " Bytes)");
  });
  if ($(':file').data('upload')) {
    $(document).on('click', $(':file').data('upload'), function(e) {
      var progress = $($(':file').data('progress'));
      var form = $(':file').data('form');
      var action = $(':file').data('action');
      if (progress)
        progress.show();
      uploadFile(
        form,
        action,
        function(progressvalue) {
          if (progress)
            progress.text(progress.text() + ".");
        },
        function(data) {
          if (progress)
            progress.hide();
          toastr.success(data.msg);
          $($(':file').data('imagesrc')).attr('src', data.src);
        },
        function(data) {
          toastr.error('Fail');
        });
      e.preventDefault();
      return false;
    });
  }
  // helpsection
  $('#helpsectiontrigger').on('click', function () {
    var hs = $('#helpsection');
    if (!hs.is(':visible')) {
      hs.fadeIn('slow').css({
        position: 'absolute',
        'margin-left': '-19px',
        'margin-top': '20px',
        'padding' : '5px 0 15px 15px',
        'background-color': 'white',
        'z-index': 5000,
        'width': '96.5%',
        'box-shadow': '2px 2px 5px'
    });
      $('#helpsectiontrigger').removeClass('icon-question-sign').addClass('icon-arrow-up');
    } else {
      hs.hide();
      $('#helpsectiontrigger').removeClass('icon-arrow-up').addClass('icon-question-sign');
    }
  });
  // chat
  $('#users_chat').dialog({
    autoOpen: false, resizable: false, width: '450', height: 'auto', modal: false, title: 'Private Chat', css: { zindex: 10000 },
    buttons: [
      {
        text: 'Send', click: function () {
          nav.server.sendChatMessageToUser($('#users_chat_peers').data('user-id'), $('#users_chat_message').val()).done(function () {
            $('#users_sent').show();
            setTimeout(function () { $('#users_sent').hide(); }, 2000);
          });
        }
      },
      {
        text: 'Close', click: function () {
          $(this).dialog("close");
        }
      }
    ]
  });
  $('#group_chat').dialog({
    autoOpen: false, resizable: false, width: '450', height: 'auto', modal: false, title: 'Group Chat', css: { zindex: 10000 },
    buttons: [
      {
        text: 'Send', click: function () {
          nav.server.sendChatMessageToGroup($('#group_chat_peers').data('group-id'), $('#group_chat_message').val()).done(function () {
            $('#group_sent').show();
            setTimeout(function () { $('#group_sent').hide(); }, 2000);
          });
        }
      },
      {
        text: 'Close', click: function () {
          $(this).dialog("close");
        }
      }
    ]
  });
  $('#team_chat').dialog({
    autoOpen: false, resizable: false, width: '450', height: 'auto', modal: false, title: 'Team Chat', css: { zindex: 10000 },
    buttons: [
      {
        text: 'Send', click: function () {
          nav.server.sendChatMessageToTeam($('#team_chat_peers').data('team-id'), $('#team_chat_message').val()).done(function () {
            $('#team_sent').show();
            setTimeout(function () { $('#team_sent').hide(); }, 2000);
          });
        }
      },
      {
        text: 'Close', click: function () {
          $(this).dialog("close");
        }
      }
    ]
  });
  var setUsers = function (users) {
    if (!users) return;

    function appendUsers(target, set) {
      for (var i = 0; i < set.length; i++) {
        $('<div class="chat_user">')
          .text(set[i].UserName)
          .attr('data-user', set[i].UserName) // for display
          .attr('data-user-id', set[i].UserId)     // for messages
          .addClass(set[i].IsConnected ? 'texxtoorGreen' : 'texxtoorRed')
          .appendTo(target);
      }
    }

    function joinGroup(id) {
      // we assume that 
      nav.server.joinGroup(id);
    }

    function joinTeam(id) {
      nav.server.joinTeam(id);
    }

    if ($('#users').length == 1) {
      var container = $('#users');
      container.empty();
      var h4 = $('<h4>').text('Chat').appendTo(container);
      $('<img>').attr('src', '/Content/icons/settings_16_2.png').click(function () {
        $('#users_settings').dialog('open');
      }).appendTo(h4);
      var a = $('<div>').html("<b>All</b>").appendTo(container);
      var t = $('<div>').html("<b>Teams</b>").appendTo(container);
      var g = $('<div>').html("<b>Groups</b>").appendTo(container);
      var currentUser = users.CurrentUser;
      $('#chatusers').text(users.Total + "/" + users.TotalOnline);
      for (var prop in users.Users) {
        var set = users.Users[prop];
        var areas = prop.split('|');
        var group;
        switch (areas[0]) {
          case "All":
            group = $('<div>').appendTo(a);
            $('<div class="chat_all">').text(areas[1]).appendTo(group);
            appendUsers(group, set);
            break;
          case "Group":
            group = $('<div>').appendTo(g);
            $('<div class="chat_group">').text(areas[1]).attr('data-group-id', areas[2]).appendTo(group);
            appendUsers(group, set);
            joinGroup(areas[2], currentUser);
            break;
          case "Team":
            group = $('<div>').appendTo(t);
            $('<div class="chat_team">').text(areas[1]).attr('data-team-id', areas[2]).appendTo(group);
            appendUsers(group, set);
            joinTeam(areas[2], currentUser);
            break;
        }
      }
      // attach private chats
      $('.chat_user').on('mouseover', function () {
        $(this).data('text', $(this).text());
        $(this).text('Chat with ' + $(this).data('user')).css({
          "border": "1px solid white",
          "cursor": "pointer"
        });
      });
      $('.chat_user').on('mouseout', function () {
        $(this).text($(this).data('text')).css({
          "border": "1px solid transparent",
          "cursor": "none"
        });
      });
      $('.chat_user').on('click', function () {
        $('#users_chat_peers').text($(this).data('user')).attr('data-user-id', $(this).data('user-id'));
        $('#users_chat').dialog('open');
      });
      // attach group chats
      $('.chat_group').on('mouseover', function () {
        $(this).data('text', $(this).text());
        $(this).text('Chat with members').css({
          "border": "1px solid white",
          "cursor": "pointer"
        });
      });
      $('.chat_group').on('mouseout', function () {
        $(this).text($(this).data('text')).css({
          "border": "1px solid transparent",
          "cursor": "none"
        });
      });
      $('.chat_group').on('click', function () {
        var peers = "";
        $(this).siblings('div').each(function (i, e) {
          peers += $(e).data('user') + ", ";
        });
        // show members and remember the group id
        $('#group_chat_peers').text(peers.slice(0, -2)).attr('data-group-id', $(this).data('group-id'));
        $('#group_chat').dialog('open');
      });
      // attach team chats
      $('.chat_team').on('mouseover', function () {
        $(this).data('text', $(this).text());
        $(this).text('Chat with team').css({
          "border": "1px solid white",
          "cursor": "pointer"
        });
      });
      $('.chat_team').on('mouseout', function () {
        $(this).text($(this).data('text')).css({
          "border": "1px solid transparent",
          "cursor": "none"
        });
      });
      $('.chat_team').on('click', function () {
        var peers = "";
        $(this).siblings('div').each(function (i, e) {
          peers += $(e).data('user') + ", ";
        });
        // show members and remember the group id
        $('#team_chat_peers').text(peers.slice(0, -2)).attr('data-team-id', $(this).data('team-id'));
        $('#team_chat').dialog('open');
      });
    }
  };
  // Reference the auto-generated proxy for the hub.      
  //var nav = $.connection && $.connection.notifications;
  //// only in case base scripts are loaded
  //if (nav) {
  //  $.extend(nav.client, {
  //    // Create a function that the hub can call back to display messages.
  //    applyTileValue: function (val) {
  //      // Add the message to the page. 
  //      $('div.hub[data-hubtarget="' + val.Target + '"]').html(val.Value);
  //    },
  //    reset: function () {
  //      $('div.hub[data-hubtarget]').empty();
  //    },
  //    setUserOnline: function (users) {
  //      setUsers(users);
  //    },
  //    setProductionProgress: function (percent, message) {
  //      if ($('#productionProgress').length == 1) {
  //        // only if target is available
  //        $('#productionProgress').progressbar("value", percent);
  //        $('#productionProgressMessage').html(message);
  //      }
  //    },
  //    // global chat support (if the dialog isn't open but user is online the dialog will always pop out
  //    chatMessage: function (senderId, sender, message) {
  //      $('#users_chat').dialog('open');
  //      $('#users_chat_peers').data('user-id', senderId);
  //      $('<p>').text(sender + ':').appendTo($('#users_chat_history'));
  //      $('<div>').text(message).appendTo($('#users_chat_history'));
  //      $('<hr>').appendTo($('#users_chat_history'));
  //    }
  //  });
  //  // call the server for very first time
  //  $.connection.hub.start().done(function () {
  //    nav.server.getTileValues().done(function () {
  //      // TODO: Set initial values
  //    });
  //    nav.server.getUserOnline().done(function (users) {
  //      setUsers(users);
  //    });
  //  });
  //  // endless keep the connection alive by transient reconnect
  //  $.connection.hub.disconnected(function () {
  //    setTimeout(function () {
  //      $.connection.hub.start();
  //    }, 5000); // Restart connection after 5 seconds.
  //  });
  //}
  loadFavorites();
}); // end ondocumentready

function getRegions(id, propertyId, selected) {
  if (!id) return;
  $.ajax({
    url: '/Tools/RegionForCountry',
    data: {
      id: id
    },
    type: 'GET',
    dataType: 'json',
    success: function (data) {
      var selBox = $('#' + propertyId).empty();
      $.each(data.q, function (i, e) {
        var option = $('<option>').attr('value', e).text(e).appendTo(selBox);
        if (e == selected) {
          option.attr('selected', 'selected');
        }
      });
    }
  });
}

function jump(h) {
  var url = location.href;                 //Save down the URL without hash.
  location.href = "#" + h;                 //Go to the target element.
  if (history.replaceState) {
    history.replaceState(null, null, url); //Don't like hashes. Changing it back.
  }
  var top = document.getElementById(h).offsetTop; //Getting Y of target element
  window.scrollTo(0, top);
}

function jumpAbsolute(h) {
  window.location.href = h;
}

function loadFavorites() {
  var $this = this;
  $.ajax({
    url: '/Home/Favorites',
    type: 'GET',
    cache: false,
    dataType: 'html'
  }).done(function (data) {
    $('#favorites').html(data);
  });
}

function addFavorite(id, title, model) {
  var $this = this;
  $.ajax({
    url: '/Home/AddFavorite',
    data: {
      id: id,
      title: title,
      model: model
    },
    type: 'POST',
    dataType: 'json'
  }).done(function (data) {
    toastr.success(data.msg);
    loadFavorites();
  });
}

function removeFavorite(key) {
  var $this = this;
  $.ajax({
    url: '/Home/RemoveFavorite',
    data: {
      key: key
    },
    type: 'POST',
    dataType: 'json'
  }).done(function (data) {
    toastr.success(data.msg);
    loadFavorites();
  });
}

function prepareTokeInput(prePopulateItems) {
  $("[data-tokeninput]").each(function (i, e) {
    var target = $(e);
    var url = target.data('url');
    var minchars = target.data('minchars');
    var theme = target.data('theme');
    target.tokenInput(url, {
      minChars: minchars,
      theme: theme,
      preventDuplicates: true,
      prePopulate: prePopulateItems
    });
    target.on('keypress', false);
  });
}

function setMaxLengthBehavior() {
  $('[type="text"][data-val-length-max]').each(function (i, e) {
    $(e).attr('maxlength', $(e).data('val-length-max'));
  });
  $('textarea[data-val-length-max]').each(function (i, e) {
    $(e).attr('maxlength', $(e).data('val-length-max'));
  });
  if ($('[maxlength]').length > 0) {
    $('[maxlength]').maxlength({
      treshold: 3,
      placement: 'bottom-right-inset',
      alwaysShow: true,
      warningClass: 'maxlength maxlength-warning',
      limitReachedClass: 'maxlength maxlength-reached'
    });
  }
}

function uploadFile(formSel, url, progress, success, error) {
  var formData = new FormData($(formSel)[0]);
  $.ajax({
    url: url,  //server script to process data
    type: 'POST',
    xhr: function () {  // custom xhr
      myXhr = $.ajaxSettings.xhr();
      if (myXhr.upload) { // if upload property exists
        myXhr.upload.addEventListener('progress', progress, false); // progressbar
      }
      return myXhr;
    },
    //Ajax events
    success: function (data) {
      success(data);
    },
    error: function (data) {
      error(data);
    },
    // Form data
    data: formData,
    //Options to tell JQuery not to process data or worry about content-type
    cache: false,
    contentType: false,
    processData: false
  }, 'json');
}