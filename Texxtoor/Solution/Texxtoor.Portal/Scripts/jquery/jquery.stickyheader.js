// Item Name: Sticky Header
// Author: Mapalla
// Author URI: http://codecanyon.net/user/Mapalla
// Version: 1.1

(function ($) {

  //start of plugin
  $.fn.stickyheader = function (options) {

    var defaults = { minimize: false, isClick: true };

    var o = jQuery.extend(defaults, options);

    return this.each(function () {

      var sh = $(this);
      sh.minimize = o.minimize;
      sh.isClick = o.isClick;

      sh.fullHeight = sh.outerHeight();

      sh.dropContent = sh.find('.dropcontent');
      sh.fullWidthDropContent = sh.find('.fullwidthdropcontent');
      sh.dropDown = sh.find('.dropdown');

      //hide content
      sh.dropContent.css('left', 'auto').hide();
      sh.fullWidthDropContent.css('left', '0').hide();
      sh.dropDown.css('display', 'block').hide();

      if (sh.isClick == true) {

        var menuList = sh.children("#menu").children('li');

        //click
        menuList.click(function (e) {
          var $this = $(this);
          var $thisDropContent = $this.children('.dropcontent');
          var $thisFullWidthDropContent = $this.children('.fullwidthdropcontent');
          var $thisDropDown = $this.children(".dropdown");

          var hidden_dropcontent = $thisDropContent.is(':hidden');
          var hidden_fullwidthdropcontent = $thisFullWidthDropContent.is(':hidden');
          var hidden_dropdown = $thisDropDown.is(":hidden");

          sh.dropContent.hide();
          sh.fullWidthDropContent.hide();
          sh.dropDown.hide();
          sh.children('#menu').children('li').removeClass('click');

          if (hidden_dropcontent) {
            $thisDropContent.show();
            $this.addClass('click');
          }

          if (hidden_fullwidthdropcontent) {
            $thisFullWidthDropContent.show();
            $this.addClass('click');
          }

          if (hidden_dropdown) {
            $thisDropDown.show();
            $this.addClass('click');
          }

          e.stopPropagation();

        });

        sh.dropContent.click(function (e) {
          e.stopPropagation();
        });

        sh.fullWidthDropContent.click(function (e) {
          e.stopPropagation();
        });

        sh.dropDown.click(function (e) {
          e.stopPropagation();
        });

        $(document).click(function () {
          sh.dropContent.hide();
          sh.fullWidthDropContent.hide();
          sh.dropDown.hide();
          sh.children('#menu').children('li').removeClass('click');
        });

      }
      else {
        //hover
        $('li').hover(
            function () {
              var $this = $(this);
              $this.children('ul').show();
              $this.children('div').show();
            },
            function () {
              var $this = $(this);
              setTimeout(function() {
                $this.children('ul').hide();
                $this.children('div').hide();
              }, 500);
            });
      }


    }); // end of return

  }; //end of plugin

  //function here 

})(jQuery);
