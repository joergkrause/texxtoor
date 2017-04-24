/*
Copyright (c) 2009 Mikael Söderström.
Contact: vimpyboy@msn.com

Feel free to use this script as long as you don't remove this comment.

Modified by Joerg Krause, 2012
Changes: 
* Added settings.baseUrl to support flexible install path
* Styles changed, static background images removed

*/

(function ($) {
  var isLoaded = false;
  var isClosed = false;

  $.fn.Ribbon = function (ribbonSettings) {
    var settings = $.extend({ theme: 'windows7', backStage: false, baseUrl: '' }, ribbonSettings || {});

    var setupMenu = function (setting) {
      var $this = this;
      $('.menu li ul').show();
      setTimeout(function () {
        // allow background images
        $('.ribbon-button > img').attr('src', '/App_Sprites/blank.gif');
        $('li a > img').attr('src', '/App_Sprites/blank.gif');
        // standard procedure
        $('.menu li ul').hide();
        var first = $('.menu li a:first');
        first.addClass('active');
        first.parent().children('ul:first').show();
        first.parent().children('ul:first').addClass('submenu');
        $('.menu li > a').click(function () { ShowSubMenu(this); });
        $('.orbButton').click(function () { ShowOrbMenu(); });
        $('.orb ul').hide();
        $('.orb ul ul').hide();
        $('.orb li ul li ul').hide();
        $('.orb li li ul').each(function () {
          $(this).before('<div style="background-color: #EBF2F7; display:none; height: 25px; line-height: 25px; position: absolute; left:216px; top:25px; width: 292px; padding-left: 9px; border-bottom: 1px solid #CFDBEB;">' + $(this).parent().children('a:first').text() + '</div>');
        });
        $('.orb li li a').each(function () {
          if ($(this).parent().children('ul').length > 0) {
            $(this).addClass('arrow');
          }
        });
        $(document).on('click', '.orb .ribbon-backstage a', function () { $('.orb ul').fadeOut('fast'); });

        //$('.ribbon-list div').each(function() { $(this).parent().width($(this).parent().width()); });

        $('.ribbon-list div').on('click', function (e) {
          var elwidth = $(this).parent().width();
          $(this).find('li').each(function () {
            elwidth = ($(this).width() > elwidth) ? $(this).width() : elwidth;
          });
          var insideX = e.pageX > $(this).offset().left && e.pageX < $(this).offset().left + $(this).width();
          var insideY = e.pageY > $(this).offset().top && e.pageY < $(this).offset().top + $(this).height();

          elwidth = $(this).children('ul').width() < elwidth ? elwidth : $(this).children('ul li').width();

          if (insideX && insideY) {
            $(this).children('ul').width(elwidth);
            if (!$(this).children('ul').is(":visible")) {
              $('.ribbon-list div ul').hide();
              $(this).children('ul').fadeIn('slow', function () {
                setTimeout(function () {
                  $('body').bind('mousedown', function () {
                    $('.ribbon-list div ul').fadeOut('slow');
                    $('body').unbind('mousedown');
                  });
                }, 100);
              });
            } else $(this).children('ul').fadeOut('slow');

          }
        });


        $('.ribbon-list div').parents().click(function (e) {
          var vis = $(this).parent().offset();
          var elm = $(this).parent();
          //var outsideX = e.pageX < $('.ribbon-list div ul' + vis).parent().offset().left || e.pageX > $('.ribbon-list div ul' + vis).parent().offset().left + $('.ribbon-list div ul' + vis).parent().width();
          //var outsideY = e.pageY < $('.ribbon-list div ul' + vis).parent().offset().top || e.pageY > $('.ribbon-list div ul' + vis).parent().offset().top + $('.ribbon-list div ul' + vis).parent().height();
          var outsideX = vis ? e.pageX < vis.left || e.pageX > vis.left + elm.width() : true;
          var outsideY = vis ? e.pageY < vis.top || e.pageY > vis.top + elm.height() : true;

          if (outsideX || outsideY) {
            $('.ribbon-list div ul').each(function () {
              $(this).parent().removeAttr('style'); //'background-image', '');
            });
          }
          //return false;
        });
        $('.ribbon-list div ul').fadeOut('fast');
        $('.orb li li a').mouseover(function () { ShowOrbChildren(this); });

        $('.menu li > a').dblclick(function () {
          $('ul .submenu').hide();
          $('body').animate({ paddingTop: $(this).parent().parent().parent().parent().height() }, function () {
            AUTHOR.resize();
          });
          isClosed = true;
        });
        $('body').css({ paddingTop: 27 + 'px' });
        $('ul .submenu').hide();
        // finally open the first tab by default
        ShowSubMenu($('.menu li a:first'));
      }, 100);
    };

    $('.ribbon a').each(function () {
      if ($(this).attr('accesskey')) {
        $(this).append('<div rel="accesskeyhelper" style="display: none; position: absolute; background-repeat: none; width: 16px; padding: 0px; text-align: center; height: 17px; line-height: 17px; top: ' + $(this).offset().top + 'px; left: ' + ($(this).offset().left + $(this).width() - 15) + 'px;">' + $(this).attr('accesskey') + '</div>');
      }
    });

    $('head').append('<link href="' + settings.baseUrl + 'ribbon/themes/' + settings.theme + '/ribbon.css" rel="stylesheet" type="text/css" />" />');

    if (!isLoaded) {
      setupMenu(settings);
    } else {
    }

    $(document).keydown(function (e) { ShowAccessKeys(e); });
    $(document).keyup(function (e) { HideAccessKeys(e); });

    $('.ribbon').parents().click(function (e) {
      var outsideX = e.pageX < $('.orb ul:first').offset().left || e.pageX > $('.orb ul:first').offset().left + $('.orb ul:first').width();
      var outsideY = e.pageY < $('.orb ul:first img:first').offset().top || e.pageY > $('.orb ul:first').offset().top + $('.orb ul:first').height();

      if ((outsideX || outsideY) && !settings.backStage) {
        $('.orb ul').fadeOut('fast');
      }
    });

    if (isLoaded) {
      $('.orb li:first ul:first img:first').remove();
      $('.orb li:first ul:first img:last').remove();
      $('.ribbon-list div img[src*="' + settings.baseUrl + '/images/arrow_down.png"]').remove();
    }

    if (!settings.backStage) {
      $('.orb li:first ul:first').append('<img src="/App_Sprites/blank.gif" style="margin-left: -10px; margin-bottom: -22px; width: 96px; height: 24px" />');
      $('.orb li:first ul:first').prepend('<img src="/App_Sprites/blank.gif" style="margin-left: -10px; margin-top: -22px; width: 96px; height: 24px" />');
    }

    $('.ribbon-list div').each(function () {
      if ($(this).children('ul').length > 0) {
        //$(this).css('float', 'left');
        $(this).append('<img src="' + settings.baseUrl + 'ribbon/themes/' + settings.theme + '/images/arrow_down.png" style="float: right; margin-top: 7px;" />');
      }
    });

    //Hack for IE 7.
    if (navigator.appVersion.indexOf('MSIE 6.0') > -1 || navigator.appVersion.indexOf('MSIE 7.0') > -1) {
      $('ul.menu li li div').css('width', '90px');
      $('ul.menu').css('width', '500px');
      $('ul.menu').css('float', 'left');
      $('ul.menu .submenu li div.ribbon-list').css('width', '100px');
      $('ul.menu .submenu li div.ribbon-list div').css('width', '100px');
    }

    $('a[href=' + window.location.hash + ']').click();

    //Add backstage class
    if (settings.backStage) {
      $('ul.ribbon .orb > li > ul').addClass('ribbon-backstage');
      $('ul.ribbon-backstage').width('3000px');
      $('ul.ribbon-backstage').height($(document).height());

      $('ul.ribbon-backstage > li').width('130px');
      $('ul.ribbon-backstage > li > a').width('125px');

      $('ul.ribbon-backstage > li').addClass('ribbon-backstage-firstLevel');

      $('ul.ribbon-backstage .ribbon-backstage-firstLevel > ul').addClass('ribbon-backstage-subMenu');

      $('ul.ribbon-backstage .ribbon-backstage-firstLevel .ribbon-backstage-subMenu > div').addClass('ribbon-backstage-subMenu-header');
    }

    $('.ribbon-backstage-rightColumnWide').hide();
    $('.ribbon-backstage-rightColumnSmall').hide();
    isClosed = true;
    isLoaded = true;

    function ResetSubMenu() {
      $('.menu li a').removeClass('active');
      $('.menu ul').removeClass('submenu');
      $('.menu li ul').hide();
    }

    function ShowSubMenu(e) {
      $('.orb ul').fadeOut('fast');
      var isActive = $(e).next().css('display') == 'block';
      ResetSubMenu();

      $(e).addClass('active');
      $(e).parent().children('ul:first').addClass('submenu');
      $(e).parent().children('ul:first').show();
      $('body').css('padding-top', '120px');
      if (isClosed) {
        // resize
      }
      isClosed = false;
    }

    function ShowOrbChildren(e, init) {
      if (!settings.backStage) {
        if (($(e).parent().children('ul').css('display') == 'none' || $(e).parent().children('ul').length == 0) && $(e).parent().parent().parent().parent().hasClass('orb')) {
          $('.orb li li ul').fadeOut('fast');
          if ($(e).parent().children('ul').length > 0) {
            $(e).parent().children('ul').fadeIn('fast');
            $(e).parent().children('div').fadeIn('fast');
          }
        }
      }

      if (settings.backStage) {
        if ($(e).parent().children('ul').length == 0 && $('ul.ribbon-backstage .ribbon-backstage-firstLevel .ribbon-backstage-subMenu:first:hidden').length > 0 && $(e).parent().parent().parent().parent().hasClass('orb')) {
          $('.orb li li ul').fadeOut('fast');
          $('.orb li li > div').fadeOut('fast');
          $('ul.ribbon-backstage .ribbon-backstage-firstLevel:first .ribbon-backstage-subMenu:first').fadeIn('fast');
          $('ul.ribbon-backstage .ribbon-backstage-firstLevel:first div').fadeIn('fast');
        }

        if ($(e).parent().children('ul').css('display') == 'none' && $(e).parent().children('ul').length > 0 && $(e).parent().parent().parent().parent().hasClass('orb')) {
          $('.orb li li ul').fadeOut('fast');
          $('.orb li li > div').fadeOut('fast');
          $(e).parent().children('ul').fadeIn('fast');
          $(e).parent().children('div').fadeIn('fast');
        }

        if (init) {
          $('.orb li li ul').hide();
          $('.orb li li > div').hide();
          $(e).parent().children('ul').fadeIn('fast');
          $(e).parent().children('div').fadeIn('fast');
        }
      }

    }

    function ShowOrbMenu() {
      //Show standard menu
      if (!settings.backStage) {
        $('.orb li > ul').animate({ opacity: 'toggle' }, 'fast');
        $('.orb li ul li ul').hide();
        return;
      }
      //Show backstage
      $('ul.ribbon-backstage .ribbon-backstage-firstLevel .ribbon-backstage-subMenu').height($(window).height() * 0.90);
      $('ul.ribbon-backstage .ribbon-backstage-firstLevel div.ribbon-backstage-rightColumnWide').height($(window).height() * 0.90).width($(window).width());
      $('ul.ribbon-backstage .ribbon-backstage-firstLevel div.ribbon-backstage-rightColumnSmall').parent().children('ul').width($(window).width()).addClass('ribbon-backstage-leftColumnWide');
      $('ul.ribbon-backstage .ribbon-backstage-firstLevel div.ribbon-backstage-rightColumnSmall').height($(window).height() * 0.90).width('200px');
      $('ul.ribbon-backstage .ribbon-backstage-firstLevel div.ribbon-backstage-rightColumnSmall').css('left', ($(window).width() - 200)).css('line-height', '17px');

      $('.orb > ul').animate({ opacity: 'toggle' }, 'fast');
      ShowOrbChildren($('.orb li li a:first'), true);
    }

    function ShowAccessKeys(e) {
      if (e.altKey) {
        $('div[rel="accesskeyhelper"]').each(function () { $(this).css('top', $(this).parent().offset().top).css('left', $(this).parent().offset().left - 20 + $(this).parent().width()); $(this).show(); });
      }
    }

    function HideAccessKeys(e) {
      $('div[rel="accesskeyhelper"]').hide();
    }
  };
})(jQuery);